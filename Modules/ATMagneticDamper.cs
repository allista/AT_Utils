//   ATMagneticDamper.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AT_Utils
{
    public class ATMagneticDamper : PartModule
    {
        [KSPField] public string DamperID = string.Empty;
        [KSPField] public string Sensor = string.Empty;
        [KSPField] public string MagnetLocation = string.Empty;
        [KSPField] public string AffectedPartTags = string.Empty;
        [KSPField] public bool AffectKerbals;
        [KSPField] public bool EnableControls = true;
        [KSPField] public float Attenuation = 0.5f;
        [KSPField] public float MaxForce = 100f;
        [KSPField] public float EnergyConsumptionK = 1f;
        [KSPField] public float ReactivateAfterSeconds = 5f;
        private double reactivateAtUT = -1;

        [KSPField(isPersistant = true)] public bool damperEnabled = true;
        [KSPField(isPersistant = true)] public bool magnetEnabled = true;
        protected Damper damper;
        protected ResourcePump socket;

        public bool HasDamper { get; private set; }
        public bool HasMagnet => HasDamper && damper.HasMagnet;

        public static ATMagneticDamper GetDamper(Part p, string id) =>
            string.IsNullOrEmpty(id)
                ? null
                : p.Modules.GetModules<ATMagneticDamper>()
                    .FirstOrDefault(d =>
                        !string.IsNullOrEmpty(d.DamperID) && d.DamperID.Equals(id));

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            HasDamper = false;
            Attenuation = Utils.Clamp(Attenuation, 0, 0.999f);
            EnergyConsumptionK = Utils.ClampL(EnergyConsumptionK, 1e-6f);
            if(!string.IsNullOrEmpty(Sensor))
            {
                var sensor = part.FindModelComponent<MeshFilter>(Sensor);
                if(sensor != null)
                {
                    sensor.AddCollider(true);
                    damper = sensor.gameObject.AddComponent<Damper>();
                    damper.Init(this);
                    damper.enabled = damperEnabled;
                    socket = part.CreateSocket();
                    HasDamper = true;
                }
            }
            Events[nameof(ToggleEvent)].active =
                HasDamper && EnableControls;
            Actions[nameof(ToggleAction)].active =
                HasDamper && EnableControls;
            Events[nameof(ToggleMagnetEvent)].active =
                HasDamper && EnableControls && damper.HasMagnet;
            Actions[nameof(ToggleMagnetAction)].active =
                HasDamper && EnableControls && damper.HasMagnet;
            updatePAW();
        }

        private void OnDestroy()
        {
            if(HasDamper)
                Destroy(damper);
        }

        private void onDamperWorking(float energy_spent) =>
            socket.RequestTransfer(energy_spent * EnergyConsumptionK * TimeWarp.fixedDeltaTime);

        private void FixedUpdate()
        {
            if(FlightDriver.Pause || !HasDamper || socket == null)
                return;
            if(!socket.TransferResource())
                return;
            if(socket.PartialTransfer)
            {
                damper.enabled = false;
                reactivateAtUT = Planetarium.GetUniversalTime() + ReactivateAfterSeconds;
                Utils.Message(ReactivateAfterSeconds,
                    $"[{part.Title()}] Damper deactivated due to lack of EC. Activating in {ReactivateAfterSeconds}");
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if(!HasDamper)
                return;
            if(damperEnabled
               && !damper.enabled
               && Planetarium.GetUniversalTime() > reactivateAtUT)
            {
                damper.enabled = true;
                Utils.Message($"[{part.Title()}] Damper reactivated"); //debug
            }
        }

        private void updatePAW()
        {
            Events[nameof(ToggleEvent)].guiName = damperEnabled
                ? "Disable Damper"
                : "Enable Damper";
            Events[nameof(ToggleMagnetEvent)].guiName = magnetEnabled
                ? "Disable Magnet"
                : "Enable Magnet";
        }

        public void Enable(bool enable = true)
        {
            if(!HasDamper || enable == damperEnabled)
                return;
            damper.enabled = damperEnabled = enable;
            updatePAW();
        }

        public void EnableMagnet(bool enable = true)
        {
            if(!HasDamper || enable == magnetEnabled)
                return;
            magnetEnabled = enable;
            updatePAW();
        }

        [KSPEvent(guiName = "Disable Damper",
            guiActive = true,
            active = true,
            guiActiveUnfocused = true,
            externalToEVAOnly = false,
            unfocusedRange = 50)]
        public void ToggleEvent(BaseEventDetails data) => Enable(!damperEnabled);

        [KSPAction(guiName = "Toggle Damper")]
        public void ToggleAction(KSPActionParam data) => Enable(!damperEnabled);

        [KSPEvent(guiName = "Disable Magnet",
            guiActive = true,
            active = true,
            guiActiveUnfocused = true,
            externalToEVAOnly = false,
            unfocusedRange = 50)]
        public void ToggleMagnetEvent(BaseEventDetails data) => EnableMagnet(!magnetEnabled);

        [KSPAction(guiName = "Toggle Magnet")]
        public void ToggleMagnetAction(KSPActionParam data) => EnableMagnet(!magnetEnabled);

        protected class Damper : MonoBehaviour
        {
            private ATMagneticDamper controller;
            private Transform magnet;
            private string[] tags;

            public bool HasMagnet => magnet != null;

            private struct VesselInfo
            {
                public uint id;
                public Vessel vessel;
                public Vector3 position;
                public float energy_consumption;
            }

            private readonly Dictionary<uint, VesselInfo> dampedVessels =
                new Dictionary<uint, VesselInfo>();

            public void Init(ATMagneticDamper damper_module)
            {
                controller = damper_module;
                if(!string.IsNullOrEmpty(controller.MagnetLocation))
                    magnet = controller.part.FindModelTransform(controller.MagnetLocation);
                if(!string.IsNullOrEmpty(controller.AffectedPartTags))
                    tags = Utils.ParseLine(controller.AffectedPartTags, Utils.Comma);
                controller.magnetEnabled = magnet != null;
            }

            private void FixedUpdate()
            {
                if(FlightDriver.Pause)
                    return;
                if(dampedVessels.Count == 0)
                    return;
                var remove_vessels = new List<uint>();
                foreach(var vsl_info in dampedVessels.Values)
                {
                    if(vsl_info.vessel != null && vsl_info.vessel.packed)
                    {
                        vsl_info.vessel.SetPosition(transform.TransformPoint(vsl_info.position));
                        if(vsl_info.energy_consumption > 0)
                            controller.onDamperWorking(vsl_info.energy_consumption);
                    }
                    else
                        remove_vessels.Add(vsl_info.id);
                }
                remove_vessels.ForEach(vsl_id => dampedVessels.Remove(vsl_id));
            }

            private void OnTriggerStay(Collider col)
            {
                if(!enabled
                   || controller == null
                   || controller.part == null
                   || controller.part.Rigidbody == null
                   || col == null
                   || col.attachedRigidbody == null)
                    return;
                if(!col.CompareTag("Untagged"))
                    return;
                var p = col.attachedRigidbody.GetComponent<Part>();
                if(p == null
                   || p.vessel == null
                   || p.vessel == controller.vessel
                   || !p.vessel.loaded)
                    return;
                if(p.vessel.isEVA && !controller.AffectKerbals)
                    return;
                if(tags != null && !tags.Any(t => p.partInfo.tags.Contains(t)))
                    return;
                if(!p.vessel.packed)
                {
                    var r = col.attachedRigidbody;
                    var h = controller.part.Rigidbody;
                    if(r == null || h == null)
                        return;
                    var total_energy = 0f;
                    // damp angular and linear velocity
                    if(controller.Attenuation > 0)
                    {
                        var dI = (controller.Attenuation * r.mass * (r.velocity - h.velocity))
                            .ClampMagnitudeH(controller.MaxForce * TimeWarp.fixedDeltaTime);
                        var dAv = controller.Attenuation * (h.angularVelocity - r.angularVelocity);
                        r.AddTorque(dAv, ForceMode.VelocityChange);
                        r.AddForce(-dI, ForceMode.Impulse);
                        h.AddForce(dI, ForceMode.Impulse);
                        if(controller.EnergyConsumptionK > 0)
                        {
                            total_energy += dI.magnitude;
                            total_energy += Vector3.Dot(dAv.AbsComponents(), r.inertiaTensor);
                        }
                    }
                    // add force to attract the part to magnet's center
                    if(controller.magnetEnabled && magnet != null)
                    {
                        var d = magnet.position - r.worldCenterOfMass;
                        var attraction = (d.sqrMagnitude > 1 ? d.normalized : d) * r.mass;
                        r.AddForce(attraction, ForceMode.Force);
                        h.AddForce(-attraction, ForceMode.Force);
                        if(controller.EnergyConsumptionK > 0)
                            total_energy += attraction.magnitude * TimeWarp.fixedDeltaTime;
                    }
                    if(total_energy > 0)
                        controller.onDamperWorking(total_energy);
                }
                else
                {
                    if(!dampedVessels.ContainsKey(p.vessel.persistentId))
                    {
                        dampedVessels[p.vessel.persistentId] = new VesselInfo
                        {
                            id = p.vessel.persistentId,
                            vessel = p.vessel,
                            position = transform
                                .InverseTransformPoint(p.vessel.vesselTransform.position),
                            energy_consumption = p.TotalMass() * 0.01f
                        };
                    }
                }
            }
        }
    }
}

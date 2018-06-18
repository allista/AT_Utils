//   ATMagneticDamper.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;

namespace AT_Utils
{
    public class ATMagneticDamper : PartModule
    {
        [KSPField] public string Sensor = string.Empty;
        [KSPField] public string MagnetLocation = string.Empty;
        [KSPField] public string AffectedPartTags = string.Empty;
        [KSPField] public float Attenuation = 0.5f;
        protected Damper damper;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if(!string.IsNullOrEmpty(Sensor))
            {
                var sensor = part.FindModelComponent<MeshFilter>(Sensor);
                if(sensor != null)
                {
                    sensor.AddCollider(true);
                    damper = sensor.gameObject.AddComponent<Damper>();
                    damper.Init(part, Attenuation, MagnetLocation, AffectedPartTags);
                }
            }
        }

        public void Enable(bool enable = true)
        {
            if(damper != null)
                damper.enabled = enable;
        }

        protected class Damper : MonoBehaviour
        {
            Part host;
            Transform magnet;
            string[] tags;
            public float K;

            public void Init(Part part, float attenuation, string magnet = null, string tags = "")
            {
                host = part;
                K = 1 - Utils.Clamp(attenuation, 0, 0.999f);
                if(!string.IsNullOrEmpty(magnet))
                    this.magnet = host.FindModelTransform(magnet);
                if(this.magnet == null)
                    this.magnet = transform;
                if(!string.IsNullOrEmpty(tags))
                    this.tags = Utils.ParseLine(tags, Utils.Comma);
            }

            void OnTriggerStay(Collider col)
            {
                if(host != null && host.Rigidbody != null
                   && col != null && col.attachedRigidbody != null)
                {
                    if(col.CompareTag("Untagged"))
                    {
                        var p = col.attachedRigidbody.GetComponent<Part>();
                        if(p != null && p.vessel != null && p.vessel.loaded)
                        {
                            if(tags != null && tags.Length > 0)
                            {
                                var tagged = false;
                                foreach(var t in tags)
                                {
                                    if(p.partInfo.tags.Contains(t))
                                    {
                                        tagged = true;
                                        break;
                                    }
                                }
                                if(!tagged)
                                    return;
                            }
                            var r = p.Rigidbody;
                            var h = host.Rigidbody;
                            if(r != null)
                            {
                                // damp angular and linear velocity
                                var dI = (r.velocity - h.velocity) * r.mass * K;
                                var dAv = (h.angularVelocity - r.angularVelocity) * K;
                                r.AddTorque(dAv, ForceMode.VelocityChange);
                                r.AddForce(-dI, ForceMode.Impulse);
                                h.AddForce(dI, ForceMode.Impulse);
                                // add force to attract the part to magnet's center
                                if(magnet != null)
                                {
                                    var d = magnet.position - r.worldCenterOfMass;
                                    var attraction = (d.sqrMagnitude > 1? d.normalized : d) * r.mass;
                                    r.AddForce(attraction, ForceMode.Force);
                                    h.AddForce(-attraction, ForceMode.Force);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
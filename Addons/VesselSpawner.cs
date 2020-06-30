//   VesselSpawner.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;
using KSP.UI.Screens;
using UnityEngine;

namespace AT_Utils
{
    public class VesselSpawner : MonoBehaviour
    {
        public class WaitForLaunchInstruction : CustomYieldInstruction
        {
            private readonly VesselSpawner Spawner;
            public WaitForLaunchInstruction(VesselSpawner spawner) => Spawner = spawner;
            public override bool keepWaiting => Spawner != null && Spawner.LaunchInProgress;
        }

        private Part part;
        private Vessel vessel => part.vessel;

        public bool LaunchInProgress { get; private set; }
        private Coroutine launch_program;
        private Vessel launched_vessel;
        private readonly List<Collider> disabled_launched_colliders = new List<Collider>();
        private Action onFixedUpdate;
        private Callback<Vessel> onVesselLoaded, onVesselOffRails;

        public void BeginLaunch() => LaunchInProgress = true;
        public void AbortLaunch() => end_launch();

        public WaitForLaunchInstruction WaitForLaunch => new WaitForLaunchInstruction(this);

        public void Init(Part p)
        {
            part = p;
        }

        private void Awake()
        {
            GameEvents.onVesselLoaded.Add(onLaunchedVesselLoaded);
            GameEvents.onVesselGoOffRails.Add(onLaunchedVesselOffRails);
        }

        private void OnDestroy()
        {
            GameEvents.onVesselLoaded.Remove(onLaunchedVesselLoaded);
            GameEvents.onVesselGoOffRails.Remove(onLaunchedVesselOffRails);
        }

        private void onLaunchedVesselLoaded(Vessel vsl)
        {
            if(launched_vessel != vsl || launched_vessel == null)
                return;
            onVesselLoaded?.Invoke(launched_vessel);
        }

        private void onLaunchedVesselOffRails(Vessel vsl)
        {
            if(launched_vessel != vsl || launched_vessel == null)
                return;
            onVesselOffRails?.Invoke(launched_vessel);
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }

        private IEnumerator<YieldInstruction> run_program(
            IEnumerator program,
            Callback<Vessel> on_vessel_launched
        )
        {
            yield return StartCoroutine(program);
            on_vessel_launched?.Invoke(launched_vessel);
            end_launch();
            Utils.Log("Launch Ended");
        }

        private void start_program(
            IEnumerator program,
            Callback<Vessel> on_vessel_loaded,
            Callback<Vessel> on_vessel_off_rails,
            Callback<Vessel> on_vessel_launched
        )
        {
            Utils.Log("Begin Launch");
            if(launch_program != null)
                StopCoroutine(launch_program);
            LaunchInProgress = true;
            onVesselLoaded = on_vessel_loaded;
            onVesselOffRails = on_vessel_off_rails;
            disabled_launched_colliders.Clear();
            FlightCameraOverride.AnchorForSeconds(FlightCameraOverride.Mode.Hold,
                FlightGlobals.ActiveVessel.transform,
                1);
            launch_program = StartCoroutine(run_program(program, on_vessel_launched));
        }

        private void end_launch()
        {
            launched_vessel = null;
            disabled_launched_colliders.Clear();
            onVesselLoaded = null;
            onVesselOffRails = null;
            launch_program = null;
            onFixedUpdate = null;
            LaunchInProgress = false;
        }

        public void SpawnShipConstructToGround(
            ShipConstruct construct,
            Transform spawn_transform,
            Vector3 spawn_offset,
            Callback<Vessel> on_vessel_positioned = null,
            Callback<Vessel> on_vessel_loaded = null,
            Callback<Vessel> on_vessel_off_rails = null,
            Callback<Vessel> on_vessel_launched = null,
            int easing_frames = 0
        )
        {
            start_program(SpawnShipConstructToGround_program(construct,
                    spawn_transform,
                    spawn_offset,
                    on_vessel_positioned,
                    easing_frames),
                on_vessel_loaded,
                on_vessel_off_rails,
                on_vessel_launched);
        }

        private IEnumerator<YieldInstruction> SpawnShipConstructToGround_program(
            ShipConstruct construct,
            Transform spawn_transform,
            Vector3 spawn_offset,
            Callback<Vessel> on_vessel_positioned = null,
            int easing_frames = 0
        )
        {
            PutShipToGround(construct, spawn_transform, spawn_offset);
            ShipConstruction.AssembleForLaunch(construct,
                vessel.landedAt,
                vessel.displaylandedAt,
                part.flagURL,
                FlightDriver.FlightStateCache,
                new VesselCrewManifest());
            launched_vessel = FlightGlobals.Vessels[FlightGlobals.Vessels.Count - 1];
            on_vessel_positioned?.Invoke(launched_vessel);
            //AssembleForLaunch calls Vessel.Initialize which sets Vessel.loaded = true
            //so the onVesselLoaded GameEvent is never fired
            onVesselLoaded?.Invoke(launched_vessel);
            while(launched_vessel.packed)
            {
                launched_vessel.precalc.isEasingGravity = true;
                launched_vessel.situation = Vessel.Situations.PRELAUNCH;
                stabilize_launched_vessel(0f);
                FlightCameraOverride.UpdateDurationSeconds(1);
                yield return new WaitForFixedUpdate();
            }
            if(easing_frames > 0)
            {
                foreach(var n in stabilize_launched_vessel(easing_frames))
                {
                    FlightCameraOverride.UpdateDurationSeconds(1);
                    yield return new WaitForFixedUpdate();
                }
            }
            StageManager.BeginFlight();
        }

        public void SpawnShipConstruct(
            ShipConstruct construct,
            Transform spawn_transform,
            Vector3 spawn_offset,
            Vector3 dV,
            Callback<Vessel> on_vessel_positioned = null,
            Callback<Vessel> on_vessel_loaded = null,
            Callback<Vessel> on_vessel_off_rails = null,
            Callback<Vessel> on_vessel_launched = null
        )
        {
            start_program(SpawnShipConstruct_program(construct,
                    spawn_transform,
                    spawn_offset,
                    dV,
                    on_vessel_positioned),
                on_vessel_loaded,
                on_vessel_off_rails,
                on_vessel_launched);
        }

        private IEnumerator<YieldInstruction> SpawnShipConstruct_program(
            ShipConstruct construct,
            Transform spawn_transform,
            Vector3 spawn_offset,
            Vector3 dV,
            Callback<Vessel> on_vessel_positioned = null
        )
        {
            float angle;
            Vector3 axis;
            spawn_transform.rotation.ToAngleAxis(out angle, out axis);
            var root = construct.parts[0].localRoot.transform;
            root.Translate(spawn_transform.TransformPointUnscaled(spawn_offset), Space.World);
            root.RotateAround(spawn_transform.position, axis, angle);
            //initialize new vessel
            launched_vessel = AssembleForLaunchUnlanded(construct,
                new Orbit(vessel.orbit),
                part.flagURL,
                FlightDriver.FlightStateCache);
            on_vessel_positioned?.Invoke(launched_vessel);
            //AssembleForLaunchUnlanded calls Vessel.Initialize which sets Vessel.loaded = true
            //so the onVesselLoaded GameEvent is never fired
            onVesselLoaded?.Invoke(launched_vessel);
            //launch the vessel
            yield return StartCoroutine(launch_moving_vessel(spawn_transform,
                spawn_offset,
                spawn_transform.rotation.Inverse() * root.rotation,
                dV));
            StageManager.BeginFlight();
        }

        public void SpawnProtoVessel(
            ProtoVessel proto_vessel,
            Transform spawn_transform,
            Vector3 spawn_offset,
            Vector3 dV,
            Callback<ProtoVessel> on_proto_vessel_positioned = null,
            Callback<Vessel> on_vessel_positioned = null,
            Callback<Vessel> on_vessel_loaded = null,
            Callback<Vessel> on_vessel_off_rails = null,
            Callback<Vessel> on_vessel_launched = null
        )
        {
            start_program(SpawnProtoVessel_program(proto_vessel,
                    spawn_transform,
                    spawn_offset,
                    dV,
                    on_proto_vessel_positioned,
                    on_vessel_positioned),
                on_vessel_loaded,
                on_vessel_off_rails,
                on_vessel_launched);
        }

        private IEnumerator<YieldInstruction> SpawnProtoVessel_program(
            ProtoVessel proto_vessel,
            Transform spawn_transform,
            Vector3 spawn_offset,
            Vector3 dV,
            Callback<ProtoVessel> on_proto_vessel_positioned = null,
            Callback<Vessel> on_vessel_positioned = null
        )
        {
            position_proto_vessel(proto_vessel, spawn_transform, spawn_offset);
            on_proto_vessel_positioned?.Invoke(proto_vessel);
            proto_vessel.Load(HighLogic.CurrentGame.flightState);
            launched_vessel = proto_vessel.vesselRef;
            launched_vessel.orbitDriver.updateMode = OrbitDriver.UpdateMode.TRACK_Phys;
            launched_vessel.skipGroundPositioning = true;
            on_vessel_positioned?.Invoke(launched_vessel);
            yield return StartCoroutine(launch_moving_vessel(spawn_transform,
                spawn_offset,
                Quaternion.identity,
                dV));
        }

        public static void PutShipToGround(ShipConstruct ship, Transform spawnPoint, Vector3 offset)
        {
            var partHeightQuery = new PartHeightQuery(float.MaxValue);
            int count = ship.parts.Count;
            for(int i = 0; i < count; i++)
            {
                var p = ship[i];
                partHeightQuery.lowestOnParts.Add(p, float.MaxValue);
                Collider[] componentsInChildren = p.GetComponentsInChildren<Collider>();
                int num = componentsInChildren.Length;
                for(int j = 0; j < num; j++)
                {
                    Collider collider = componentsInChildren[j];
                    if(collider.enabled && collider.gameObject.layer != 21)
                    {
                        partHeightQuery.lowestPoint =
                            Mathf.Min(partHeightQuery.lowestPoint, collider.bounds.min.y);
                        partHeightQuery.lowestOnParts[p] =
                            Mathf.Min(partHeightQuery.lowestOnParts[p], collider.bounds.min.y);
                    }
                }
            }
            for(int k = 0; k < count; k++)
                ship[k]
                    .SendMessage("OnPutToGround",
                        partHeightQuery,
                        SendMessageOptions.DontRequireReceiver);
            Utils.Log("Putting ship to ground: " + partHeightQuery.lowestPoint);
            float angle;
            Vector3 axis;
            spawnPoint.rotation.ToAngleAxis(out angle, out axis);
            var root = ship.parts[0].localRoot.transform;
            offset += spawnPoint.position;
            var CoG = ship.Bounds().center;
            offset -= new Vector3(CoG.x, partHeightQuery.lowestPoint, CoG.z);
            root.Translate(offset, Space.World);
            root.RotateAround(spawnPoint.position, axis, angle);
        }

        public static Vessel AssembleForLaunchUnlanded(
            ShipConstruct ship,
            Orbit orbit,
            string flagURL,
            Game sceneState
        )
        {
            var localRoot = ship.parts[0].localRoot;
            var vessel = localRoot.gameObject.GetComponent<Vessel>();
            if(vessel == null)
                vessel = localRoot.gameObject.AddComponent<Vessel>();
            vessel.id = Guid.NewGuid();
            vessel.vesselName = Localizer.Format(ship.shipName);
            vessel.persistentId = ship.persistentId;
            if(orbit != null)
            {
                var orbitDriver = vessel.gameObject.GetComponent<OrbitDriver>();
                if(orbitDriver == null)
                    orbitDriver = vessel.gameObject.AddComponent<OrbitDriver>();
                orbitDriver.orbit = orbit;
            }
            vessel.Initialize(true);
            vessel.orbitDriver.updateMode = OrbitDriver.UpdateMode.TRACK_Phys;
            vessel.skipGroundPositioning = true;
            vessel.vesselSpawning = false;
            vessel.Landed = false;
            var hashCode = (uint)Guid.NewGuid().GetHashCode();
            var launchID = HighLogic.CurrentGame.launchID++;
            for(int i = 0, count = vessel.parts.Count; i < count; ++i)
            {
                var p = vessel.parts[i];
                p.flightID = ShipConstruction.GetUniqueFlightID(sceneState.flightState);
                p.missionID = hashCode;
                p.launchID = launchID;
                p.flagURL = flagURL;
            }
            if(localRoot.isControlSource == Vessel.ControlLevel.NONE)
            {
                var firstCrewablePart = ShipConstruction.findFirstCrewablePart(ship.parts[0]);
                if(firstCrewablePart == null)
                {
                    var firstControlSource = ShipConstruction.findFirstControlSource(vessel);
                    if(firstControlSource == null)
                        firstCrewablePart = localRoot;
                    else
                        firstCrewablePart = firstControlSource;
                }
                vessel.SetReferenceTransform(firstCrewablePart, true);
            }
            else
                vessel.SetReferenceTransform(localRoot, true);
            Utils.Log("Vessel assembled for launch: " + vessel.GetDisplayName());
            return vessel;
        }

        void stabilize_launched_vessel(float mult)
        {
            launched_vessel.permanentGroundContact = true;
            for(int j = 0, nparts = launched_vessel.parts.Count; j < nparts; j++)
            {
                var p = launched_vessel.parts[j];
                if(p == null|| p.Rigidbody == null)
                    continue;
                p.Rigidbody.angularVelocity *= mult;
                p.Rigidbody.velocity *= mult;
            }
        }

        IEnumerable stabilize_launched_vessel(int frames)
        {
            if(launched_vessel == null)
                yield break;
            var step = 1f / frames;
            for(int i = 0; i < frames; i++)
            {
                stabilize_launched_vessel(step * i);
                yield return null;
            }
            launched_vessel.permanentGroundContact = false;
        }

        IEnumerator push_and_spin_launched_vessel(Vector3 dV, float accelerationTime)
        {
            FlightCameraOverride.AnchorForSeconds(FlightCameraOverride.Mode.Hold,
                launched_vessel.transform,
                1, 
                true);
            var startP = part.Rigidbody.worldCenterOfMass;
            var startAV = part.Rigidbody.angularVelocity;
            var vel = part.Rigidbody.velocity;
            foreach(var p in launched_vessel.Parts)
            {

                if(p.Rigidbody == null)
                    continue;
                p.Rigidbody.angularVelocity = startAV;
                p.Rigidbody.velocity = vel + Vector3.Cross(startAV, p.Rigidbody.worldCenterOfMass - startP);
            }
            if(dV.IsZero())
                yield break;
            var launchedMass = launched_vessel.GetTotalMass();
            var ddV = dV / accelerationTime; // this is for the launched vessel
            var impulseChangeSpeed = ddV * -launchedMass; // this is for the host
            while(accelerationTime > 0)
            {
                var dt = Math.Min(TimeWarp.fixedDeltaTime, accelerationTime);
                var frameDeltaV = ddV * dt;
                foreach(var p in launched_vessel.Parts)
                {
                    if(p.Rigidbody != null)
                        p.Rigidbody.AddForce(frameDeltaV, ForceMode.VelocityChange);
                }
                part.Rigidbody.AddForce(impulseChangeSpeed * dt, ForceMode.Impulse);
                accelerationTime -= dt;
                yield return null;
                FlightCameraOverride.UpdateDurationSeconds(1);
            }
        }

        private static IEnumerable<Collider> all_vessel_colliders(
            Vessel vsl,
            bool only_active = true
        )
        {
            var colliders = vsl.Parts
                .SelectMany(p => p.partTransform.GetComponentsInChildren<Collider>(!only_active));
            return only_active
                ? colliders.Where(c => c.enabled && c.gameObject.activeInHierarchy)
                : colliders;
        }

        private void ignore_launched_vessel_colliders(bool ignore)
        {
            if(part == null || part.vessel == null || launched_vessel == null)
                return;
            if(ignore)
            {
                disabled_launched_colliders.AddRange(all_vessel_colliders(launched_vessel));
                disabled_launched_colliders.ForEach(c => c.enabled = false);
            }
            else
            {
                foreach(var collider in disabled_launched_colliders)
                {
                    if(collider == null)
                        continue;
                    collider.enabled = true;
                }
                disabled_launched_colliders.Clear();
            }
        }

        private void set_launched_parts_state(PartStates state) =>
            launched_vessel.Parts.ForEach(p => p.State = state);

        private void set_launched_vessel_pos_rot(
            Transform spawn_transform,
            Vector3 spawn_offset,
            Quaternion spawn_rot_offset
        )
        {
            try
            {
                launched_vessel
                    .SetPosition(spawn_transform.TransformPointUnscaled(spawn_offset));
                launched_vessel
                    .SetRotation(spawn_transform.rotation * spawn_rot_offset);
            }
            catch(Exception e)
            {
                Utils.Log(
                    $"Exception occured during launched_vessel.SetPosition/Rotation call. Ignoring it:\n{e.StackTrace}");
            }
        }

        private void launch_moving_vessel_on_fixed()
        {
            if(launched_vessel == null)
                return;
            if(vessel.LandedOrSplashed)
            {
                launched_vessel.situation = Vessel.Situations.PRELAUNCH;
                launched_vessel.precalc.isEasingGravity = true;
            }
            launched_vessel.IgnoreGForces(10);
            ignore_launched_vessel_colliders(true);
            set_launched_parts_state(PartStates.PLACEMENT);
            CollisionEnhancer.bypass = true;
        }

        IEnumerator<YieldInstruction> launch_moving_vessel(
            Transform spawn_transform,
            Vector3 spawn_offset,
            Quaternion spawn_rot_offset,
            Vector3 dV
        )
        {
            onFixedUpdate = launch_moving_vessel_on_fixed;
            while(launched_vessel != null && launched_vessel.packed)
            {
                FlightCameraOverride.UpdateDurationSeconds(1);
                set_launched_vessel_pos_rot(spawn_transform, spawn_offset, spawn_rot_offset);
                yield return new WaitForFixedUpdate();
            }
            onFixedUpdate = null;
            if(launched_vessel == null)
                yield break;
            set_launched_vessel_pos_rot(spawn_transform, spawn_offset, spawn_rot_offset);
            launched_vessel.situation = vessel.situation;
            set_launched_parts_state(PartStates.IDLE);
            ignore_launched_vessel_colliders(false);
            var spinner = push_and_spin_launched_vessel(spawn_transform.TransformDirection(dV),
                Utils.Clamp(dV.magnitude / 10, 0.1f, 3f));
            onFixedUpdate = () =>
            {
                if(spinner.MoveNext())
                    return;
                onFixedUpdate = null;
            };
            while(onFixedUpdate != null)
                yield return null;
            FlightGlobals.ForceSetActiveVessel(launched_vessel);
        }

        private void position_proto_vessel(
            ProtoVessel proto_vessel,
            Transform spawn_transform,
            Vector3 spawn_offset
        )
        {
            //state
            proto_vessel.situation = vessel.situation;
            proto_vessel.splashed = vessel.Splashed;
            proto_vessel.landed = vessel.Landed;
            proto_vessel.landedAt = vessel.landedAt;
            //rotation
            spawn_offset = spawn_transform.TransformDirection(spawn_offset);
            //rotate spawn_transform.rotation to protovessel's reference frame
            proto_vessel.rotation = vessel.mainBody.bodyTransform.rotation.Inverse()
                                    * spawn_transform.rotation;
            //set vessel's orbit
            var UT = Planetarium.GetUniversalTime();
            var horb = vessel.orbit;
            var vorb = new Orbit();
            var d_pos = spawn_transform.position + spawn_offset - vessel.CurrentCoM;
            var vpos = horb.pos
                       + new Vector3d(d_pos.x, d_pos.z, d_pos.y)
                       + (horb.vel
                          + ((Vector3d)vessel.rb_velocity).xzy
                          - horb.GetRotFrameVel(horb.referenceBody))
                       * TimeWarp.fixedDeltaTime;
            var vvel = horb.vel
                       + ((Vector3d)(vessel.rb_velocity
                                     + Vector3.Cross(
                                         vessel.transform.rotation * vessel.angularVelocity,
                                         d_pos))).xzy;
            vorb.UpdateFromStateVectors(vpos, vvel, horb.referenceBody, UT);
            proto_vessel.orbitSnapShot = new OrbitSnapshot(vorb);
            //position on a surface
            if(vessel.LandedOrSplashed)
                vpos = spawn_transform.position + spawn_offset;
            else
                vpos = vessel.mainBody.position + vpos.xzy;
            proto_vessel.longitude = vessel.mainBody.GetLongitude(vpos);
            proto_vessel.latitude = vessel.mainBody.GetLatitude(vpos);
            proto_vessel.altitude = vessel.mainBody.GetAltitude(vpos);
        }
    }
}

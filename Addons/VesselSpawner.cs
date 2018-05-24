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
using KSP.UI.Screens;
using UnityEngine;

namespace AT_Utils
{
    public class VesselSpawner
    {
        protected Part part;
        protected Vessel vessel => part.vessel;

        public bool LaunchInProgress { get; private set; }
        Vessel launched_vessel;
        bool vessel_loaded;

        public VesselSpawner() {}
        public VesselSpawner(Part part) { this.part = part; }

        void stabilize_launched_vessel(float mult)
        {
            launched_vessel.permanentGroundContact = true;
            for(int j = 0, nparts = launched_vessel.parts.Count; j < nparts; j++)
            {
                var p = launched_vessel.parts[j];
                var r = p.Rigidbody;
                r.angularVelocity *= mult;
                r.velocity *= mult;
            }
        }

        IEnumerable stabilize_launched_vessel(int frames)
        {
            if(launched_vessel == null) yield break;
            var step = 1f / frames;
            for(int i = 0; i < frames; i++)
            {
                stabilize_launched_vessel(step * i);
                yield return null;
            }
            launched_vessel.permanentGroundContact = false;
        }

        IEnumerable push_and_spin_launched_vessel(Vector3 dV)
        {
            FlightCameraOverride.UpdateDurationSeconds(1);
            var startP = part.Rigidbody.worldCenterOfMass;
            var startAV = part.Rigidbody.angularVelocity;
            var startAVm = startAV.sqrMagnitude;
            var vel = dV;
            vel += (Vector3d)part.Rigidbody.velocity;
            vel += Vector3d.Cross(startAV, launched_vessel.CoM-startP);
            launched_vessel.SetWorldVelocity(vel);
            for(int i = 0; i < 10; i++)
            {
                //this is a hack for incorrect VelocityChange mode (or whatever causing this);
                //if the startAV is applied once, the resulting vessel.angularVelocity is 2-3 times bigger
                var deltaAV = startAV-launched_vessel.transform.rotation*launched_vessel.angularVelocity;
                var deltaAVm = deltaAV.sqrMagnitude;
                if(deltaAVm < 1e-5) break;
                var av = deltaAVm > startAVm? deltaAV.ClampMagnitudeH(startAVm*Mathf.Sqrt(1/deltaAVm)) : deltaAV/3;
                var CoM = launched_vessel.CoM;
                foreach(Part p in launched_vessel.Parts)
                {
                    if(p.Rigidbody != null)
                    {
                        p.Rigidbody.AddTorque(av, ForceMode.VelocityChange);
                        p.Rigidbody.AddForce(Vector3.Cross(av, p.Rigidbody.worldCenterOfMass-CoM), ForceMode.VelocityChange);
                    }
                }
                FlightCameraOverride.UpdateDurationSeconds(1);
                yield return null;
                FlightCameraOverride.UpdateDurationSeconds(1);
                yield return null;
            }
        }

        static void disable_vsl_colliders(Vessel vsl, List<Collider> colliders)
        {
            if(colliders.Count == 0)
            {
                vsl.Parts.ForEach(p => colliders.AddRange(p.FindModelComponents<Collider>().Where(c => c.enabled)));
                colliders.ForEach(c => c.enabled = false);
            }
        }

        static void enable_vsl_colliders(List<Collider> colliders)
        {
            colliders.ForEach(c => { if(c != null) c.enabled = true; });
            colliders.Clear();
        }

        void begin_launch(Transform spawn_transform)
        {
            LaunchInProgress = true;
            vessel_loaded = false;
            FlightCameraOverride.AnchorForSeconds(FlightCameraOverride.Mode.Hold, 
                                                  FlightGlobals.ActiveVessel.transform, 1);
            if(FlightGlobals.ready)
                FloatingOrigin.SetOffset(spawn_transform.position);
        }

        void end_launch()
        {
            launched_vessel = null;
            LaunchInProgress = false;
        }

        void run_on_vessel_loaded(Callback<Vessel> on_vessel_loaded)
        {
            if(!vessel_loaded && launched_vessel.loaded)
            {
                vessel_loaded = true;
                launched_vessel.parts.ForEach(p => p.partTransform = p.transform);
                on_vessel_loaded?.Invoke(launched_vessel);
            }
        }

        IEnumerable<YieldInstruction> launch_moving_vessel(Transform spawn_transform,
                                                           Vector3 spawn_offset, 
                                                           Vector3 dV,
                                                           Callback<Vessel> on_vessel_loaded,
                                                           Callback<Vessel> on_vessel_off_rails,
                                                           Callback<Vessel> on_vessel_launched)
        {
            var vsl_colliders = new List<Collider>();
            FlightCameraOverride.UpdateDurationSeconds(1);
            if(vessel.LandedOrSplashed)
            {
                while(launched_vessel.packed) 
                {
                    if(launched_vessel == null) goto end;
                    launched_vessel.situation = Vessel.Situations.PRELAUNCH;
                    disable_vsl_colliders(launched_vessel, vsl_colliders);
                    run_on_vessel_loaded(on_vessel_loaded);
                    FlightCameraOverride.UpdateDurationSeconds(1);
                    try 
                    { 
                        launched_vessel.SetPosition(spawn_transform.position + spawn_transform.TransformDirection(spawn_offset));
                        launched_vessel.SetRotation(spawn_transform.rotation);
                    }
                    catch(Exception e) 
                    { Utils.Log("Exception occured during launched_vessel.SetPosition/Rotation call. Ignoring it:\n{}", e.StackTrace); }
                    launched_vessel.GoOffRails();
                    yield return new WaitForFixedUpdate();
                }
                if(launched_vessel == null) goto end;
                launched_vessel.SetPosition(spawn_transform.position + spawn_transform.TransformDirection(spawn_offset));
                launched_vessel.SetRotation(spawn_transform.rotation);
                launched_vessel.situation = vessel.situation;
            }
            else
            {
                launched_vessel.Load();
                //hold the vessel inside the hangar until unpacked
                while(launched_vessel.packed) 
                {
                    if(launched_vessel == null) goto end;
                    disable_vsl_colliders(launched_vessel, vsl_colliders);
                    run_on_vessel_loaded(on_vessel_loaded);
                    FlightCameraOverride.UpdateDurationSeconds(1);
                    try 
                    { 
                        launched_vessel.SetPosition(spawn_transform.position+ spawn_transform.TransformDirection(spawn_offset));
                        launched_vessel.SetRotation(spawn_transform.rotation);
                    }
                    catch(Exception e) 
                    { Utils.Log("Exception occured during launched_vessel.SetPosition/Rotation call. Ignoring it:\n{}", e.StackTrace); }
                    yield return new WaitForFixedUpdate();
                }
                launched_vessel.SetPosition(spawn_transform.position+ spawn_transform.TransformDirection(spawn_offset));
                launched_vessel.SetRotation(spawn_transform.rotation);
            }
            FlightGlobals.ForceSetActiveVessel(launched_vessel);
            on_vessel_off_rails?.Invoke(launched_vessel);
            enable_vsl_colliders(vsl_colliders);
            foreach(var _ in push_and_spin_launched_vessel(spawn_transform.TransformDirection(dV)))
                yield return null;
            on_vessel_launched?.Invoke(launched_vessel);
            end:
            {
                enable_vsl_colliders(vsl_colliders);
                yield break;
            }
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
                        partHeightQuery.lowestPoint = Mathf.Min(partHeightQuery.lowestPoint, collider.bounds.min.y);
                        partHeightQuery.lowestOnParts[p] = Mathf.Min(partHeightQuery.lowestOnParts[p], collider.bounds.min.y);
                    }
                }
            }
            for(int k = 0; k < count; k++)
                ship[k].SendMessage("OnPutToGround", partHeightQuery, SendMessageOptions.DontRequireReceiver);
            Utils.Log("putting ship to ground: " + partHeightQuery.lowestPoint);
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

        public IEnumerator<YieldInstruction> SpawnShipConstructToGround(ShipConstruct construct, 
                                                                        Transform spawn_transform,
                                                                        Vector3 spawn_offset,
                                                                        Callback<Vessel> on_vessel_loaded = null,
                                                                        Callback<Vessel> on_vessel_off_rails = null,
                                                                        Callback<Vessel> on_vessel_launched = null,
                                                                        int easing_frames = 0)
        {
            begin_launch(spawn_transform);
            PutShipToGround(construct, spawn_transform, spawn_offset);
            ShipConstruction.AssembleForLaunch(construct,
                                               vessel.landedAt, vessel.displaylandedAt, part.flagURL,
                                               FlightDriver.FlightStateCache,
                                               new VesselCrewManifest());
            launched_vessel = FlightGlobals.Vessels[FlightGlobals.Vessels.Count - 1];
            StageManager.BeginFlight();
            while(!launched_vessel.loaded)
            {
                FlightCameraOverride.UpdateDurationSeconds(1);
                yield return new WaitForFixedUpdate();
            }
            on_vessel_loaded?.Invoke(launched_vessel);
            while(launched_vessel.packed)
            {
                launched_vessel.precalc.isEasingGravity = true;
                launched_vessel.situation = Vessel.Situations.PRELAUNCH;
                stabilize_launched_vessel(0);
                FlightCameraOverride.UpdateDurationSeconds(1);
                yield return new WaitForFixedUpdate();
            }
            on_vessel_off_rails?.Invoke(launched_vessel);
            if(easing_frames > 0)
            {
                foreach(var n in stabilize_launched_vessel(easing_frames))
                {
                    FlightCameraOverride.UpdateDurationSeconds(1);
                    yield return new WaitForFixedUpdate();
                }
            }
            on_vessel_launched?.Invoke(launched_vessel);
            end_launch();
        }

        public IEnumerator<YieldInstruction> SpawnShipConstruct(ShipConstruct construct, 
                                                                Transform spawn_transform,
                                                                Vector3 spawn_offset, 
                                                                Vector3 dV,
                                                                Callback<Vessel> on_vessel_loaded = null,
                                                                Callback<Vessel> on_vessel_off_rails = null,
                                                                Callback<Vessel> on_vessel_launched = null)
        {
            begin_launch(spawn_transform);
            float angle;
            Vector3 axis;
            spawn_transform.rotation.ToAngleAxis(out angle, out axis);
            var root = construct.parts[0].localRoot.transform;
            var offset = -construct.Bounds(root).center + spawn_offset;
            root.Translate(spawn_transform.position+spawn_transform.TransformDirection(offset), Space.World);
            root.RotateAround(spawn_transform.position, axis, angle);
            //initialize new vessel
            ShipConstruction.AssembleForLaunch(construct, 
                                               vessel.landedAt, vessel.displaylandedAt, 
                                               part.flagURL, 
                                               FlightDriver.FlightStateCache,
                                               new VesselCrewManifest());
            ShipConstructLoader.SetConstructRendering(construct, true);
            launched_vessel = FlightGlobals.Vessels[FlightGlobals.Vessels.Count - 1];
            launched_vessel.situation = vessel.situation;
            launched_vessel.Landed = false;
            launched_vessel.Splashed = false;
			launched_vessel.skipGroundPositioning = true;
            var orbitDriver = launched_vessel.gameObject.GetComponent<OrbitDriver>();
            if(orbitDriver == null)
                orbitDriver = launched_vessel.gameObject.AddComponent<OrbitDriver>();
            orbitDriver.orbit = new Orbit(vessel.orbit);
            orbitDriver.updateMode = OrbitDriver.UpdateMode.TRACK_Phys;
            //launch the vessel
            StageManager.BeginFlight();
            foreach(var i in launch_moving_vessel(spawn_transform, offset, dV, 
                                                  on_vessel_loaded, 
                                                  on_vessel_off_rails,
                                                  on_vessel_launched))
                yield return i;
            end_launch();
        }

        public IEnumerator<YieldInstruction> SpawnProtoVessel(ProtoVessel proto_vessel, 
                                                              Transform spawn_transform,
                                                              Vector3 spawn_offset, 
                                                              Vector3 dV,
                                                              Callback<Vessel> on_vessel_loaded = null,
                                                              Callback<Vessel> on_vessel_off_rails = null,
                                                              Callback<Vessel> on_vessel_launched = null)
        {
            begin_launch(spawn_transform);
            proto_vessel.Load(HighLogic.CurrentGame.flightState);
            launched_vessel = proto_vessel.vesselRef;
            foreach(var i in launch_moving_vessel(spawn_transform, spawn_offset, dV, 
                                                  on_vessel_loaded, 
                                                  on_vessel_off_rails,
                                                  on_vessel_launched))
                yield return i;
            end_launch();
        }
    }
}

//   FlightCameraOverride.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using UnityEngine;

namespace AT_Utils
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class FlightCameraOverrideInitializer : MonoBehaviour
	{
		void Awake()
		{
			GameEvents.onLevelWasLoadedGUIReady.Add(onLevelWasLoaded);
		}

		void onLevelWasLoaded(GameScenes scene)
		{
			if(scene != GameScenes.FLIGHT) return;
			var camera = FlightCamera.fetch.mainCamera;
			if(camera == null || camera.gameObject == null) 
				this.Log("Flight camera is null. Unable to attach FlightCameraOverride script.");
			else if(camera.gameObject.GetComponent<FlightCameraOverride>() == null)
			{
				var fco = camera.gameObject.AddComponent<FlightCameraOverride>();
				this.Log("Attached FlightCameraOverride component to camera GameObject: {}", fco);
			}
		}
	}

	public class FlightCameraOverride : MonoBehaviour
	{
		public enum Mode { None, Hold, LookAt, LookBetween, LookFromTo, OrbitAround }

		const int EASING_FRAMES = 120;
        const float MAX_DIST = 1000;
        static float ln12 = Mathf.Log(0.5f);

		static Mode mode;
		static int duration = -1;
		static float easing = -1;
		static double endUT = -1;

        public static Transform anchor { get; private set; }
        public static Transform target { get; private set; }

		static Vector3 pos, rel_pos;
		static Vector3 pivot, rel_pivot;

		static void set_rel_coordinates(Transform new_anchor)
		{
			anchor = new_anchor;
			var camera = FlightCamera.fetch;
			pos = camera.GetCameraTransform().position;
			rel_pos = pos-anchor.position;
			pivot = camera.GetPivot().position;
			rel_pivot = pivot-anchor.position;
		}

		public static bool Active { get { return mode != Mode.None; } }

		public static void UpdateDurationSeconds(double seconds)
		{
			if(!Active) return;
			var new_endUT = Planetarium.GetUniversalTime()+seconds;
			if(new_endUT > endUT) endUT = new_endUT;
			duration = -1;
		}

		public static void UpdateDuration(int num_frames)
		{
			if(!Active) return;
			if(num_frames > duration) 
				duration = num_frames;
			endUT = -1;
		}

		static void Activate(Mode M, Transform new_anchor, Transform new_target, bool override_reference = false)
		{
			mode = M;
			FlightCamera.fetch.DeactivateUpdate();
			if(anchor == null || override_reference)
				set_rel_coordinates(new_anchor);
			if(target == null || override_reference)
				target = new_target;
			easing = EASING_FRAMES;
		}

        public static void AnchorForSeconds(Mode mode, Transform new_anchor, double seconds, bool override_reference = false)
		{
            if(override_reference || !Active)
                Activate(mode, new_anchor, null, override_reference);
			UpdateDurationSeconds(seconds);
		}

		public static void Anchor(Mode mode, Transform new_anchor, int num_frames, bool override_reference = false)
		{
            if(override_reference || !Active)
                Activate(mode, new_anchor, null, override_reference);
			UpdateDuration(num_frames);
		}

        public static void TargetForSeconds(Mode mode, Transform new_anchor, Transform new_target, double seconds, bool override_reference = false)
        {
            if(override_reference || !Active)
                Activate(mode, new_anchor, new_target, override_reference);
            UpdateDurationSeconds(seconds);
        }

        public static void Target(Mode mode, Transform new_anchor, Transform new_target, int num_frames, bool override_reference = false)
        {
            if(override_reference || !Active)
                Activate(mode, new_anchor, new_target, override_reference);
            UpdateDuration(num_frames);
        }

		public static void Deactivate()
		{
			if(FlightCamera.fetch != null)
				FlightCamera.fetch.ActivateUpdate();
            if(FlightGlobals.ActiveVessel != null)
            {
                anchor = FlightGlobals.ActiveVessel.transform;
                pos = rel_pos+anchor.position;
                pivot = rel_pivot+anchor.position;
            }
            update_camera();
			mode = Mode.None;
			anchor = null;
			target = null;
			duration = -1;
			easing = -1;
			endUT = -1;
		}

		void OnDestroy() { Deactivate(); }

        static float smooth_easing(Vector3 t, out float lin_easing)
        {
            var L = (t-anchor.position).magnitude;
            var a = Mathf.Min(L*0.1f, 100);
            var p = Mathf.Log(a/L)/ln12;
            lin_easing = 1-easing/EASING_FRAMES;
            return Mathf.Pow(lin_easing, p);
        }

		static void update_pos_and_pivot()
		{
            Vessel vsl;
            Vector3d axis;
            float tl, ts;
			switch(mode)
			{
			case Mode.Hold:
				pos = rel_pos+anchor.position;
				pivot = rel_pivot+anchor.position;
				break;
			case Mode.LookAt:
                if(target == null) { Deactivate(); return; }
				pos = rel_pos+anchor.position;
                pivot = pos+(target.position-pos).normalized*MAX_DIST;
				if(easing > 0)
                {
                    ts = smooth_easing(pivot, out tl);
                    pivot = Vector3.Lerp(rel_pivot+anchor.position, pivot, ts);
					easing -= 1;
				}
				break;
            case Mode.LookBetween:
                if(target == null) { Deactivate(); return; }
                pos = rel_pos+anchor.position;
                pivot = pos+((target.position+anchor.position)/2-pos).normalized*MAX_DIST;
                if(easing > 0)
                {
                    ts = smooth_easing(pivot, out tl);
                    pivot = Vector3.Lerp(rel_pivot+anchor.position, pivot, ts);
                    easing -= 1;
                }
                break;
            case Mode.LookFromTo:
                if(target == null) { Deactivate(); return; }
                vsl = anchor.gameObject.GetComponent<Vessel>();
                axis = vsl == null? anchor.up : (Vector3)vsl.orbit.pos.xzy;
                pos = anchor.position + Quaternion.AngleAxis(20, axis)*(anchor.position-target.position).normalized*30;
                pivot = pos+(target.position-pos).normalized*MAX_DIST;
                if(easing > 0)
                {
                    ts = smooth_easing(pivot, out tl);
                    pos = Vector3.Lerp(rel_pos+anchor.position, pos, tl);
                    pivot = Vector3.Lerp(rel_pivot+anchor.position, pivot, ts);
                    easing -= 1;
                }
                break;
            case Mode.OrbitAround:
                pos = rel_pos+anchor.position;
                pivot = rel_pivot+anchor.position;
                vsl = anchor.gameObject.GetComponent<Vessel>();
                axis = vsl == null? anchor.up : (Vector3)vsl.orbit.pos.xzy;
                rel_pos = Quaternion.AngleAxis(0.15f, axis)*rel_pos;
                break;
			}
		}

		static void update_camera()
		{
			var camera = FlightCamera.fetch;
			camera.GetPivot().position = pivot;
			camera.SetCamCoordsFromPosition(pos);
			camera.GetCameraTransform().position = pos;
		}

		void OnPreCull()
		{
			if(!Active) return;
			if(anchor == null) { Deactivate(); return; }
			//update camera position and focus
			update_pos_and_pivot();
			update_camera();
			//check boundary conditions
			if(endUT > 0)
			{ if(endUT < Planetarium.GetUniversalTime()) Deactivate(); }
			else if(duration-- <= 0) Deactivate();
		}

		void Update()
		{
			if(!Active) return;
			FlightCamera.fetch.DeactivateUpdate();
			update_camera();
		}

//		#if DEBUG
//		void OnRenderObject()
//		{
//			if(!Active) return;
//			Utils.GLDrawPoint(pivot, Color.magenta, 0.5f);
//		}
//		#endif
	}
}


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
		public enum Mode { None, Hold, LookAt }

		const int EASING_FRAMES = 60;

		static Mode mode;
		static int duration = -1;
		static float easing = -1;
		static double endUT = -1;

		static Transform anchor, target;

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

		public static void HoldCameraStillForSeconds(Transform new_anchor, double seconds, bool override_reference = false)
		{
			Activate(Mode.Hold, new_anchor, null, override_reference);
			UpdateDurationSeconds(seconds);
		}

		public static void HoldCameraStill(Transform new_anchor, int num_frames, bool override_reference = false)
		{
			Activate(Mode.Hold, new_anchor, null, override_reference);
			UpdateDuration(num_frames);
		}

		public static void LookAtForSeconds(Transform new_anchor, Transform new_target, double seconds, bool override_reference = false)
		{
			Activate(Mode.LookAt, new_anchor, new_target, override_reference);
			UpdateDurationSeconds(seconds);
		}

		public static void LookAt(Transform new_anchor, Transform new_target, int num_frames, bool override_reference = false)
		{
			Activate(Mode.LookAt, new_anchor, new_target, override_reference);
			UpdateDuration(num_frames);
		}

		public static void Deactivate()
		{
			#if DEBUG
			Utils.Log("Deactivating FCO: duration {}, seconds {}, anchor {}", 
			          duration, endUT-Planetarium.GetUniversalTime(), anchor);
			#endif
			if(FlightCamera.fetch != null)
				FlightCamera.fetch.ActivateUpdate();
			mode = Mode.None;
			anchor = null;
			target = null;
			duration = -1;
			easing = -1;
			endUT = -1;
		}

		void OnDestroy() { Deactivate(); }

		static void update_pos_and_pivot()
		{
			switch(mode)
			{
			case Mode.Hold:
				pos = rel_pos+anchor.position;
				pivot = rel_pivot+anchor.position;
				break;
			case Mode.LookAt:
				pos = rel_pos+anchor.position;
				if(easing > 0)
				{
					pivot = Vector3.Lerp(target.position, rel_pivot+anchor.position, easing/EASING_FRAMES);
					easing -= 1;
				}
				else pivot = target.position;
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

		#if DEBUG
		void OnRenderObject()
		{
			if(!Active) return;
			Utils.GLDrawPoint(pivot, Color.magenta, 0.5f);
		}
		#endif
	}
}


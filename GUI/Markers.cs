//   Markers.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public static class Markers
    {
        public const float DefaultIconSize = 16;
        public static readonly Texture2D DefaultTexture = Texture2D.whiteTexture;

        static Material _icon_material;
        public static Material IconMaterial
        {
            get
            {
                if(_icon_material == null) 
                    _icon_material = new Material(Shader.Find("Particles/Additive"));
                return _icon_material;
            }
        }

        public static Camera CurrentCamera 
        { 
            get 
            { 
                if(HighLogic.LoadedSceneIsEditor) return EditorCamera.Instance.cam;
                return MapView.MapIsEnabled? PlanetariumCamera.Camera : FlightCamera.fetch.mainCamera;
            } 
        }

        public static void DrawLabelAtPointer(string text)
        { GUI.Label(new Rect(Input.mousePosition.x + 15, Screen.height - Input.mousePosition.y, 300, 200), text); }

        static readonly Rect texture_rect = new Rect(0f, 0f, 1f, 1f);
        public static bool DrawMarker(Vector3 icon_center, Color color, Texture2D texture, float size)
        {
            if(color.a.Equals(0)) return false;
            if(texture == null) texture = DefaultTexture;
            var icon_rect = new Rect(icon_center.x - size * 0.5f, (float)Screen.height - icon_center.y - size * 0.5f, size, size);
            Graphics.DrawTexture(icon_rect, texture, texture_rect, 0, 0, 0, 0, color, IconMaterial);
            return icon_rect.Contains(Event.current.mousePosition);
        }

        public static bool DrawCBMarker(CelestialBody body, Coordinates pos, Color color, out Vector3d worldPos, 
                                        Texture2D texture = null, float size = DefaultIconSize)
        {
            worldPos = Vector3d.zero;
            if(color.a.Equals(0)) return false;
            Camera camera;
            Vector3d point;
            if(MapView.MapIsEnabled)
            {
                //TODO: cache local center coordinates of the marker
                camera = PlanetariumCamera.Camera;
                worldPos = body.position + (pos.Alt+body.Radius) * body.GetSurfaceNVector(pos.Lat, pos.Lon);
                if(IsOccluded(worldPos, body)) return false;
                point = ScaledSpace.LocalToScaledSpace(worldPos);
            }
            else
            {
                camera = FlightCamera.fetch.mainCamera;
                worldPos = body.GetWorldSurfacePosition(pos.Lat, pos.Lon, pos.Alt);
                if(camera.transform.InverseTransformPoint(worldPos).z <= 0) return false;
                point = worldPos;
            }
            return DrawMarker(camera.WorldToScreenPoint(point), color, texture, size);
        }

        public static bool DrawCBMarker(CelestialBody body, Coordinates pos, Color color, Texture2D texture = null, float size = DefaultIconSize)
        { Vector3d worldPos; return DrawCBMarker(body, pos, color, out worldPos, texture, size); }

        public static void DrawWorldMarker(Vector3d wPos, Color color, string label = "", Texture2D texture = null, float size = DefaultIconSize)
        {
            if(color.a.Equals(0)) return;
            var camera = CurrentCamera;
            if(camera.transform.InverseTransformPoint(wPos).z <= 0) return;
            if(DrawMarker(camera.WorldToScreenPoint(MapView.MapIsEnabled? ScaledSpace.LocalToScaledSpace(wPos) : wPos), color, texture, size) &&
               !string.IsNullOrEmpty(label)) DrawLabelAtPointer(label);
        }

        //Tests if byBody occludes worldPosition, from the perspective of the planetarium camera
        public static bool IsOccluded(Vector3d wPos, CelestialBody byBody)
        {
            var c_pos = MapView.MapIsEnabled? 
                ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position) :
                (Vector3d)FlightCamera.fetch.mainCamera.transform.position;
            return Utils.Angle2(c_pos-wPos, byBody.position-wPos) <= 90.0;
        }
    }
}


//   StringID.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using UnityEngine;
using KSP.Localization;

namespace AT_Utils
{
    public static class StringID
    {
        static string GetSystemID(object o) =>
        string.Format("{0}[{1:X}]", o.GetType().Name, o.GetHashCode());

        static string GetObjectID(Object o) =>
        string.Format("{0}[{1:X}]", o.name, o.GetHashCode());

        static string GetComponentID(Component c) =>
        string.Format("{0}:{1}", c.gameObject.GetID(), GetObjectID(c));

        static string GetComponentSysID(Component c) =>
        string.Format("{0}:{1}", c.gameObject.GetID(), GetSystemID(c));

        public static string GetID(this object o) =>
        o == null ? "_object" : GetSystemID(o);

        public static string GetID(this Object o) =>
        o == null ? "_Object" : GetSystemID(o);

        public static string GetID(this GameObject o) =>
        o == null ? "_GO" : GetObjectID(o);

        public static string GetID(this Component c) =>
        c == null ? "_component" : GetComponentSysID(c);

        public static string GetID(this Transform t) =>
        t == null ? "_transform" : GetComponentID(t);

        public static string GetID(this MonoBehaviour mb) =>
        mb == null ? "_behaviour" : GetComponentSysID(mb);

        public static string GetID(this Vessel vessel) =>
        vessel == null ?
        "_vessel" :
        string.Format("{0}[{1:X}]",
                      string.IsNullOrEmpty(vessel.vesselName) ?
                      vessel.id.ToString() : Localizer.Format(vessel.vesselName),
                      vessel.persistentId);

        public static string GetID(this Part part) =>
        part == null ?
        "_part" :
        string.Format("{0}:{1}[{2:X}]",
                      part.vessel.GetID(),
                      part.Title(), part.persistentId);

        public static string GetID(this PartModule part_module) =>
        part_module == null ?
        "_part_module" :
        string.Format("{0}:{1}[{2:X}]",
                      part_module.part.GetID(),
                      part_module.ClassName, part_module.GetHashCode());
    }
}

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
        private static string GetSystemID(object o) => $"{o.GetType().Name}[{o.GetHashCode():X}]";

        private static string GetObjectID(Object o) => $"{o.name}[{o.GetHashCode():X}]";

        private static string GetComponentID(Component c) => $"{c.gameObject.GetID()}:{GetObjectID(c)}";

        private static string GetComponentSysID(Component c) => $"{c.gameObject.GetID()}:{GetSystemID(c)}";

        public static string GetID(this object o) => o == null ? "_object" : GetSystemID(o);

        public static string GetID(this Object o) => o == null ? "_Object" : GetObjectID(o);

        public static string GetID(this GameObject o) => o == null ? "_GO" : GetObjectID(o);

        public static string GetID(this Component c) => c == null ? "_component" : GetComponentSysID(c);

        public static string GetID(this Transform t) => t == null ? "_transform" : GetComponentID(t);

        public static string GetID(this MonoBehaviour mb) => mb == null ? "_behaviour" : GetComponentSysID(mb);

        public static string GetID(this Vessel vessel) =>
            vessel == null
                ? "_vessel"
                : $"{(string.IsNullOrEmpty(vessel.vesselName) ? vessel.id.ToString() : Localizer.Format(vessel.vesselName))}[{vessel.persistentId:X}]";

        public static string GetID(this Part part) =>
            part == null
                ? "_part"
                : $"{GetID(part.vessel)}:{part.Title()}[{part.persistentId:X}]";

        public static string GetID(this PartModule part_module) =>
            part_module == null
                ? "_part_module"
                : $"{GetID(part_module.part)}:{part_module.GetType().Name}[{part_module.GetHashCode():X}]";
    }
}

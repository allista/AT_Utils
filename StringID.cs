//   StringID.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;
namespace AT_Utils
{
    public static class StringID
    {
        public static string GetID(this object o) =>
        o == null ? 
        "_object" : 
        string.Format("{0}[{1:X}]", 
                      o.GetType().Name, o.GetHashCode());

        public static string GetID(this MonoBehaviour mb) =>
        mb == null ? 
        "_behaviour" : 
        string.Format("{0}[{1:X}]", 
                      mb.name, mb.GetHashCode());

        public static string GetID(this Vessel vessel) =>
        vessel == null ?
        "_vessel" :
        string.Format("{0}[{1:X}]",
                      string.IsNullOrEmpty(vessel.vesselName) ?
                      vessel.id.ToString() : vessel.vesselName,
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

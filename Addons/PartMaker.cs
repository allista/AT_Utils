//   ShipMaker.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;

namespace AT_Utils
{
    public static class PartMaker
    {
        public static Part CreatePart(string part_name, string flag_url)
        {
            var part_info = PartLoader.getPartInfoByName(part_name);
            if(part_info == null)
            {
                Utils.Message("No such part: {0}", part_name);
                return null;
            }
            return CreatePart(part_info, flag_url);
        }

        public static Part CreatePart(AvailablePart part_info, string flag_url)
        {
            var part = Object.Instantiate(part_info.partPrefab);
            part.gameObject.SetActive(true);
            part.partInfo = part_info;
            part.name = part_info.name;
            part.flagURL = flag_url;
            part.persistentId = FlightGlobals.GetUniquepersistentId();
            FlightGlobals.PersistentLoadedPartIds.Remove(part.persistentId);
            part.transform.position = Vector3.zero;
            part.attPos0 = Vector3.zero;
            part.transform.rotation = Quaternion.identity;
            part.attRotation = Quaternion.identity;
            part.attRotation0 = Quaternion.identity;
            part.partTransform = part.transform;
            part.orgPos = part.transform.root.InverseTransformPoint(part.transform.position);
            part.orgRot = Quaternion.Inverse(part.transform.root.rotation) * part.transform.rotation;
            part.packed = true;
            //initialize modules
            part.InitializeModules();
            var module_nodes = part_info.partConfig.GetNodes("MODULE");
            for(int i = 0, maxLength = module_nodes.Length; i < maxLength; i++)
            {
                var node = module_nodes[i];
                var module_name = node.GetValue("name");
                if(!string.IsNullOrEmpty(module_name))
                {
                    var module = part.Modules[i];
                    if(module != null && module.ClassName == module_name)
                        module.Load(node);
                }
            }
            foreach(var module in part.Modules)
                module.OnStart(PartModule.StartState.PreLaunch);
            return part;
        }

        public static ShipConstruct CreatePartConstruct(Part part, string name = null, string description = null)
        {
            var title = part.Title();
            var ship = new ShipConstruct(name ?? title, description ?? title, part)
            {
                rotation = Quaternion.identity,
                missionFlag = part.flagURL
            };
            return ship;
        }

        public static ShipConstruct CreatePartConstruct(string part_name, string flag_url,
                                                        string name = null, string description = null)
        {
            var part = CreatePart(part_name, flag_url);
            return part != null ? CreatePartConstruct(part, name, description) : null;
        }

        public static ShipConstruct CreatePartConstruct(AvailablePart part_info, string flag_url,
                                                        string name = null, string description = null)
        {
            var part = CreatePart(part_info, flag_url);
            return part != null ? CreatePartConstruct(part, name, description) : null;
        }
    }
}

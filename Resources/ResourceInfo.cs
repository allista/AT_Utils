//   ResourceInfo.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

namespace AT_Utils
{
    public class ResourceInfo : ConfigNodeObject
    {
        new public const string NODE_NAME = "RESOURCE_INFO";

        [Persistent] public string name = "";

        PartResourceDefinition _resource;

        public PartResourceDefinition def
        {
            get
            {
                if(_resource == null)
                    _resource = PartResourceLibrary.Instance.GetDefinition(name);
                return _resource;
            }
        }

        public int id { get { return def.id; } }

        public ResourceInfo() {}

        public ResourceInfo(string name = "")
        {
            this.name = name;
        }
    }
}


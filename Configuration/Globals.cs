//   PluginConfig.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System.Collections.Generic;

namespace AT_Utils
{
    public class AT_UtilsGlobals : PluginGlobals<AT_UtilsGlobals>
    {
        [Persistent] public Styles.Config StylesConfig = new Styles.Config();

        //for Metric
        [Persistent] public string MeshesToSkip = string.Empty;
        [Persistent] public string BadParts = string.Empty;

        public List<string> MeshesToSkipList = new List<string>();
        public List<string> BadPartsList = new List<string>();

        public override void Init()
        {
            //init meshes and parts names
            if(!string.IsNullOrEmpty(MeshesToSkip))
                MeshesToSkipList.AddUniqueRange(Utils.ParseLine(MeshesToSkip, Utils.Comma));
            if(!string.IsNullOrEmpty(BadParts))
                BadPartsList.AddUniqueRange(Utils.ParseLine(BadParts, Utils.Comma));
        }
    }
}


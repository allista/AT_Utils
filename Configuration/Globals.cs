//   PluginConfig.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System.Collections.Generic;
using AT_Utils.UI;

namespace AT_Utils
{
    public class AT_UtilsGlobals : PluginGlobals<AT_UtilsGlobals>
    {
        [Persistent] public Styles.Config StylesConfig = new Styles.Config();

        [Persistent] public float LineWidthMult = 0.2f;
        [Persistent] public float MinLineWidthMult = 0.02f;

        //for Metric
        [Persistent] public string MeshesToSkip = string.Empty;
        [Persistent] public string BadParts = string.Empty;

        public List<string> MeshesToSkipList = new List<string>();
        public List<string> BadPartsList = new List<string>();

        public UIBundle AssetBundle = UIBundle.Create("000_AT_Utils/at_utils_ui.bundle");

        public override void Save(ConfigNode node)
        {
            base.Save(node);
            var colors = node.AddNode("Colors");
            foreach(var color in Colors.All)
                colors.AddValue(color.Key, color.Value.html);
        }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            var colors = node.GetNode("Colors");
            if(colors != null)
            {
                foreach(ConfigNode.Value color in colors.values)
                {
                    var setting = Colors.GetColor(color.name);
                    if(setting != null)
                        setting.html = color.value;
                }
            }
        }

        public override void Init()
        {
            //init meshes and parts names
            if(!string.IsNullOrEmpty(MeshesToSkip))
                MeshesToSkipList.AddUniqueRange(Utils.ParseLine(MeshesToSkip, Utils.Delimiters));
            if(!string.IsNullOrEmpty(BadParts))
                BadPartsList.AddUniqueRange(Utils.ParseLine(BadParts, Utils.Delimiters));
            //log configuration to be able to check it later
            Utils.Log("Meshes to skip: {}", MeshesToSkipList);
            Utils.Log("Bad parts: {}", BadPartsList);
        }
    }
}


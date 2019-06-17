//   AT_Utils_UI.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using KSPAssets;
using KSPAssets.Loaders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public class UIBundle
    {
        readonly string BUNDLE = "";
        BundleDefinition bundle_def;
        Dictionary<string,GameObject> loaded_assets = new Dictionary<string, GameObject>();

        public UIBundle(string bundle_path)
        {
            BUNDLE = bundle_path;
        }

        IEnumerable load(AssetDefinition asset)
        {
            AssetLoader.Loader loader = null;
            if(!AssetLoader.LoadAssets(l => { loader = l; }, asset))
            {
                Utils.Log("Asset load request invalid: {}", asset.path);
                yield break;
            }
            while(loader == null)
                yield return null;
            if(loader.objects[0] == null)
            {
                Utils.Log("Failed loading asset: {}", asset.path);
                yield break;
            }
            var obj = loader.objects[0] as GameObject;
            if(obj == null)
            {
                Utils.Log("Loaded object is not a GO: {}", asset.path);
                yield break;
            }
            loaded_assets[asset.name] = obj;
        }

        public IEnumerable LoadAsset(string name)
        {
            if(loaded_assets.ContainsKey(name))
                yield break;
            if(bundle_def == null)
                bundle_def = AssetLoader.GetBundleDefinition(BUNDLE);
            if(bundle_def != null)
            {
                var asset = AssetLoader.GetAssetDefinitionWithName(bundle_def, name);
                if(asset != null)
                {
                    foreach(var _ in load(asset))
                        yield return null;
                }
            }
        }

        public GameObject GetAsset(string name)
        {
            GameObject obj;
            return loaded_assets.TryGetValue(name, out obj) ? obj : null;
        }
    }
}

//   AT_Utils_UI.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public class UIBundle
    {
        private readonly string BUNDLE;
        public bool BundleNotFound { get; private set; }
        private AssetBundle bundle;
        private AssetBundleCreateRequest bundleReq;
        private readonly Dictionary<string,GameObject> loaded_assets = new Dictionary<string, GameObject>();

        public UIBundle(string game_data_path)
        {
            BUNDLE = game_data_path;
        }

        ~UIBundle()
        {
            if(bundle != null) 
                bundle.Unload(true);
        }


        public IEnumerable LoadAsset(string name)
        {
            if(loaded_assets.ContainsKey(name))
                yield break;
            if(BundleNotFound)
                yield break;
            if(bundle == null && bundleReq == null)
            {
                bundleReq = AssetBundle.LoadFromFileAsync(Utils.PathChain(KSPUtil.ApplicationRootPath, "GameData", BUNDLE));
                yield return bundleReq;
                bundle = bundleReq.assetBundle;
                bundleReq = null;
                if(bundle == null)
                {
                    Utils.Log($"Unable to load {BUNDLE} bundle.");
                    BundleNotFound = false;
                    yield break;
                }
            }
            while(bundleReq != null)
                yield return null;
            var assetReq = bundle.LoadAssetAsync<GameObject>(name);
            yield return assetReq;
            if(assetReq.asset == null)
                Utils.Log($"Unable to load {name} asset from {BUNDLE}.");
            loaded_assets[name] = assetReq.asset as GameObject;
        }

        public GameObject GetAsset(string name) => 
            loaded_assets.TryGetValue(name, out var obj) ? obj : null;

        public override string ToString() => BUNDLE;
    }
}

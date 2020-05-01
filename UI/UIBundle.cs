//   AT_Utils_UI.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AT_Utils
{
    public class UIBundle
    {
        private readonly string bundlePath;
        public bool BundleNotFound { get; private set; }
        private AssetBundle bundle;
        private AssetBundleCreateRequest bundleReq;
        private readonly Dictionary<string, GameObject> loaded_assets = new Dictionary<string, GameObject>();

        public UIBundle(string game_data_path)
        {
            bundlePath = Utils.PathChain(KSPUtil.ApplicationRootPath,
                "GameData",
                game_data_path);
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
            if(bundle == null)
            {
                bundle = AssetBundle
                    .GetAllLoadedAssetBundles()
                    .FirstOrDefault(a => a.name == bundlePath);
            }
            if(bundle == null && bundleReq == null)
            {
                bundleReq = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return bundleReq;
                bundle = bundleReq.assetBundle;
                bundleReq = null;
                if(bundle == null)
                {
                    Utils.Error($"Unable to load {bundlePath} bundle.");
                    BundleNotFound = false;
                    yield break;
                }
                bundle.name = bundlePath;
            }
            while(bundleReq != null)
                yield return null;
            if(bundle == null)
                yield break;
            var assetReq = bundle.LoadAssetAsync<GameObject>(name);
            yield return assetReq;
            if(assetReq.asset == null)
                Utils.Error($"Unable to load {name} asset from {bundlePath}.");
            loaded_assets[name] = assetReq.asset as GameObject;
        }

        public GameObject GetAsset(string name) => loaded_assets.TryGetValue(name, out var obj) ? obj : null;

        public override string ToString() => bundlePath;
    }
}

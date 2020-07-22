//   AT_Utils_UI.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AT_Utils
{
    public class UIBundle
    {
        private static readonly Dictionary<string, UIBundle> bundles = new Dictionary<string, UIBundle>();

        public static UIBundle Create(string game_data_path)
        {
            if(bundles.TryGetValue(game_data_path, out var bundle)
               && bundle != null)
                return bundle;
            bundle = new UIBundle(game_data_path);
            bundles[game_data_path] = bundle;
            bundle.loadBundleSync();
            Utils.Info($"Registered UIBundle: {game_data_path}");
            return bundle;
        }

        private readonly string bundlePath;
        public bool BundleNotFound { get; private set; }
        private AssetBundle bundle;
        private readonly Dictionary<string, GameObject> loaded_assets = new Dictionary<string, GameObject>();

        private UIBundle(string game_data_path)
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

        private bool findLoadedBundle()
        {
            bundle = AssetBundle
                .GetAllLoadedAssetBundles()
                .FirstOrDefault(a => a.name == bundlePath);
            return bundle != null;
        }

        private void loadBundleSync()
        {
            if(!findLoadedBundle())
                bundle = AssetBundle.LoadFromFile(bundlePath);
            if(bundle != null)
                return;
            Utils.Error($"Unable to load bundle: {bundlePath}");
            BundleNotFound = false;
        }

        public IEnumerable<YieldInstruction> LoadAsset(string name)
        {
            if(loaded_assets.ContainsKey(name))
                yield break;
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

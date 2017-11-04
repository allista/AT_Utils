//   ModuleAsteroidFix.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

namespace AT_Utils
{
    public class ModuleAsteroidFix : PartModule
    {
        [KSPField(isPersistant = true)]
        public string seed = "";

        ModuleAsteroid asteroid;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            asteroid = part.Modules.GetModule<ModuleAsteroid>();
            if(asteroid && asteroid.seed > 0)
            {
                if(string.IsNullOrEmpty(seed))
                    seed = string.Format("{0:D}", (int)asteroid.seed);
                if(int.TryParse(seed, out asteroid.seed))
                {
                    asteroid.OnAwake();
                    var pasteroid = part.GetComponentInChildren<PAsteroid>();
//                    this.Log(DebugUtils.formatTransformTree(part.transform));//debug
                    if(pasteroid)
                    {
                        pasteroid.transform.parent = null;
                        if(pasteroid.gameObject)
                            Destroy(pasteroid.gameObject);
                    }
                    asteroid.OnStart(state);
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            if(asteroid && asteroid.seed > 0 && string.IsNullOrEmpty(seed))
                seed = string.Format("{0:D}", (int)asteroid.seed);
            base.OnSave(node);
        }
    }
}


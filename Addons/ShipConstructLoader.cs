//   ShipConstructLoader.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System;
using System.Collections.Generic;
using AT_Utils;
using KSP.UI.Screens;
using UnityEngine;

namespace AT_Utils
{
    public class ShipConstructLoader : MonoBehaviour
    {
        SubassemblySelector subassembly_selector;
        CraftBrowserDialog vessel_selector;
        public EditorFacility Facility { get; private set; }
        public Action<ShipConstruct> process_construct = delegate {};

        public void Awake()
        {
            subassembly_selector = gameObject.AddComponent<SubassemblySelector>();
            subassembly_selector.Show(false);
        }

        void OnDestroy()
        {
            Destroy(subassembly_selector);
        }

        void subassembly_selected(ShipTemplate template) => 
        ShipConstruction
            .CreateConstructFromTemplate(template, construct => 
                                         StartCoroutine(delayed_process_construct(construct)));

        void vessel_selected(string filename, CraftBrowserDialog.LoadType t)
        {
            vessel_selector = null;
            //load vessel config
            var node = ConfigNode.Load(filename);
            if(node == null) return;
            var construct = new ShipConstruct();
            if(!construct.LoadShip(node))
            {
                Utils.Log("Unable to load ShipConstruct from {}. " +
                          "This usually means that some parts are missing " +
                          "or some modules failed to initialize.", filename);
                Utils.Message("Unable to load {0}", filename);
                return;
            }
            //check if it's possible to launch such vessel
            bool cant_launch = false;
            var preFlightCheck = new PreFlightCheck(new Callback(() => cant_launch = false), new Callback(() => cant_launch = true));
            preFlightCheck.AddTest(new PreFlightTests.ExperimentalPartsAvailable(construct));
            preFlightCheck.RunTests();
            //cleanup loaded parts and try to store construct
            if(cant_launch) construct.Unload();
            else StartCoroutine(delayed_process_construct(construct));
        }
        void selection_canceled() { vessel_selector = null; }

        public static void SetShipRendering(IShipconstruct construct, bool render)
        {
            foreach(var p in construct.Parts)
            {
                var renderers = p.GetComponentsInChildren<Renderer>();
                for(int i = 0, len = renderers.Length; i < len; i++)
                    renderers[i].enabled = render;
            }
        }

        IEnumerator<YieldInstruction> delayed_process_construct(ShipConstruct construct)
        {
            if(construct == null) yield break;
            var lock_name = "construct_loading"+GetHashCode();
            Utils.LockControls(lock_name);
            SetShipRendering(construct, false);
            if(HighLogic.LoadedSceneIsEditor)
                for(int i = 0; i < 3; i++) yield return null;
            SetShipRendering(construct, true);
            process_loaded_construct(construct);
            Utils.LockControls(lock_name, false);
        }

        void process_loaded_construct(ShipConstruct construct)
        {
            if(construct != null)
                process_construct(construct);
        }

        public void Draw() => subassembly_selector.Draw(subassembly_selected);
        public void Show(bool show) => subassembly_selector.Show(show);

        public void SelectVessel()
        {
            if(vessel_selector != null) return;
            var facility = EditorLogic.fetch != null?
                                      EditorLogic.fetch.ship.shipFacility : 
                                      EditorFacility.VAB;
            vessel_selector =
                CraftBrowserDialog.Spawn(
                    facility,
                    HighLogic.SaveFolder,
                    vessel_selected,
                    selection_canceled, false);
        }

        public void SelectSubassembly()
        {
            subassembly_selector.Show(true);
        }
    }
}

//   ScenarioTester.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SaveUpgradePipeline;

namespace AT_Utils
{
    public interface ITestScenario
    {
        string Status { get; }
        bool NeedsFixedUpdate { get; }
        bool NeedsUpdate { get; }

        string Setup();
        bool Update(System.Random RND);
        void Cleanup();
        void Draw();
    }

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class ScenarioTester : AddonWindowBase<ScenarioTester>
    {
        static SortedList<string, SortedList<string, ITestScenario>> scenarios;

        System.Random RND;
        protected ITestScenario current_test;
        string current_assembly = "";
        string current_test_name = "";

        public override void Awake()
        {
            base.Awake();
            if(Instance != this) return;
            if(scenarios == null)
            {
                scenarios = new SortedList<string, SortedList<string, ITestScenario>>();
                foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = assembly.GetTypes(); }
                    catch(Exception) { types = Type.EmptyTypes; }
                    var aname = Utils.ParseCamelCase(assembly.GetName().Name);
                    foreach(var type in types) 
                    {
                        if(!type.IsAbstract &&
                           type.GetInterfaces().Contains(typeof(ITestScenario)))
                        {
                            var cinfo = type.GetConstructor(Type.EmptyTypes);
                            if(cinfo != null)
                            {
                                SortedList<string,ITestScenario> lst;
                                if(!scenarios.TryGetValue(aname, out lst))
                                {
                                    lst = new SortedList<string, ITestScenario>();
                                    scenarios.Add(aname, lst);
                                }
                                lst.Add(Utils.ParseCamelCase(type.Name).Replace("_", " "), 
                                        cinfo.Invoke(null) as ITestScenario);
                                Utils.Log("Added {}.{} to testing scenarios.", aname, type.Name);
                            }
                        }
                    }
                }
                if(scenarios.Count == 0) Destroy(gameObject);
                else
                {
                    DontDestroyOnLoad(this);
                    Show(true);
                }
            }
            else Destroy(gameObject);
        }

        protected virtual void setup_test(ITestScenario test)
        {
            var msg = test.Setup();
            if(string.IsNullOrEmpty(msg))
            {
                current_test = test;
                RND = new System.Random(DateTime.Now.Second);
            }
            else Utils.Message(msg);
        }

        protected virtual void stop_test()
        {
            if(current_test == null) return;
            current_test.Cleanup();
            current_test = null;
            RND = null;
        }

        void test_loop(ref ITestScenario test)
        {
            if(test.Update(RND)) return;
            test.Cleanup();
            test = null;
        }

        bool unpaused => !FlightDriver.Pause;

        protected virtual void Update()
        {
            if(unpaused && current_test != null && current_test.NeedsUpdate)
                test_loop(ref current_test);
        }

        protected virtual void FixedUpdate()
        {
            if(unpaused && current_test != null && current_test.NeedsFixedUpdate)
                test_loop(ref current_test);
        }

        protected override void draw_gui()
        {
            LockControls();
            WindowPos = GUILayout.Window(GetInstanceID(),
                                         WindowPos, MainWindow,
                                         "Test Scenarios",
                                         GUILayout.Width(320),
                                         GUILayout.Height(100)).clampToScreen();
        }

        protected virtual void MainWindow(int windowID)
        {
            GUILayout.BeginVertical();
            var old_assembly = current_assembly;
            current_assembly = Utils.LeftRightChooser(current_assembly, scenarios, "Select assembly");
            if(old_assembly != current_assembly) current_test_name = "";
            SortedList<string, ITestScenario> tests = null;
            if(scenarios.TryGetValue(current_assembly, out tests))
                current_test_name = Utils.LeftRightChooser(current_test_name, tests, "Select test to run");
            if(current_test != null)
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Stop", Styles.danger_button, GUILayout.ExpandWidth(true)))
                    stop_test();
                if(GUILayout.Button("Restart", Styles.danger_button, GUILayout.ExpandWidth(true)))
                {
                    current_test.Cleanup();
                    current_test.Setup();
                }
                GUILayout.EndHorizontal();
                GUILayout.Label(current_test != null? current_test.Status : "", 
                                Styles.boxed_label, GUILayout.ExpandWidth(true));
            }
            else if(tests != null)
            {
                ITestScenario scn;
                if(tests.TryGetValue(current_test_name, out scn))
                {
                    scn.Draw();
                    if(GUILayout.Button("Run the test", Styles.active_button, GUILayout.ExpandWidth(true)))
                        setup_test(scn);
                }
            }
            GUILayout.EndVertical();
            TooltipsAndDragWindow();
        }

        #region Helpers
        public static bool LoadGame(string filename)
        {
            ConfigNode configNode = GamePersistence.LoadSFSFile(filename, HighLogic.SaveFolder);
            if (configNode == null)
            {
                ScreenMessages.PostScreenMessage("<color=orange>Unable to load the save: " + filename, 5f, ScreenMessageStyle.UPPER_LEFT);
                return false;
            }
            var v = NodeUtil.GetCfgVersion(configNode, LoadContext.SFS);
            KSPUpgradePipeline.Process(configNode,  HighLogic.SaveFolder, LoadContext.SFS, 
                                       node => onPipelineFinished(node, filename, v), 
                                       (opt, node) => 
                                       ScreenMessages.PostScreenMessage(string.Format("<color=orange>Unable to load the save: {0}\n" +
                                                                                      "KSPUpgradePipeline finished with error.", filename), 
                                                                        5f, ScreenMessageStyle.UPPER_LEFT));
            return true;
        }

        static void onPipelineFinished(ConfigNode node, string saveName, Version originalVersion)
        {
            var game = GamePersistence.LoadGameCfg(node, saveName, true, false);
            if(game != null && game.flightState != null)
            {
                if (game.compatible)
                {
                    GamePersistence.UpdateScenarioModules(game);
                    if(node != null)
                        GameEvents.onGameStatePostLoad.Fire(node);
                    if(game.startScene != GameScenes.FLIGHT &&
                       originalVersion >= new Version(0, 24, 0))
                    {
                        GamePersistence.SaveGame(game, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
                        HighLogic.LoadScene(GameScenes.SPACECENTER);
                        return;
                    }
                }
                FlightDriver.StartAndFocusVessel(game, game.flightState.activeVesselIdx);
            }
        }
        #endregion
    }
}


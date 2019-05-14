////   GlobalsEditor.cs
////
////  Author:
////       Allis Tauri <allista@gmail.com>
////
////  Copyright (c) 2018 Allis Tauri
//using UnityEngine;

//namespace AT_Utils
//{
//    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
//    public class GlobalsEditor : AddonWindowBase<GlobalsEditor>
//    {
//        ConfigNodeObjectGUI UI;

//        public override void Awake()
//        {
//            width = 600;
//            height = 400;
//            base.Awake();
//            Show(true);
//        }

//        public override void Show(bool show)
//        {
//            base.Show(show);
//            if(show)
//                UI = ConfigNodeObjectGUI.FromObject(AT_UtilsGlobals.Instance);
//            else
//            {
//                UI = null;
//                AT_UtilsGlobals.Load();
//            }
//        }

//        void drawGlobalsUI(int windowID)
//        {
//            GUILayout.BeginVertical();
//            if(GUILayout.Button("Close", Styles.close_button, GUILayout.ExpandWidth(true)))
//                Show(false);
//            UI.Draw();
//            if(GUILayout.Button("Save", Styles.danger_button, GUILayout.ExpandWidth(true)))
//                AT_UtilsGlobals.SaveOverride();
//            GUILayout.EndVertical();
//            TooltipsAndDragWindow();
//        }

//        protected override void draw_gui()
//        {
//            WindowPos = GUILayout.Window(GetInstanceID(), WindowPos, 
//                                         drawGlobalsUI, "AT Utils Global Setting",
//                                         GUILayout.Width(width), 
//                                         GUILayout.Height(height))
//                           .clampToScreen();
//        }
//    }
//}

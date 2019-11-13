//   ColorListWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri
using AT_Utils.UI;

namespace AT_Utils
{
    public class ColorListWindow : UIWindowBase<ColorList>
    {
        private static ColorListWindow instance;
        public static ColorListWindow Instance => instance ?? (instance = new ColorListWindow());

        protected override void init_controller()
        {
            Controller.SetTitle("Color Scheme of AT Mods");
            Controller.closeButton.onClick.AddListener(OnClose);
            Controller.saveButton.onClick.AddListener(OnSave);
            Controller.resetButton.onClick.AddListener(OnReset);
            Controller.restoreButton.onClick.AddListener(OnRestore);
        }

        private ColorListWindow(): base(AT_UtilsGlobals.Instance.AssetBundle) { }

        void OnClose()
        {
            Close();
            AT_UtilsGlobals.Load();
            Styles.Reset();
        }

        void OnReset()
        {
            Colors.SetDefaults();
            Styles.Reset();
        }

        void OnRestore()
        {
            AT_UtilsGlobals.Restore();
            Styles.Reset();
        }

        void OnSave()
        {
            AT_UtilsGlobals.Save("Colors");
            Styles.Reset();
        }
    }
}

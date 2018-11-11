//   Colorizer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;

namespace AT_Utils.UI
{
    public abstract class ColorizerBase : MonoBehaviour
    {
        public string Color;
        ColorSetting setting;

        void Awake()
        {
            if(!string.IsNullOrEmpty(Color))
            {
                setting = Colors.GetColor(Color);
                if(setting != null)
                {
                    onColorChanged(setting);
                    setting.onColorChanged.AddListener(onColorChanged);
                }
            }
        }

        void OnDestroy()
        {
            if(setting != null)
                setting.onColorChanged.RemoveListener(onColorChanged);
        }

        protected abstract void onColorChanged(Color color);
    }
}

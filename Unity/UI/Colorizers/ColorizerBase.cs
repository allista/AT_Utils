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
        protected ColorSetting setting { get; private set; }

        protected virtual void Awake()
        {
            if(string.IsNullOrEmpty(Color))
                return;
            setting = Colors.GetColor(Color);
            if(setting == null)
                return;
            onColorChanged(setting);
            setting.onColorChanged.AddListener(onColorChanged);
        }

        protected virtual void OnDestroy()
        {
            setting?.onColorChanged.RemoveListener(onColorChanged);
        }

        protected abstract void onColorChanged(Color color);
    }
}

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
            SetColor(Color);
        }

        protected virtual void OnDestroy()
        {
            setting?.onColorChanged.RemoveListener(onColorChanged);
        }

        public void SetColor(ColorSetting newSetting)
        {
            newSetting ??= Colors.Neutral;
            Color = newSetting.html;
            updateSetting(newSetting);
        }

        public void SetColor(string color)
        {
            Color = color;
            var newSetting = Colors.GetColor(Color);
            if(newSetting != null)
                updateSetting(newSetting);
            else
                SetColor(new ColorSetting(Color));
        }

        private void updateSetting(ColorSetting newSetting)
        {
            setting?.onColorChanged.RemoveListener(onColorChanged);
            setting = newSetting;
            onColorChanged(setting);
            setting.onColorChanged.AddListener(onColorChanged);
        }

        public void UpdateColor() => onColorChanged(setting);

        protected abstract void onColorChanged(Color color);
    }
}

//   ToggleColorizer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class ToggleColorizer : MonoBehaviour
    {
        [SerializeField]
        Toggle toggle;

        [SerializeField]
        Text toggleText;

        void Awake()
        {
            toggle.onValueChanged.AddListener(onToggle);
            Colors.Enabled.onColorChanged.AddListener(c => onToggle(toggle.isOn));
            Colors.Active.onColorChanged.AddListener(c => onToggle(toggle.isOn));
            onToggle(toggle.isOn);
        }

        void onToggle(bool isOn)
        {
            changeColor(isOn? Colors.Enabled: Colors.Active);
        }

        void changeColor(Color color)
        {
            toggleText.color = color;
            var colors = toggle.colors;
            colors.highlightedColor = color;
            toggle.colors = colors;
        }
    }
}

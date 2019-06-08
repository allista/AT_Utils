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
        Toggle toggle;

        [SerializeField]
        Text toggleText;

        void Awake()
        {
            toggle = gameObject.GetComponent<Toggle>();
            if(toggle != null)
            {
                toggle.onValueChanged.AddListener(isOn => UpdateColor());
                Colors.Enabled.onColorChanged.AddListener(c => UpdateColor());
                Colors.Active.onColorChanged.AddListener(c => UpdateColor());
                UpdateColor();
            }
            else
                enabled = false;
        }

        public void SetInteractable(bool interactable)
        {
            toggle.interactable = interactable;
            UpdateColor();
        }

        void changeColor(Color color)
        {
            toggleText.color = color;
            var colors = toggle.colors;
            colors.highlightedColor = color;
            toggle.colors = colors;
        }

        public void UpdateColor()
        {
            changeColor(toggle.isOn
                        ? Colors.Enabled
                        : (toggle.interactable
                           ? Colors.Active
                           : Colors.Inactive));
        }
    }
}

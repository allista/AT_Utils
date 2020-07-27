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
    [RequireComponent(typeof(Toggle))]
    public class ToggleColorizer : MonoBehaviour, IColorizer
    {
        Toggle toggle;
        public Text toggleText;


        void Awake()
        {
            toggle = gameObject.GetComponent<Toggle>();
            if(toggle != null)
            {
                toggle.onValueChanged.AddListener(onStateChanged);
                Colors.Enabled.onColorChanged.AddListener(onColorChanged);
                Colors.Active.onColorChanged.AddListener(onColorChanged);
                Colors.Inactive.onColorChanged.AddListener(onColorChanged);
                UpdateColor();
            }
            else
                enabled = false;
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(onStateChanged);
            Colors.Enabled.onColorChanged.RemoveListener(onColorChanged);
            Colors.Active.onColorChanged.RemoveListener(onColorChanged);
            Colors.Inactive.onColorChanged.RemoveListener(onColorChanged);
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
            if(toggle == null)
                return;
            changeColor(toggle.isOn
                        ? Colors.Enabled
                        : (toggle.interactable
                           ? Colors.Active
                           : Colors.Inactive));
        }

        private void onColorChanged(Color c) => UpdateColor();
        private void onStateChanged(bool isOn) => UpdateColor();
    }
}

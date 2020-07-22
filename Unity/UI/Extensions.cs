//   Extensions.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using System.Collections.Generic;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public static class Extensions
    {
        public static void SetIsOnAndColorWithoutNotify(this Toggle toggle, bool isOn)
        {
            if(toggle.isOn == isOn)
                return;
            toggle.SetIsOnWithoutNotify(isOn);
            toggle.GetComponent<ToggleColorizer>()?.UpdateColor();
        }

        public static void SetInteractable(this Selectable selectable, bool interactable)
        {
            if(selectable.interactable == interactable)
                return;
            selectable.interactable = interactable;
            selectable.GetComponent<Colorizer>()?.UpdateColor();
        }

        public static void SetOptionsSafe(this Dropdown dropdown, List<Dropdown.OptionData> newOptions)
        {
            dropdown.options = newOptions;
            if(dropdown.value >= dropdown.options.Count)
                dropdown.SetValueWithoutNotify(dropdown.options.Count - 1);
        }
    }
}

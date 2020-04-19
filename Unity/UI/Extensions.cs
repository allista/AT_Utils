//   Extensions.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

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
        }
    }
}

//   Extensions.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri
using System.Reflection;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public static class Extensions
    {
        static readonly MethodInfo toggleSet = typeof(Toggle)
            .GetMethod("Set",
                       BindingFlags.NonPublic | BindingFlags.Instance,
                       null,
                       new[] { typeof(bool), typeof(bool) },
                       null);

        public static void SetIsOnWithoutNotify(this Toggle toggle, bool isOn)
        {
            if(toggle.isOn != isOn)
            {
                toggleSet.Invoke(toggle, new object[] { isOn, false });
                toggle.GetComponent<ToggleColorizer>()?.UpdateColor();
            }
        }
    }
}

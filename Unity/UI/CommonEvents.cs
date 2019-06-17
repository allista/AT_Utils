//   CommonEvents.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri
using System;
using UnityEngine.Events;
namespace AT_Utils.UI
{
    [Serializable]
    public class FloatEvent : UnityEvent<float>
    {
    }

    [Serializable]
    public class BoolEvent : UnityEvent<bool>
    {
    }
}

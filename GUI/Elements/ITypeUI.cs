//   IFieldUI.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using System.Collections.Generic;
using System.Reflection;

namespace AT_Utils
{
    public interface I_UI
    {
        void Draw();
    }

    public interface ITypeUI<T>
    {
        bool Draw();
        T Value { get; set; }
    }

//    public static class FieldUIs
//    {
//        public delegate IFieldUI ui_constructor()
//        Dictionary<type,
//        
//    }
}


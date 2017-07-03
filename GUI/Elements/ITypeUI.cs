//   IFieldUI.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

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
}


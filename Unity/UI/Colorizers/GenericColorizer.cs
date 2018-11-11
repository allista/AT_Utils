//   Colorizer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class Colorizer : ColorizerBase
    {
        [SerializeField]
        Text text;

        [SerializeField]
        Image image;

        [SerializeField]
        Selectable selectable;

        protected override void onColorChanged(Color color)
        {
            if(text != null)
                text.color = color;
            if(image != null)
                image.color = color;
            if(selectable != null)
            {
                var colors = selectable.colors;
                colors.highlightedColor = color;
                selectable.colors = colors;
            }
        }
    }
}

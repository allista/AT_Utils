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
        public Text text;
        public Image image;
        public Selectable selectable;

        public void SetInteractable(bool interactable)
        {
            if(selectable == null
               || selectable.interactable == interactable)
                return;
            selectable.interactable = interactable;
            onColorChanged(setting);
        }

        protected override void onColorChanged(Color color)
        {
            if(selectable != null 
               && !selectable.interactable)
                color = Colors.Inactive;
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

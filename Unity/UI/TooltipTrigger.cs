//   TooltipTrigger.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using UnityEngine;
using UnityEngine.EventSystems;

namespace AT_Utils.UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string text;
        public float delay = 0.3f;
        public TooltipView.TooltipPosition position = TooltipView.TooltipPosition.BOTTOM;
        private float enterTime = -1;

        private void Update()
        {
            if(enterTime < 0
               || Time.realtimeSinceStartup - enterTime < delay)
                return;
            enterTime = -1;
            show();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!string.IsNullOrEmpty(text))
                enterTime = Time.realtimeSinceStartup;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            enterTime = -1;
            if(TooltipView.Instance != null)
                TooltipView.Instance.HideTooltip();
        }

        public void SetText(string newText)
        {
            var oldText = text;
            text = newText;
            if(TooltipView.IsShown
               && oldText == TooltipView.CurrentTooltip)
                show();
        }

        private void show()
        {
            if(TooltipView.Instance != null)
                TooltipView.Instance.ShowTooltip(text, position);
        }
    }
}

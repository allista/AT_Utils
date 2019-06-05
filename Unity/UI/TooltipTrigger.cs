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
        float enterTime = -1;

        void Update()
        {
            if(enterTime > 0 && Time.realtimeSinceStartup - enterTime > delay)
            {
                enterTime = -1;
                TooltipView.Instance?.ShowTooltip(text, position);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!string.IsNullOrEmpty(text))
                enterTime = Time.realtimeSinceStartup;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            enterTime = -1;
            TooltipView.Instance?.HideTooltip();
        }
    }
}

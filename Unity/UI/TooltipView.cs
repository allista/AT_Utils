//   TooltipView.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AT_Utils.UI
{
    public class TooltipView : MonoBehaviour, IPointerExitHandler
    {
        public enum TooltipPosition { BOTTOM, TOP };
        public static TooltipView Instance { get; private set; }

        Canvas canvas;
        public Text tooltipText;
        RectTransform rectT => transform as RectTransform;

        void Awake()
        {
            if(Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            canvas = GetComponentInParent<Canvas>();
            HideTooltip();
        }


        IEnumerator set_position_in_bounds(TooltipPosition position)
        {
            yield return null;
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
            rectT.position = offset_position(pos, position);
            var old_pos = rectT.position;
            ScreenBoundRect.ClampToScreen(rectT, canvas);
        }

        Vector3 offset_position(Vector3 position, TooltipPosition offset)
        {
            switch(offset)
            {
            case TooltipPosition.BOTTOM:
                position.y -= rectT.rect.height;
                break;
            case TooltipPosition.TOP:
                position.y += rectT.rect.height+5;
                break;
            default:
                goto case TooltipPosition.BOTTOM;
            }
            return position;
        }

        public void ShowTooltip(string text, TooltipPosition position)
        {
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, new Vector2(-Screen.width, 0), canvas.worldCamera, out pos);
            transform.position = pos;
            tooltipText.text = text;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StartCoroutine(set_position_in_bounds(position));
        }

        public void HideTooltip()
        {
            if(!RectTransformUtility.RectangleContainsScreenPoint(rectT, Input.mousePosition))
                gameObject.SetActive(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }
    }
}

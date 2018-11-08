using UnityEngine;
using UnityEngine.EventSystems;

namespace AT_Utils.UI
{
    public class DragablePanel : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        Vector2 pointerOffset;
        RectTransform parentRectTransform;

        public virtual void OnPointerDown(PointerEventData data)
        {
            transform.SetAsLastSibling();
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(transform as RectTransform,
                                                    data.position, data.pressEventCamera,
                                                    out pointerOffset);

        }

        public virtual void OnDrag(PointerEventData data)
        {
            if(parentRectTransform == null && transform.parent != null)
                parentRectTransform = transform.parent as RectTransform;
            if(parentRectTransform != null)
            {
                Vector2 localPointerPosition;
                if(RectTransformUtility
                .ScreenPointToLocalPointInRectangle(parentRectTransform,
                                                    ClampToWindow(data), data.pressEventCamera,
                                                    out localPointerPosition))
                    transform.localPosition = localPointerPosition - pointerOffset;
            }
        }

        protected Vector2 ClampToWindow(PointerEventData data)
        {
            var rawPointerPosition = data.position;
            Vector3[] corners = new Vector3[4];
            parentRectTransform.GetWorldCorners(corners);
            return new Vector2(Mathf.Clamp(rawPointerPosition.x, corners[0].x, corners[2].x),
                               Mathf.Clamp(rawPointerPosition.y, corners[0].y, corners[2].y));
        }
    }
}

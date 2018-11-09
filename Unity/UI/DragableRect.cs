using UnityEngine;
using UnityEngine.EventSystems;

namespace AT_Utils.UI
{
    public class DragableRect : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        protected RectTransform parentTransform;
        protected RectTransform rectTransform;
        protected Vector2 pointerOffset;

        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;
            if(rectTransform == null)
                enabled = false;
        }

        //this event fires when a drag event begins
        public virtual void OnPointerDown(PointerEventData data)
        {
            if(parentTransform == null)
            {
                parentTransform = rectTransform.parent as RectTransform;
                if(parentTransform == null)
                {
                    enabled = false;
                    return;
                }
            }
            rectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out pointerOffset);
        }

        //this event fires while we're dragging. It's constantly moving the UI to a new position
        public virtual void OnDrag(PointerEventData data)
        {
            Vector2 pointer;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentTransform, data.position, data.pressEventCamera, out pointer);
            rectTransform.localPosition = pointer - pointerOffset;
        }
    }

    public class ScreenBoundRect : DragableRect
    {
        bool screen_space;
        protected Canvas canvas;

        public override void OnPointerDown(PointerEventData data)
        {
            base.OnPointerDown(data);
            if(parentTransform != null && canvas == null)
            {
                canvas = parentTransform.GetComponent<Canvas>();
                if(canvas != null)
                    screen_space = canvas.renderMode != RenderMode.WorldSpace;
            }
        }

        public override void OnDrag(PointerEventData data)
        {
            base.OnDrag(data);
            if(screen_space)
            {
                var new_pos = rectTransform.position;
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);
                var top_left = corners[1];
                var bottom_right = corners[3];
                if(top_left.x < -Screen.width / 2)
                    new_pos.x -= top_left.x + Screen.width / 2;
                else if(bottom_right.x > Screen.width / 2)
                    new_pos.x -= bottom_right.x - Screen.width / 2;
                if(top_left.y > Screen.height / 2)
                    new_pos.y -= top_left.y - Screen.height / 2;
                else if(bottom_right.y < -Screen.height / 2)
                    new_pos.y -= bottom_right.y + Screen.height / 2;
                rectTransform.position = new_pos;
            }
        }
    }
}

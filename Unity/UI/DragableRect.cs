using UnityEngine;
using UnityEngine.EventSystems;

namespace AT_Utils.UI
{
    public class DragableRect : OnHoverTrigger, IBeginDragHandler, IDragHandler
    {
        protected RectTransform rectTransform;
        protected Vector3 pointerOffset;
        protected Vector3 positionOffset;

        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;
            if(rectTransform == null)
                enabled = false;
        }

        //this event fires when a drag event begins
        public virtual void OnBeginDrag(PointerEventData data)
        {
            rectTransform.SetAsLastSibling();
            positionOffset = rectTransform.position;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform,
                data.position,
                data.pressEventCamera,
                out pointerOffset);
        }

        //this event fires while we're dragging. It's constantly moving the UI to a new position
        public virtual void OnDrag(PointerEventData data)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform,
                data.position,
                data.pressEventCamera,
                out var pointer);
            rectTransform.position = positionOffset + pointer - pointerOffset;
        }
    }

    public class ScreenBoundRect : DragableRect
    {
        private bool screen_space;
        private Canvas _canvas;

        protected Canvas canvas
        {
            get
            {
                if(_canvas != null)
                    return _canvas;
                _canvas = GetComponentInParent<Canvas>();
                if(_canvas != null)
                    screen_space = _canvas.renderMode != RenderMode.WorldSpace;
                return _canvas;
            }
        }

        protected virtual void Start()
        {
            if(canvas != null && screen_space)
                ClampToScreen(rectTransform, canvas);
        }

        public static void ClampToScreen(RectTransform rectTransform, Canvas canvas)
        {
            var screen_pos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position);
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var top_left = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
            var bottom_right = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
            var w = Screen.width;
            var h = Screen.height;
            if(top_left.x < 0)
                screen_pos.x -= top_left.x;
            else if(bottom_right.x > w)
                screen_pos.x -= bottom_right.x - w;
            if(top_left.y > h)
                screen_pos.y -= top_left.y - h;
            else if(bottom_right.y < 0)
                screen_pos.y -= bottom_right.y;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform,
                screen_pos,
                canvas.worldCamera,
                out var new_pos);
            rectTransform.position = new_pos;
        }

        public override void OnDrag(PointerEventData data)
        {
            base.OnDrag(data);
            if(canvas != null && screen_space)
                ClampToScreen(rectTransform, canvas);
        }
    }
}

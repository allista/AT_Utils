using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AT_Utils.UI
{
    public class OnHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public class PointerEvent : UnityEvent<PointerEventData> { }

        public PointerEvent onPointerEnterEvent = new PointerEvent();
        public PointerEvent onPointerExitEvent = new PointerEvent();


        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnterEvent.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExitEvent.Invoke(eventData);
        }
    }
}

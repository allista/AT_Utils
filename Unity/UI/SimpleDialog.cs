using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class DialogFactory : MonoBehaviour
    {
        private static DialogFactory Instance { get; set; }

        protected virtual RectTransform dialogParent => GetComponentInParent<Canvas>()?.transform as RectTransform;

        public GameObject
            prefab;

        protected virtual void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if(prefab != null)
                prefab.SetActive(false);
        }

        protected virtual void setupDialog(SimpleDialog dialog) { }

        private static SimpleDialog newInstance()
        {
            if(Instance == null || Instance.prefab == null)
                return null;
            var obj = Instantiate(Instance.prefab, Instance.dialogParent);
            if(obj == null)
                return null;
            var dialog = obj.GetComponent<SimpleDialog>();
            Instance.setupDialog(dialog);
            return dialog;
        }

        public static SimpleDialog Create(
            string message,
            string title,
            UnityAction onConfirm,
            UnityAction onCancel = null,
            UnityAction onClose = null,
            string confirmText = "Yes",
            string cancelText = "No",
            bool destroyOnClose = true
        )
        {
            var dialog = newInstance();
            if(dialog == null)
                return null;
            dialog.confirmText.text = confirmText ?? "Yes";
            if(string.IsNullOrEmpty(cancelText))
                dialog.cancelText.gameObject.SetActive(false);
            else
                dialog.cancelText.text = cancelText;
            dialog.title.text = title;
            dialog.message.text = message;
            dialog.DestroyOnClose = destroyOnClose;
            dialog.onConfirm.AddListener(onConfirm);
            if(onCancel != null)
                dialog.onCancel.AddListener(onCancel);
            if(onClose != null)
                dialog.onClose.AddListener(onClose);
            return dialog;
        }

        public static SimpleDialog Show(
            string message,
            string title,
            UnityAction onConfirm,
            UnityAction onCancel = null,
            UnityAction onClose = null,
            string confirmText = "Yes",
            string cancelText = "No",
            bool destroyOnClose = true
        )
        {
            var dialog = Create(message, title, onConfirm, onCancel, onClose, confirmText, cancelText, destroyOnClose);
            if(dialog == null)
                return null;
            dialog.Show();
            return dialog;
        }

        public static SimpleDialog Info(
            string message,
            string title
        ) =>
            Show(message, title, null, null, null, "Close", null);

        public static SimpleDialog Warning(
            string message,
            string title = null
        ) =>
            Show(message, title ?? "Warning", null, null, null, "Close", null);

        public static SimpleDialog Danger(
            string message,
            UnityAction onConfirm,
            UnityAction onCancel = null,
            UnityAction onClose = null,
            string title = null
        )
        {
            var dialog = Create(message, title ?? "Warning", onConfirm, onCancel, onClose, "Yes", "Cancel");
            if(dialog == null)
                return null;
            dialog.confirmButtonColorizer.SetColor(Colors.Danger);
            dialog.cancelButtonColorizer.SetColor(Colors.Good);
            dialog.Show();
            return dialog;
        }
    }

    public class SimpleDialog : ScreenBoundRect
    {
        public UnityEvent onConfirm = new UnityEvent();
        public UnityEvent onCancel = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();

        public Button
            confirmButton,
            cancelButton;

        public Colorizer
            confirmButtonColorizer,
            cancelButtonColorizer;

        public Text
            title,
            message,
            confirmText,
            cancelText;

        public bool DestroyOnClose = true;

        protected override void Awake()
        {
            base.Awake();
            confirmButton.onClick.AddListener(handleConfirm);
            cancelButton.onClick.AddListener(handleCancel);
        }

        private void OnDestroy()
        {
            if(gameObject.activeInHierarchy)
                sendEvent(onClose, nameof(onClose));
            onConfirm.RemoveAllListeners();
            onCancel.RemoveAllListeners();
            onClose.RemoveAllListeners();
            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
        }

        private void sendEvent(UnityEvent @event, string eventName)
        {
            try
            {
                @event.Invoke();
            }
            catch(Exception e)
            {
                Debug.LogError($"[AT_Utils: SimpleDialog] exception in {eventName}: {e}", this);
            }
        }

        private void close()
        {
            sendEvent(onClose, nameof(onClose));
            gameObject.SetActive(false);
            if(DestroyOnClose)
                Destroy(gameObject);
        }

        private void handleConfirm()
        {
            sendEvent(onConfirm, nameof(onConfirm));
            close();
        }

        private void handleCancel()
        {
            sendEvent(onCancel, nameof(onCancel));
            close();
        }

        public void Show(string newMessage, string newTitle = null)
        {
            if(!string.IsNullOrEmpty(newMessage))
                message.text = newMessage;
            if(!string.IsNullOrEmpty(newTitle))
                title.text = newTitle;
            Show();
        }

        public void Show() => gameObject.SetActive(true);

        public void Close() => gameObject.SetActive(false);
    }
}

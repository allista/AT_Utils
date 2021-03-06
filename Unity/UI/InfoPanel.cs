using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils.UI
{
    [RequireComponent(typeof(ClickableLabel))]
    public class InfoPanel : PanelledUI
    {
        private ClickableLabel message;
        private readonly List<string> content = new List<string>();

        private void Awake()
        {
            message = GetComponent<ClickableLabel>();
            message.onLabelClicked.AddListener(onMessageClicked);
        }

        private void OnDestroy()
        {
            message.onLabelClicked.RemoveAllListeners();
        }

        private void onMessageClicked()
        {
            content.Clear();
            setMessageText("");
        }

        private void setMessageText(string text)
        {
            message.text.text = text;
            gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void SetMessage(string newMessage)
        {
            content.Clear();
            content.Add(newMessage);
            setMessageText(newMessage);
        }

        public void AddMessage(string newMessage)
        {
            if(string.IsNullOrEmpty(newMessage))
                return;
            content.Add(newMessage);
            setMessageText(string.Join("\n", content));
        }
    }
}

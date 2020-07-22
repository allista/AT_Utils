using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class DebugWindow : ScreenBoundRect
    {
        public Text title;
        public Text content;
        public Button closeButton;

        public void SetContent(string text)
        {
            content.text = text;
        }
    }
}

using UnityEngine;

namespace AT_Utils
{
    public class SimpleWarning : SimpleDialog
    {
        public string Message;

        protected override void DrawContent()
        {
            Title = "Warning";
            GUILayout.Label(Message, Styles.rich_label, GUILayout.Width(width));
        }
    }
}

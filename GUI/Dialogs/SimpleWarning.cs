using UnityEngine;

namespace AT_Utils
{
    public class SimpleWarning : SimpleDialog
    {

        string message;

        protected override void DrawContent()
        {
            GUILayout.Label(message, Styles.rich_label, GUILayout.Width(width));
        }

        public Answer Draw(string message)
        {
            this.message = message;
            return draw("Warning");
        }
    }
}

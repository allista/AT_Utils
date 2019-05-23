using UnityEngine;

namespace AT_Utils
{
    public class SimpleWarning : SimpleDialog
    {
        public string Message = "";
        string tmpMessage;

        public SimpleWarning()
        {
            Title = "Warning";
        }

        protected override void DrawContent()
        {
            GUILayout.Label(tmpMessage ?? Message, Styles.rich_label, GUILayout.Width(width));
        }

        protected override void clear_tmp_state()
        {
            base.clear_tmp_state();
            tmpMessage = null;
        }

        public void Show(string message, Callback yes = null, Callback no = null)
        {
            if(message != null)
                tmpMessage = message;
            set_tmp_callback(yes, no);
            Show(true);
        }
    }
}

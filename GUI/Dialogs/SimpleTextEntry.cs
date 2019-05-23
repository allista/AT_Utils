using UnityEngine;

namespace AT_Utils
{
    public class SimpleTextEntry : SimpleDialog
    {
        public string Text = "";

        public SimpleTextEntry()
        { 
            Yes_text = "Accept";
            No_text = "Cancel";
        }

        protected override void DrawContent()
        {
            Text = GUILayout.TextField(Text, 50); 
        }

        public void Show(string text, Callback yes = null, Callback no = null)
        {
            if(text != null) 
                Text = text;
            set_tmp_callback(yes, no);
            Show(true);
        }
    }
}

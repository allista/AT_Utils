using UnityEngine;

namespace AT_Utils
{
    public class SimpleTextEntry : SimpleDialog
    {
        public SimpleTextEntry()
        { 
            Yes_text = "Accept";
            No_text = "Cancel";
        }

        protected override void DrawContent()
        {
            Text = GUILayout.TextField(Text, 50); 
        }

        public string Text = "";

        public Answer Draw(string title) { return draw(title); }
    }
}

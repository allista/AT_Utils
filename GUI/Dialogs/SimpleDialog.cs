using UnityEngine;

namespace AT_Utils
{
    public abstract class SimpleDialog : GUIWindowBase
    {
        public string Title = "Dialog";

        public enum Answer { None, Yes, No }
        protected string Yes_text = "Yes";
        protected string No_text = "No";

        public Callback yesCallback;
        Callback tmpYesCallback;

        public Callback noCallback;
        Callback tmpNoCallback;

        protected SimpleDialog()
        { 
            width = 400;
            WindowPos = new Rect(Screen.width/2-width/2, 100, width, 50);
        }

        public override void Awake()
        {
            base.Awake();
            Show(false);
        }

        public Answer Result { get; private set; }

        protected abstract void DrawContent();

        protected void set_tmp_callback(Callback yes, Callback no = null)
        {
            tmpYesCallback = yes;
            tmpNoCallback = no;
        }

        protected virtual void clear_tmp_state()
        {
            tmpYesCallback = null;
            tmpNoCallback = null;
        }

        void DialogWindow(int windowId)
        {
            GUILayout.BeginVertical();
            DrawContent();
            GUILayout.BeginHorizontal();
            Result = Answer.None;
            if(GUILayout.Button(No_text, Styles.close_button, GUILayout.ExpandWidth(false))) 
                Result = Answer.No;
            GUILayout.FlexibleSpace();
            if(GUILayout.Button(Yes_text, Styles.open_button, GUILayout.ExpandWidth(false))) 
                Result = Answer.Yes;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            TooltipsAndDragWindow();
        }

        protected void OnGUI()
        {
            if(doShow)
            {
                LockControls();
                WindowPos = GUILayout.Window(GetInstanceID(),
                                             WindowPos, DialogWindow,
                                             Title,
                                             GUILayout.Width(width)).clampToScreen();
                if(Result != Answer.None)
                {
                    Show(false);
                    Callback callback;
                    if(Result == Answer.Yes)
                        callback = tmpYesCallback ?? yesCallback;
                    else
                        callback = tmpNoCallback ?? noCallback;
                    callback?.Invoke();
                    clear_tmp_state();
                }
            }
            UnlockControls();
        }
    }
}

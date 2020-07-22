#if DEBUG
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    internal class DebugWindowUI : UIWindowBase<UI.DebugWindow>
    {
        public string title = "Debug Window";
        private string contentCache = string.Empty;

        public void SetContent(string content, MonoBehaviour host)
        {
            if(IsShown)
                Controller.SetContent(content);
            else
            {
                contentCache = content;
                Show(host);
            }
        }

        private DebugWindowUI(UIBundle bundle) : base(bundle) { }
        public DebugWindowUI() : this(AT_UtilsGlobals.Instance.AssetBundle) { }

        protected override void init_controller()
        {
            base.init_controller();
            Controller.title.text = title;
            Controller.closeButton.onClick.AddListener(Close);
            Controller.SetContent(contentCache);
        }
    }

    public static class DebugWindowController
    {
        private class HostMB : MonoBehaviour { }

        private static GameObject hostGO;
        private static HostMB host;
        private static readonly Dictionary<string, DebugWindowUI> UIs = new Dictionary<string, DebugWindowUI>();

        public static void PostMessage(string tag, string content)
        {
            if(hostGO == null)
            {
                hostGO = new GameObject("DebugWindowHost", typeof(HostMB));
                host = hostGO.GetComponent<HostMB>();
                hostGO.SetActive(true);
            }
            if(!UIs.TryGetValue(tag, out var ui))
            {
                ui = new DebugWindowUI { title = tag };
                UIs[tag] = ui;
            }
            ui.SetContent(content, host);
        }
    }
}
#endif

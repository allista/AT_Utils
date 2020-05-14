//   NamedDockingNode.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System.Collections.Generic;

namespace AT_Utils
{
    public class NamedDockingNode : ModuleDockingNode
    {
        [KSPField(isPersistant = true)]
        public string PortName = "";

        private SimpleTextEntry name_editor;

        private readonly Dictionary<BaseEvent, string> event_names = new Dictionary<BaseEvent, string>();

        private void update_event_names()
        {
            foreach(var e in event_names)
                e.Key.guiName = $"{e.Value}: {PortName}";
        }

        public override void OnAwake()
        {
            base.OnAwake();
            name_editor = gameObject.AddComponent<SimpleTextEntry>();
            name_editor.Title = "Rename Docking Port";
            name_editor.yesCallback = () =>
            {
                PortName = name_editor.Text;
                update_event_names();
            };
            foreach(var evt in Events)
                event_names.Add(evt, evt.guiName);
        }

        protected void OnDestroy()
        {
            Destroy(name_editor);
        }

        public override void OnStart(StartState st)
        {
            base.OnStart(st);
            if(string.IsNullOrEmpty(PortName))
                PortName = string.IsNullOrEmpty(referenceAttachNode) ?
                             nodeTransformName : referenceAttachNode;
            update_event_names();
        }

        [KSPEvent(guiName = "Rename Port", guiActive = true, guiActiveEditor = true, 
                  guiActiveUncommand = true, guiActiveUnfocused = true, unfocusedRange = 300,
                  active = true)]
        public void EditName()
        {
            name_editor.Text = PortName;
            name_editor.Toggle();
        }
    }
}

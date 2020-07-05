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
        [KSPField(isPersistant = true)] public string PortName = "";

        private SimpleTextEntry name_editor;

        private readonly Dictionary<BaseEvent, string> event_names = new Dictionary<BaseEvent, string>();
        private readonly Dictionary<BaseAction, string> action_names = new Dictionary<BaseAction, string>();
        private readonly Dictionary<BaseField, string> field_names = new Dictionary<BaseField, string>();

        private static void saveName<T>(T host, string name, IDictionary<T, string> map)
        {
            if(!string.IsNullOrEmpty(name))
                map.Add(host, name);
        }

        private string reName(string originalName) =>
            string.IsNullOrEmpty(PortName) ? originalName : $"{originalName}: {PortName}";

        private void update_names()
        {
            foreach(var i in event_names)
                i.Key.guiName = reName(i.Value);
            foreach(var i in action_names)
                i.Key.guiName = reName(i.Value);
            foreach(var i in field_names)
                i.Key.guiName = reName(i.Value);
        }

        public override void OnAwake()
        {
            base.OnAwake();
            name_editor = gameObject.AddComponent<SimpleTextEntry>();
            name_editor.Title = "Rename Docking Port";
            name_editor.yesCallback = () =>
            {
                PortName = name_editor.Text;
                update_names();
            };
            foreach(var item in Events)
                saveName(item, item.guiName, event_names);
            foreach(var item in Actions)
                saveName(item, item.guiName, action_names);
            foreach(var item in Fields)
                saveName(item, item.guiName, field_names);
        }

        protected virtual void OnDestroy()
        {
            Destroy(name_editor);
        }

        public override void OnStart(StartState st)
        {
            base.OnStart(st);
            if(string.IsNullOrEmpty(PortName))
                PortName = string.IsNullOrEmpty(referenceAttachNode) ? nodeTransformName : referenceAttachNode;
            update_names();
        }

        [KSPEvent(guiName = "Rename Port",
            guiActive = true,
            guiActiveEditor = true,
            guiActiveUncommand = true,
            guiActiveUnfocused = true,
            unfocusedRange = 300,
            active = true)]
        public void EditName()
        {
            name_editor.Text = PortName;
            name_editor.Toggle();
        }

        public override string GetModuleDisplayName() => reName(base.GetModuleDisplayName());
        public override string GetStagingDisableText() => reName(base.GetStagingDisableText());
        public override string GetStagingEnableText() => reName(base.GetStagingEnableText());
    }
}

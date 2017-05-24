//   ConfigNodeObjectGUI.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AT_Utils
{
    public class ConfigNodeObjectGUI : I_UI
    {
        ConfigNodeObject obj;
        string name;
        List<I_UI> fields = new List<I_UI>();
        List<ConfigNodeObjectGUI> children = new List<ConfigNodeObjectGUI>();
        ConfigNodeObjectGUI parent;

        ConfigNodeObjectGUI root { get { return parent == null ? this : parent.root; } }

        ConfigNodeObjectGUI selected_ui;
        ConfigNodeObjectGUI selected
        {
            get 
            {
                return parent == null? 
                    selected_ui : parent.selected;
            }
            set
            {
                root.unfolded = false;
                if(parent == null) 
                    selected_ui = value ?? this;
                else 
                    parent.selected = value;
                selected.unfolded = true;
            }
        }
        bool is_selected { get { return this == selected; } }

        bool is_unfolded;
        bool unfolded
        {
            get { return is_unfolded; }
            set
            {
                is_unfolded = value;
                if(is_unfolded)
                {
                    if(parent != null)
                        parent.unfolded = is_unfolded;
                }
                else children.ForEach(ch => ch.unfolded = is_unfolded);
            }
        }

        public static ConfigNodeObjectGUI FromObject(ConfigNodeObject o)
        {
            var gui = new ConfigNodeObjectGUI();
            gui.SetObject(o);
            return gui;
        }

        void SetObject(ConfigNodeObject o)
        {
            obj = o;
            name = Utils.ParseCamelCase(obj.GetType().Name);
            foreach(var fi in o.GetType()
                    .GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)
                    .Where(f => f.GetCustomAttributes(typeof(Persistent), true).Length > 0))
            {
                if(typeof(ConfigNodeObject).IsAssignableFrom(fi.FieldType))
                {
                    var child = fi.GetValue(obj) as ConfigNodeObject;
                    if(child != null) 
                    {
                        var gui = ConfigNodeObjectGUI.FromObject(child);
                        gui.parent = this;
                        gui.name = fi.Name;
                        children.Add(gui);
                    }
                }
                else 
                {
                    if(fi.FieldType == typeof(bool))
                        fields.Add(new BoolUI(fi, obj));
                    else if(fi.FieldType == typeof(string))
                        fields.Add(new StringUI(fi, obj));
                    else if(fi.FieldType == typeof(int))
                        fields.Add(new IntUI(fi, obj));
                    else if(fi.FieldType == typeof(float))
                        fields.Add(new FloatUI(fi, obj));
                    else if(fi.FieldType == typeof(Vector3))
                        fields.Add(new Vector3UI(fi, obj));
                }
            }
        }

        Vector2 tree_scroll;
        void DrawTree()
        {
            GUILayout.BeginVertical();
            var sel = is_selected;
            if(Utils.ButtonSwitch(name, sel) && !sel) selected = this;
            if(is_unfolded && children.Count > 0) 
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(25);
                GUILayout.BeginVertical();
                children.ForEach(ch => ch.DrawTree());
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        Vector2 fields_scroll;
        void DrawFields()
        {
            if(fields.Count == 0) return;
            fields_scroll = GUILayout.BeginScrollView(fields_scroll, Styles.white);
            GUILayout.BeginVertical();
            fields.ForEach(fi => fi.Draw());
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();
            tree_scroll = GUILayout.BeginScrollView(tree_scroll, Styles.white);
            DrawTree();
            GUILayout.EndScrollView();
            if(selected != null) selected.DrawFields();
            GUILayout.EndHorizontal();
        }
    }

    public abstract class FieldUI<T> : I_UI
    {
        readonly object host;
        readonly FieldInfo field;
        readonly string name;
        protected ITypeUI<T> ui;

        public T Value 
        {
            get { return (T)field.GetValue(host); }
            set { field.SetValue(host, value); }
        }

        protected FieldUI(FieldInfo field, object host)
        { 
            if(field.FieldType != typeof(T))
                throw new ArgumentException(string.Format("Field type should match generic parameter type, but {0} != {1}", 
                                                          field.FieldType.Name, typeof(T).Name));
            this.host = host;
            this.field = field;
            name = field.Name;
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.ExpandWidth(false));
            if(ui.Draw())
            {
                Value = ui.Value;
                ui.Value = Value;
            }
            GUILayout.EndHorizontal();
        }
    }

    public class StringUI : FieldUI<string>
    {
        public StringUI(FieldInfo field, object host) 
            : base(field, host) 
        {
            ui = new StringField();
            ui.Value = Value;
        }
    }

    public class BoolUI : FieldUI<bool>
    {
        public BoolUI(FieldInfo field, object host) 
            : base(field, host) 
        {
            ui = new BoolField();
            ui.Value = Value;
        }
    }

    public class IntUI : FieldUI<int>
    {
        public IntUI(FieldInfo field, object host) 
            : base(field, host) 
        {
            ui = new IntField();
            ui.Value = Value;
        }
    }

    public class FloatUI : FieldUI<float>
    {
        public FloatUI(FieldInfo field, object host) 
            : base(field, host) 
        {
            ui = new FloatField();
            ui.Value = Value;
        }
    }

    public class Vector3UI : FieldUI<Vector3>
    {
        public Vector3UI(FieldInfo field, object host) 
            : base(field, host) 
        {
            ui = new Vector3Field();
            ui.Value = Value;
        }
    }
}


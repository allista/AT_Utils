//   Attributes.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using System;
using System.Reflection;
using System.Collections.Generic;
using KSP.IO;
using System.Linq;

namespace AT_Utils
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ConfigOption : Attribute { }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class PersistState : Attribute { }

    public interface IInitializable
    {
        void InitState();
    }

    public interface ICachedState
    {
        void SyncState();
    }

    public static class PluginState
    {
        static Dictionary<string, PluginConfiguration> configs = new Dictionary<string, PluginConfiguration>();

        static PluginConfiguration get_config(Type object_type)
        {
            var config_path = AssemblyLoader.GetPathByType(object_type);
            if(configs.TryGetValue(config_path, out var cfg))
                return cfg;
            var create_for_type = typeof(PluginConfiguration).GetMethod("CreateForType");
            if(create_for_type == null)
                return null;
            create_for_type = create_for_type.MakeGenericMethod(object_type);
            cfg = create_for_type.Invoke(null, new object[] { null }) as PluginConfiguration;
            configs[config_path] = cfg;
            return cfg;
        }

        static readonly MethodInfo get_value = typeof(PluginConfiguration)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(mi => mi.Name == "GetValue" && mi.GetParameters().Length == 2)
            .SingleOrDefault();

        public static void LoadState(this object obj, string basename = "")
        {
            var T = obj.GetType();
            var cfg = get_config(T);
            if(string.IsNullOrEmpty(basename))
                basename = T.Name;
            //Utils.Log("Loading object: {}", basename);//debug
            basename += "-";
            cfg.load();
            foreach(var fi in T.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if(fi.GetCustomAttributes(typeof(ConfigOption), true).Length > 0)
                {
                    var get_val_gen = get_value.MakeGenericMethod(new[] { fi.FieldType });
                    var val = get_val_gen.Invoke(cfg, new[] { basename + fi.Name, fi.GetValue(obj) });
                    //Utils.Log("Load: {} = {}, was {}", basename + fi.Name, val, fi.GetValue(obj));//debug
                    fi.SetValue(obj, val);
                }
                else if(fi.FieldType.GetCustomAttributes(typeof(PersistState), true).Length > 0)
                {
                    var sub_obj = fi.GetValue(obj);
                    if(sub_obj != null)
                        LoadState(sub_obj, basename+fi.Name);
                }
            }
            if(obj is IInitializable initializable)
                initializable.InitState();
        }

        public static void SaveState(this object obj, string basename = "")
        {
            if(obj is ICachedState cached_state_obj)
                cached_state_obj.SyncState();
            var T = obj.GetType();
            var cfg = get_config(T);
            if(string.IsNullOrEmpty(basename))
                basename = T.Name;
            //Utils.Log("Saving object: {}", basename);//debug
            basename += "-";
            foreach(var fi in T.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if(fi.GetCustomAttributes(typeof(ConfigOption), true).Length > 0)
                {
                    var val = fi.GetValue(obj);
                    //Utils.Log("Save: {} = {}", basename + fi.Name, val);//debug
                    if(val != null)
                        cfg.SetValue(basename + fi.Name, val);
                }
                else if(fi.FieldType.GetCustomAttributes(typeof(PersistState), true).Length > 0)
                {
                    var sub_obj = fi.GetValue(obj);
                    if(sub_obj != null)
                        SaveState(sub_obj, basename + fi.Name);
                }
            }
            cfg.save();
        }
    }
}

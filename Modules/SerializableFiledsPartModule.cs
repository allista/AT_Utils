//   SerializableFiledsPartModule.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using KSP.IO;
using System.Text;

namespace AT_Utils
{
    public abstract class SerializableFiledsPartModule : PartModule, ISerializationCallbackReceiver
    {
        static readonly string cnode_name = typeof(IConfigNode).Name;

        List<FieldInfo> _serializable_fields;
        List<FieldInfo> serializable_fields
        {
            get
            {
                if(_serializable_fields == null)
                {
                    _serializable_fields = new List<FieldInfo>();
                    var fields = GetType().GetFields(BindingFlags.FlattenHierarchy|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance);
                    for(int i = 0, len = fields.Length; i < len; i++)
                    {
                        var fi = fields[i];
                        if(fi.GetCustomAttributes(typeof(SerializeField), true).Length == 0) continue;
                        if(typeof(ConfigNode).IsAssignableFrom(fi.FieldType) ||
                           fi.FieldType.GetInterface(cnode_name) != null ||
                           fi.FieldType.GetCustomAttributes(typeof(SerializableAttribute), true).Length > 0)
                            _serializable_fields.Add(fi);
                    }
//                    if(_serializable_fields.Count > 0) //debug
//                        Utils.Log("{}.serializable_fields: {}", GetType().Name, _serializable_fields);
                }
                return _serializable_fields;
            }
        }

        [SerializeField] byte[] _serialized_fields;
        [SerializeField] List<int> _offsets = new List<int>();
        [SerializeField] List<int> _fields = new List<int>();

        public virtual void OnBeforeSerialize()
        { 
            _serialized_fields = null;
            _offsets.Clear(); _fields.Clear();
            var count = serializable_fields.Count;
            if(count == 0) return;
            var offset = 0;
            var fields_data = new List<byte[]>(count);
            for(int i = 0; i < count; i++)
            {
                var fi = serializable_fields[i];
                var val = fi.GetValue(this);
//                Utils.Log("{}.{}.value = {}", this, fi.Name, val);//debug
                if(val != null)
                {
                    byte[] data;
                    var cn = val as ConfigNode;
                    if(cn != null)
                        data = Encoding.UTF8.GetBytes(cn.ToString());
                    else
                    {
                        var icn = val as IConfigNode;
                        if(icn != null)
                        {
                            var node = new ConfigNode(fi.Name);
                            icn.Save(node);
                            data = Encoding.UTF8.GetBytes(node.ToString());
                        }
                        else data = IOUtils.SerializeToBinary(val);
                    }
                    if(data != null && data.Length > 0)
                    {
                        fields_data.Add(data);
                        offset += data.Length;
                        _offsets.Add(offset);
                        _fields.Add(i);
                    }
                }
            }
            if(fields_data.Count == 0) return;
            if(fields_data.Count == 1) _serialized_fields = fields_data[0];
            else
            {
                var start = 0;
                _serialized_fields = new byte[offset];
                foreach(var data in fields_data)
                {
                    Array.Copy(data, 0, _serialized_fields, start, data.Length);
                    start += data.Length;
                }
            }
        }

        public virtual void OnAfterDeserialize() 
        { 
            if(_serialized_fields == null || _serialized_fields.Length == 0) return;
//            Utils.Log("_fields: {}\n_offsets: {}", _fields, _offsets);//debug
            var start = 0;
            for(int i = 0, count = _fields.Count; i < count; i++)
            {
                var offset = _offsets[i];
                var len = offset-start;
                var data = new byte[len];
                var fi = serializable_fields[_fields[i]];
                Array.Copy(_serialized_fields, start, data, 0, len);
                if(typeof(ConfigNode).IsAssignableFrom(fi.FieldType))
                    fi.SetValue(this, ConfigNode.Parse(Encoding.UTF8.GetString(data)).nodes[0]);
                else if(fi.FieldType.GetInterface(cnode_name) != null)
                {
                    var node = ConfigNode.Parse(Encoding.UTF8.GetString(data)).nodes[0];
//                    Utils.Log("{}.{}.node: {}", this, fi.Name, node);//debug
                    var f = fi.GetValue(this) as IConfigNode;
                    if(f == null) 
                    {
                        var constructor = fi.FieldType.GetConstructor(Type.EmptyTypes);
                        if(constructor != null)
                            f = constructor.Invoke(null) as IConfigNode;
                    }
                    if(f != null)
                    {
                        f.Load(node);
                        fi.SetValue(this, f);
                    }
                }
                else fi.SetValue(this, IOUtils.DeserializeFromBinary(data));
//                Utils.Log("{}.{}.value = {}", this, fi.Name, fi.GetValue(this));//debug
                start = offset;
            }
        }
    }
}


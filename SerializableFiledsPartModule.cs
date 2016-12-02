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

namespace AT_Utils
{
	public abstract class SerializableFiledsPartModule : PartModule, ISerializationCallbackReceiver
	{
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
						if(fi.GetCustomAttributes(typeof(SerializeField), true).Length > 0 &&
						   fi.FieldType.GetCustomAttributes(typeof(SerializableAttribute), true).Length > 0)
							_serializable_fields.Add(fi);
					}
//					Utils.Log("{}.fields: {}\n.serializable_fields: {}", GetType().Name, fields, serializable_fields);//debug
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
//				Utils.Log("{}.value = {}", fi.Name, val);//debug
				if(val != null)
				{
					var data = IOUtils.SerializeToBinary(val);
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
//			Utils.Log("_fields: {}\n_offsets: {}", _fields, _offsets);//debug
			if(_serialized_fields == null) return;
			var start = 0;
			for(int i = 0, count = _fields.Count; i < count; i++)
			{
				var offset = _offsets[i];
				var len = offset-start;
				var data = new byte[len];
				var fi = serializable_fields[_fields[i]];
				Array.Copy(_serialized_fields, start, data, 0, len);
				fi.SetValue(this, IOUtils.DeserializeFromBinary(data));
				start = offset;
			}
		}
	}
}


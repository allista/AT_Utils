//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

using System;
using System.Reflection;
using System.Collections.Generic;

namespace AT_Utils
{
	public class ConfigNodeObject : IConfigNode
	{
		public const string NODE_NAME = "NODE";

		static readonly string cnode_name = typeof(IConfigNode).Name;
		static readonly Type[] save_load_args = new [] {typeof(ConfigNode)};

		string node_name = null;
		public string NodeName 
		{ 
			get 
			{ 
				if(node_name == null)
				{
					var T = GetType();
					var name_field = T.GetField("NODE_NAME", BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy);
					node_name = name_field != null? name_field.GetValue(null) as string : T.Name;
//					Utils.Log("{}, field {}, name {}", T.Name, name_field, node_name);//debug
				}
				return node_name;
			}
		}

		protected bool not_persistant(FieldInfo fi)
		{ return fi.GetCustomAttributes(typeof(Persistent), true).Length == 0; }

		virtual public void Load(ConfigNode node)
		{ 
			ConfigNode.LoadObjectFromConfig(this, node);
			foreach(var fi in GetType().GetFields())
			{
				if(not_persistant(fi)) continue;
				var n = node.GetNode(fi.Name);
				if(n == null) continue;
				if(typeof(ConfigNode).IsAssignableFrom(fi.FieldType))
					fi.SetValue(this, n.CreateCopy());
				else if(fi.FieldType.GetInterface(cnode_name) != null)
				{
					var method = fi.FieldType.GetMethod("Load", save_load_args);
					if(method == null) continue;
					var f = fi.GetValue(this);
					if(f == null) 
					{
						var constructor = fi.FieldType.GetConstructor(Type.EmptyTypes);
						if(constructor == null) continue;
						f = constructor.Invoke(null);
						if(f == null) continue;
					}
					method.Invoke(f, new [] {n});
					fi.SetValue(this, f);
				}
			}
		}

		public void Load(ConfigNodeWrapper wrapper)
		{ Load(wrapper.ToConfigNode()); }

		virtual public void LoadFrom(ConfigNode parent)
		{
			var node = parent.GetNode(NodeName);
			if(node != null) Load(node);
		}

		ConfigNode get_field_node(FieldInfo fi, ConfigNode parent)
		{
			var n = parent.GetNode(fi.Name);
			if(n == null) n = parent.AddNode(fi.Name);
			else n.ClearData();
			return n;
		}

		virtual public void Save(ConfigNode node)
		{ 
			ConfigNode.CreateConfigFromObject(this, node); 
			foreach(var fi in GetType().GetFields())
			{
				if(not_persistant(fi)) continue;
				if(typeof(ConfigNode).IsAssignableFrom(fi.FieldType))
				{
					var f = fi.GetValue(this) as ConfigNode;
					if(f != null) get_field_node(fi, node).AddData(f);
				}
				else if(fi.FieldType.GetInterface(cnode_name) != null)
				{
					var method = fi.FieldType.GetMethod("Save", save_load_args);
					if(method == null) continue;
					var n = get_field_node(fi, node);
					var f = fi.GetValue(this);
					if(f == null) continue;
					method.Invoke(f, new [] {n});
				}
			}
		}

		virtual public void SaveInto(ConfigNode parent)
		{ Save(parent.AddNode(NodeName)); }

		virtual public void Copy(ConfigNodeObject other)
		{
			var node = new ConfigNode();
			other.Save(node);
			Load(node);
		}

		virtual public CNO Clone<CNO>()
			where CNO : ConfigNodeObject, new()
		{
			var node = new ConfigNode(NODE_NAME);
			Save(node);
			return ConfigNodeObject.FromConfig<CNO>(node);
		}

		public static CNO FromConfig<CNO>(ConfigNode node)
			where CNO : ConfigNodeObject, new()
		{
			var cno = new CNO();
			cno.Load(node);
			return cno;
		}

		public override string ToString()
		{
			var n = new ConfigNode(GetType().Name);
			Save(n);
			return n.ToString();
		}
	}

	public class TypedConfigNodeObject : ConfigNodeObject
	{
		public override void Save(ConfigNode node)
		{
			var type = GetType();
			node.AddValue("type", string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name));
			base.Save(node);
		}

		public override void Load(ConfigNode node)
		{
			base.Load(node);
			foreach(var fi in GetType().GetFields())
			{
				if(not_persistant(fi)) continue;
				if(fi.FieldType.IsSubclassOf(typeof(TypedConfigNodeObject)))
				{
					var n = node.GetNode(fi.Name);
					fi.SetValue(this, n == null? null : FromConfig(n));
				}
			}
		}

		public static TypedConfigNodeObject FromConfig(ConfigNode node)
		{
			TypedConfigNodeObject obj = null;
			var typename = node.GetValue("type");
			if(typename == null) return obj;
			var type = Type.GetType(typename);
			if(type == null)
			{
				Utils.Log("Unable to create {}: Type not found.", typename);
				return obj;
			}
			try 
			{ 
				obj = Activator.CreateInstance(type) as TypedConfigNodeObject;
				obj.Load(node);
			}
			catch(Exception ex) { Utils.Log("Unable to create {}: {}\n{}", typename, ex.Message, ex.StackTrace); }
			return obj;
		}
	}

	public class PersistentList<T> : List<T>, IConfigNode where T : IConfigNode, new()
	{
		public PersistentList() {}
		public PersistentList(IEnumerable<T> content) : base(content) {}

		public void Save(ConfigNode node)
		{
			for(int i = 0, count = Count; i < count; i++) 
				this[i].Save(node.AddNode("Item"));
		}

		public void Load(ConfigNode node)
		{
			Clear();
			var nodes = node.GetNodes();
			for(int i = 0, len = nodes.Length; i < len; i++)
			{
				var item = new T();
				item.Load(nodes[i]);
				Add(item);
			}
		}
	}

	public class PersistentQueue<T> : Queue<T>, IConfigNode where T : IConfigNode, new()
	{
		public PersistentQueue() {}
		public PersistentQueue(IEnumerable<T> content) : base(content) {}

		public void Save(ConfigNode node)
		{
			foreach(var item in this)
				item.Save(node.AddNode("Item"));
		}

		public void Load(ConfigNode node)
		{
			Clear();
			var nodes = node.GetNodes();
			for(int i = 0, len = nodes.Length; i < len; i++)
			{
				var item = new T();
				item.Load(nodes[i]);
				Enqueue(item);
			}
		}
	}

	public class PersistentBaseList<T> : ConfigNodeObject where T : TypedConfigNodeObject, new()
	{
		readonly public List<T> List = new List<T>();

		public int Count { get { return List.Count; } }
		public T this[int i]
		{
			get { return List[i]; }
			set { List[i] = value; }
		}
		public void Add(T it) { List.Add(it); }
		public bool Remove(T it) { return List.Remove(it); }
		public void Clear() { List.Clear(); }
		public int IndexOf(T it) { return List.IndexOf(it); }

		public override void Save(ConfigNode node)
		{
			base.Save(node);
			for(int i = 0, count = List.Count; i < count; i++) 
				List[i].Save(node.AddNode(i.ToString()));
		}

		public override void Load(ConfigNode node)
		{
			base.Load(node);
			foreach(var n in node.GetNodes())
			{
				var it = TypedConfigNodeObject.FromConfig(n) as T;
				if(it != null) List.Add(it);
			}
		}
	}
}


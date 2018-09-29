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
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Text;

namespace AT_Utils
{
    public class ConfigNodeObject : IConfigNode
    {
        public const string NODE_NAME = "NODE";

        static readonly string cnode_name = typeof(IConfigNode).Name;

        string node_name = null;
        public string NodeName
        {
            get
            {
                if(node_name == null)
                {
                    var T = GetType();
                    var name_field = T.GetField("NODE_NAME", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    node_name = name_field != null ? name_field.GetValue(null) as string : T.Name;
                }
                return node_name;
            }
        }

        protected bool not_persistant(FieldInfo fi) =>
        fi.GetCustomAttributes(typeof(Persistent), true).Length == 0;

        T get_or_create<T>(FieldInfo fi) where T : class =>
        (fi.GetValue(this) ?? Activator.CreateInstance(fi.FieldType)) as T;

        FieldInfo[] get_fields() =>
        GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        virtual public void Load(ConfigNode node)
        {
            try
            {
                ConfigNode.LoadObjectFromConfig(this, node);
            }
            catch(Exception e)
            {
                Utils.Log("Exception while loading {}\n{}\n{}\n{}", 
                          this, e.Message, e.StackTrace, node);
            }
            foreach(var fi in get_fields())
            {
                if(not_persistant(fi)) continue;
                var n = node.GetNode(fi.Name);
                //restore types saved as nodes
                if(n != null)
                {
                    //restore IConfigNodes
                    if(fi.FieldType.GetInterface(cnode_name) != null)
                    {
                        var f = get_or_create<IConfigNode>(fi);
                        if(f != null)
                        {
                            f.Load(n);
                            fi.SetValue(this, f);
                        }
                    }
                    //restore ConfigNodes
                    else if(typeof(ConfigNode).IsAssignableFrom(fi.FieldType))
                        fi.SetValue(this, n.CreateCopy());
                    //restore Orbit
                    else if(fi.FieldType == typeof(Orbit))
                    {
                        var obt = new OrbitSnapshot(n);
                        fi.SetValue(this, obt.Load());
                    }
                    continue;
                }
                //restore types saved as values
                var v = node.GetValue(fi.Name);
                if(v != null)
                {
                    if(fi.FieldType == typeof(Guid))
                        fi.SetValue(this, new Guid(v));
                    else if(fi.FieldType == typeof(Vector3d))
                        fi.SetValue(this, KSPUtil.ParseVector3d(v));
                }
            }
        }

        virtual public void LoadFrom(ConfigNode parent, string node_name = null)
        {
            var node = parent.GetNode(node_name ?? NodeName);
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
            try
            {
                ConfigNode.CreateConfigFromObject(this, node);
            }
            catch(Exception e)
            {
                Utils.Log("Exception while saving {}\n{}\n{}\n{}", 
                          this, e.Message, e.StackTrace, node);
            }
            foreach(var fi in get_fields())
            {
                if(not_persistant(fi)) continue;
                //save all IConfigNode
                if(fi.FieldType.GetInterface(cnode_name) != null)
                {
                    var f = fi.GetValue(this) as IConfigNode;
                    if(f != null) f.Save(get_field_node(fi, node));
                }
                //save ConfigNode
                else if(typeof(ConfigNode).IsAssignableFrom(fi.FieldType))
                {
                    var f = fi.GetValue(this) as ConfigNode;
                    if(f != null) get_field_node(fi, node).AddData(f);
                }
                //save some often used types
                else if(fi.FieldType == typeof(Guid))
                    node.AddValue(fi.Name, ((Guid)fi.GetValue(this)).ToString("N"));
                else if(fi.FieldType == typeof(Vector3d))
                    node.AddValue(fi.Name, KSPUtil.WriteVector((Vector3d)fi.GetValue(this)));
                else if(fi.FieldType == typeof(Orbit))
                {
                    var f = fi.GetValue(this) as Orbit;
                    if(f != null)
                    {
                        var obt = new OrbitSnapshot(f);
                        obt.Save(get_field_node(fi, node));
                    }
                }
            }
        }

        virtual public void SaveInto(ConfigNode parent, string node_name = null)
        { Save(parent.AddNode(node_name ?? NodeName)); }

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
            return FromConfig<CNO>(node);
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

        #region Binary Serializer
        static MemoryStream memoryStream = new MemoryStream();
        static BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static string Serialize(object obj)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            binaryFormatter.Serialize(memoryStream, obj);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static T Deserialize<T>(string data)
        {
            var bytes = Convert.FromBase64String(data);
            memoryStream.SetLength(0);
            memoryStream.Write(bytes, 0, bytes.Length);
            return (T)binaryFormatter.Deserialize(memoryStream);
        }
        #endregion
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
                    fi.SetValue(this, n == null ? null : FromConfig(n));
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
        public PersistentList() { }
        public PersistentList(IEnumerable<T> content) : base(content) { }

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

    public class PersistentDictS<T> : Dictionary<string, T>, IConfigNode where T : IConfigNode, new()
    {
        public PersistentDictS() { }
        public PersistentDictS(IDictionary<string, T> data) : base(data) { }

        public void Save(ConfigNode node)
        {
            foreach(var item in this)
                item.Value.Save(node.AddNode(item.Key));
        }

        public void Load(ConfigNode node)
        {
            Clear();
            var nodes = node.GetNodes();
            for(int i = 0, len = nodes.Length; i < len; i++)
            {
                var n = nodes[i];
                var item = new T();
                item.Load(n);
                Add(n.name, item);
            }
        }
    }

    public class PersistentQueue<T> : Queue<T>, IConfigNode where T : IConfigNode, new()
    {
        public PersistentQueue() { }
        public PersistentQueue(IEnumerable<T> content) : base(content) { }

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


//   TechFloat.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.Collections.Generic;

namespace AT_Utils
{
	public abstract class TechValue<T> : ConfigNodeObject
	{
		public readonly Dictionary<string, T> Values = new Dictionary<string, T>();
		public Func<T, T, bool> Compare;

		#region ConfigNode
		protected abstract T parse(string val);

		public override void Load(ConfigNode node)
		{
			if(node == null) return;
			base.Load(node);
			foreach(ConfigNode.Value tech in node.values)
				Values[tech.name] = parse(tech.value);
		}

		public override void Save(ConfigNode node) 
		{  
			base.Save(node);
			foreach(var v in Values) 
				node.AddValue(v.Key, v.Value); 
		}
		#endregion

		#region TechTree
		//current_value is needed to preserve scale of an existing vessel when configuration is changed
		public bool TryGetValue(out T value, bool ignore_tech_tree = false, Func<T, T, bool> compare = null)
		{
			if(compare == null) compare = Compare;
			value = default(T); var first = true;
			foreach(var pair in Values)
			{
				if((ignore_tech_tree || Utils.PartIsPurchased(pair.Key)) &&
				   (first || compare(pair.Value, value)))
				{ value = pair.Value; first = false; }
			}
			return !first;
		}
		#endregion
	}

	public class TechFloat : TechValue<float>
	{
		protected override float parse(string val)
		{
			try { return float.Parse(val); }
			catch { return 0f; }
		}

		public TechFloat(Func<float, float, bool> compare) 
		{ Compare = compare; }
	}
}


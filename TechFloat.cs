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
	public abstract class TechValue<T> : IConfigNode
	{
		public readonly Dictionary<string, T> Values = new Dictionary<string, T>();
		public Func<T, T, bool> Compare;

		#region ConfigNode
		protected abstract T parse(string val);

		public void Load(ConfigNode node)
		{
			if(node == null) return;
			foreach(ConfigNode.Value tech in node.values)
				Values[tech.name] = parse(tech.value);
		}

		public void Save(ConfigNode node) {}
		#endregion

		#region TechTree
		//ResearchAndDevelopment.PartModelPurchased is broken and always returns 'true'
		public static bool PartIsPurchased(string name)
		{
			var info = PartLoader.getPartInfoByName(name);
			if(info == null) return false;
			if(HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return true;
			var tech = ResearchAndDevelopment.Instance.GetTechState(info.TechRequired);
			return tech != null && tech.state == RDTech.State.Available && tech.partsPurchased.Contains(info);
		}

		//current_value is needed to preserve scale of an existing vessel when configuration is changed
		public bool TryGetValue(out T value, bool ignore_tech_tree = false, Func<T, T, bool> compare = null)
		{
			if(compare == null) compare = Compare;
			value = default(T); var first = true;
			foreach(var pair in Values)
			{
				if((ignore_tech_tree || PartIsPurchased(pair.Key)) &&
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


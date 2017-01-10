//   ResourceTransferWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

//This code is based on code from ExLaunchPads mod, BuildWindow class.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
	public class ResourceTransferWindow : GUIWindowBase
	{
		List<ResourceManifest> transfer_list;
		Vector2 scroll = Vector2.zero;
		bool link_lfo_sliders = true;
		const int res_line_height = 40;

		public bool transferNow { get; private set; } = false;
		public bool Closed { get { return transferNow || closed; } }
		bool closed;

		public ResourceTransferWindow()
		{
			width = 600;
			height = 100;
		}
		
		static float ResourceLine(string label, float fraction, ResourceManifest res)
		{
			GUILayout.BeginHorizontal ();

			// Resource name
			GUILayout.Box(label, Styles.white, GUILayout.Width(120), GUILayout.Height(res_line_height));
			var rhs = fraction*res.maxAmount;
			var lhs = res.pool-rhs;
			// lhs amount/capacity
			GUILayout.Box(Utils.formatBigValue((float)res.host_capacity, "u"),
			              Styles.yellow, GUILayout.Width(60), GUILayout.Height(res_line_height));
			GUILayout.Box(Utils.formatBigValue((float)lhs, "u"),
			              Styles.fracStyle((float)(lhs/res.host_capacity)), 
			              GUILayout.Width(60), GUILayout.Height(res_line_height));
			// Fill amount
			GUILayout.BeginVertical();
			GUILayout.Box(fraction.ToString("P1"), Styles.slider_text, 
			              GUILayout.Width(200), GUILayout.Height(20));
			var frac = GUILayout.HorizontalSlider(fraction, 0.0F, 1.0F,
												  Styles.slider,
												  GUI.skin.horizontalSliderThumb,
			                                      GUILayout.Width(200),
												  GUILayout.Height(20));
			if(fraction*res.maxAmount < res.minAmount) 
				frac = (float)(res.minAmount/res.maxAmount);
			GUILayout.EndVertical();
			// rhs amount and capacity
			GUILayout.Box(Utils.formatBigValue((float)(rhs), "u"),
			              Styles.fracStyle((float)(rhs/res.capacity)), 
			              GUILayout.Width(60), GUILayout.Height(res_line_height));
			GUILayout.Box(Utils.formatBigValue((float)res.capacity, "u"),
			              Styles.yellow, GUILayout.Width(60), GUILayout.Height(res_line_height));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			return frac;
		}
		
		void TransferWindow(int windowId)
		{
			
			GUILayout.BeginVertical();
			link_lfo_sliders = GUILayout.Toggle(link_lfo_sliders, "Link LiquidFuel and Oxidizer sliders");
			var nres = transfer_list.Count;
			scroll = GUILayout.BeginScrollView(scroll, 
			                                   GUILayout.Width(width),
			                                   GUILayout.Height(res_line_height*Math.Min(nres, 4)+20));
			for(int i = 0; i < nres; i++)
			{
				var r = transfer_list[i];
				float frac = r.maxAmount > 0 ? (float)(r.amount / r.maxAmount) : 0f;
				frac = ResourceLine(r.name, frac, r);
				if(link_lfo_sliders && (r.name == "LiquidFuel" || r.name == "Oxidizer"))
				{
					string other = r.name == "LiquidFuel" ? "Oxidizer" : "LiquidFuel";
					var or = transfer_list.Find(res => res.name == other);
					if(or != null)
						or.amount = or.maxAmount * frac;
				}
				r.amount = frac * r.maxAmount;
			} 
			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			closed = GUILayout.Button("Close", Styles.close_button, GUILayout.ExpandWidth(true));
			transferNow = GUILayout.Button("Transfer now", Styles.active_button, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			TooltipsAndDragWindow();
		}
		
		public void Draw(string title, List<ResourceManifest> resourceTransferList)
		{
			LockControls();
			transfer_list = resourceTransferList;
			WindowPos = GUILayout.Window(GetInstanceID(), 
			                             WindowPos, TransferWindow,
			                             title, 
			                             GUILayout.Width(width),
			                             GUILayout.Height(height)).clampToScreen();
			if(Closed) UnlockControls();
		}
	}
}

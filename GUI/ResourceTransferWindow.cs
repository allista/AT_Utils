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
			width = 360;
			height = 100;
		}
		
		static float ResourceLine(string label, float fraction, 
		               		      double pool, 
		               		      double minAmount, double maxAmount, 
		               		      double capacity)
		{
			GUILayout.BeginHorizontal ();

			// Resource name
			GUILayout.Box(label, Styles.white, GUILayout.Width(120), GUILayout.Height(res_line_height));
			// Fill amount
			// limit slider to 0.5% increments
			GUILayout.BeginVertical();
			fraction = GUILayout.HorizontalSlider(fraction, 0.0F, 1.0F,
												  Styles.slider,
												  GUI.skin.horizontalSliderThumb,
												  GUILayout.Width(300),
												  GUILayout.Height(20));
			
			fraction = (float)Math.Round(fraction, 3);
			fraction = (Mathf.Floor(fraction * 200)) / 200;
			if(fraction*maxAmount < minAmount) fraction =(float)(minAmount/maxAmount);
			GUILayout.Box((fraction * 100) + "%",
						   Styles.slider_text, GUILayout.Width(300),
						   GUILayout.Height(20));
			GUILayout.EndVertical();
			// amount and capacity
			GUILayout.Box((Math.Round(pool-fraction*maxAmount, 2)).ToString(),
						   Styles.white, GUILayout.Width(75),
						   GUILayout.Height(40));
			GUILayout.Box((Math.Round(fraction*maxAmount, 2)).ToString(),
						   Styles.fracStyle(fraction), GUILayout.Width(75),
						   GUILayout.Height(40));
			GUILayout.Box((Math.Round(capacity, 2)).ToString(),
						   Styles.yellow, GUILayout.Width(75),
						   GUILayout.Height(40));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			return fraction;
		}
		
		void TransferWindow(int windowId)
		{
			
			GUILayout.BeginVertical();
			link_lfo_sliders = GUILayout.Toggle(link_lfo_sliders, "Link LiquidFuel and Oxidizer sliders");
			var nres = transfer_list.Count;
			scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(res_line_height*Math.Max(nres, 4)));
			for(int i = 0; i < nres; i++)
			{
				var r = transfer_list[i];
				float frac = r.maxAmount > 0 ? (float)(r.amount / r.maxAmount) : 0f;
				frac = ResourceLine(r.name, frac, r.pool, r.minAmount, r.maxAmount, r.capacity);
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
			TooltipsAndDragWindow(WindowPos);
		}
		
		public void Draw(string title, List<ResourceManifest> resourceTransferList)
		{
			if(resourceTransferList.Count == 0) return;
			transfer_list = resourceTransferList;
			LockControls();
			WindowPos = GUILayout.Window(GetInstanceID(), 
			                             WindowPos, TransferWindow,
			                             title, 
			                             GUILayout.Width(width),
			                             GUILayout.Height(height)).clampToScreen();
		}
	}
}


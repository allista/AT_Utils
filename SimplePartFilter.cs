//   SimplePartCategorizer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using System.Linq;
using UnityEngine;
using RUI.Icons.Selectable;
using KSP.UI;
using KSP.UI.Screens;
using System.Collections.Generic;

namespace AT_Utils
{
    /// <summary>
    /// This is the base class for easy creaton of custom part filters.
    /// Its main purpouse is to allow creation of dynamic filters responsive 
    /// to in-game part modifications.
    /// 
    /// The code is adapted from the RealChute mod by Christophe Savard (stupid_chris):
    /// https://github.com/StupidChris/RealChute/blob/master/RealChute/RCFilterManager.cs
    /// Many thanks to Chris for figuring this out so fast!
    /// </summary>
    public abstract class SimplePartFilter : MonoBehaviour
    {
        protected List<Type> MODULES;
        protected string CATEGORY = "Filter by function";
        protected string SUBCATEGORY = "";
        protected string FOLDER = "";
        protected string ICON = "";

        void Awake()
        { 
            GameEvents.onGUIEditorToolbarReady.Add(add_filter);
        }

        protected abstract bool filter(AvailablePart part);

        static void set_modules_icon(List<Type> modules, Icon icon)
        {
            if(modules != null && modules.Count > 0)
            {
                PartCategorizer.Instance.filters
                    .Find(f => f.button.categoryName == "Filter by module")
                    .subcategories.FindAll(s => 
                {
                    var cat_name = string.Join("", s.button.categoryName.Split());
                    return modules.Any(m => m.Name == cat_name);
                })
                    .ForEach(c => c.button.SetIcon(icon));
            }
        }

        static Icon load_icon(string icon_name, string folder)
        {
            if(PartCategorizer.Instance.iconLoader.iconDictionary.ContainsKey(icon_name))
                return PartCategorizer.Instance.iconLoader.GetIcon(icon_name);
            var icon_path = folder+"/"+icon_name;
            var icon   = GameDatabase.Instance.GetTexture(icon_path, false);
            var icon_s = GameDatabase.Instance.GetTexture(icon_path+"_selected", false) ?? icon;
            var selectable_icon = new Icon(icon_name, icon, icon_s, icon == icon_s);
            PartCategorizer.Instance.iconLoader.icons.Add(selectable_icon);
            PartCategorizer.Instance.iconLoader.iconDictionary.Add(icon_name, selectable_icon);
            return selectable_icon;
        }

        protected virtual void add_filter()
        {
            if(string.IsNullOrEmpty(ICON) ||
               string.IsNullOrEmpty(CATEGORY) ||
               string.IsNullOrEmpty(SUBCATEGORY))
                return;
            //load the icon
            var icon = load_icon(ICON, FOLDER);
            //get category
            var category = PartCategorizer.Instance.filters
                .Find(f => f.button.categoryName == CATEGORY);
//            Utils.Log("add_filter.category: {}\nall: {}", category, 
//                      PartCategorizer.Instance.filters.ConvertAll(f => f.button.categoryName));//debug
            //add custom function filter
            PartCategorizer.AddCustomSubcategoryFilter(category, SUBCATEGORY, SUBCATEGORY, icon, filter);//FIXME: NRE here!!!
            //Apparently needed to make sure the icon actually shows at first
            var button = category.button.activeButton;
            button.Value = false;
            button.Value = true;
            //set icon(s) for all the modules
            set_modules_icon(MODULES, icon);
        }
    }
}

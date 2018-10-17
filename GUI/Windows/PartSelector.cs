//   PartSelector.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public class PartSelector : GUIWindowBase
    {
        const int PAGE_SIZE = 7;
        enum LoadState { NONE, LOADING, LOADED };

        class PartComparer : IComparer<AvailablePart>
        {
            public int Compare(AvailablePart x, AvailablePart y) =>
            x.title.CompareTo(y.title);
        }

        int pages, page;
        LoadState load_state;
        float load_progress;
        string filter = "";
        RealTimer filter_timer = new RealTimer(0.5);
        SortedList<AvailablePart, GUIContent> _parts = new SortedList<AvailablePart, GUIContent>(new PartComparer());
        SortedList<AvailablePart, GUIContent> parts = new SortedList<AvailablePart, GUIContent>(new PartComparer());
        Action<AvailablePart> load_part = delegate { };

        public PartSelector()
        {
            width = 400;
            height = 410;
            WindowPos = new Rect(Screen.width / 2 - width / 2, 100, width, 100);
        }

        public override void Awake()
        {
            base.Awake();
            Show(false);
        }

        void update_pages()
        {
            pages = parts.Count/PAGE_SIZE;
            if(parts.Count > PAGE_SIZE && parts.Count % PAGE_SIZE > 0)
                pages += 1;
        }

        void clear_filtered()
        {
            parts.Clear();
            page = pages = 0;
            load_progress = 0f;
        }

        void loading()
        {
            load_state = LoadState.LOADING;
            filter_timer.Reset();
        }

        IEnumerator<YieldInstruction> filter_parts()
        {
            loading();
            yield return null;
            clear_filtered();
            var i = 1;
            var parts_count = _parts.Count;
            foreach(var item in _parts)
            {
                if(string.IsNullOrEmpty(filter)
                   || item.Key.title.ToLowerInvariant().Contains(filter))
                {
                    parts.Add(item.Key, item.Value);
                    update_pages();
                }
                load_progress = (float)(i++)/parts_count;
                if(i % PAGE_SIZE == 0)
                    yield return null;
            }
            load_state = LoadState.LOADED;
        }

        IEnumerator<YieldInstruction> build_parts_list()
        {
            loading();
            _parts.Clear();
            clear_filtered();
            var part_list = PartLoader.LoadedPartsList;
            if(part_list == null) yield break;
            for(int i = 0, part_listCount = part_list.Count; i < part_listCount; i++)
            {
                var info = part_list[i];
                if(!string.IsNullOrEmpty(info.title)
                   && info.category != PartCategories.none
                   && !_parts.ContainsKey(info)
                   && Utils.PartIsPurchased(info))
                {
                    var dims = Vector3.zero;
                    var mass = float.NaN;
                    if(info.partPrefab)
                    {
                        mass = info.partPrefab.mass;
                        dims = info.partPrefab.Bounds(null).size;
                    }
                    var label = string.Format("<color=yellow><b>{0}</b></color>\n" +
                                              "<color=silver>mass:</color> {1:F1}t " +
                                              "<color=silver>cost:</color> {2:F0} " +
                                              "<color=silver>size:</color> {3}",
                                              info.title, mass, info.cost,
                                              Utils.formatDimensions(dims));
                    var button = new GUIContent(label, info.description);
                    _parts.Add(info, button);
                    if(string.IsNullOrEmpty(filter) 
                       || info.title.ToLowerInvariant().Contains(filter))
                    {
                        parts.Add(info, button);
                        update_pages();
                    }
                }
                load_progress = (float)(i+1)/part_listCount;
                if(i % PAGE_SIZE == 0)
                    yield return null;
            }
            load_state = LoadState.LOADED;
        }

        public override void Show(bool show)
        {
            base.Show(show);
            if(show) 
            {
                if(load_state == LoadState.NONE)
                    StartCoroutine(build_parts_list());
            }
            else load_state = LoadState.NONE;
        }

        void select_page()
        {
            page += Utils.LeftRightChooser(string.Format("Page {0}/{1}", (page+1), pages));
            if(page*PAGE_SIZE >= parts.Count)
                page -= 1;
            if(page < 0)
                page = 0;
        }

        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search: ", GUILayout.ExpandWidth(false));
            var flt = "";
            if(load_state != LoadState.LOADING)
                flt = GUILayout.TextField(filter, GUILayout.ExpandWidth(true));
            else
                GUILayout.Label(filter, GUI.skin.textField, GUILayout.ExpandWidth(true));
            if(!string.IsNullOrEmpty(filter) && 
               GUILayout.Button("X", Styles.danger_button, GUILayout.ExpandWidth(false)))
                flt = "";
            if(load_state == LoadState.LOADING)
                GUILayout.Label(string.Format("<b>Loading:{0,6:P0}</b>", load_progress), 
                                Styles.rich_label, GUILayout.ExpandWidth(false));
            else
                GUILayout.Space(0);
            GUILayout.EndHorizontal();
            var new_filter = flt != filter;
            if(load_state != LoadState.LOADING 
               && (new_filter || filter_timer.Started))
            {
                if(new_filter)
                {
                    filter = flt.ToLowerInvariant();
                    filter_timer.Restart();
                }
                else if(filter_timer.TimePassed)
                    StartCoroutine(filter_parts());
            }
            if(parts.Count > 0)
            {
                var keys = parts.Keys;
                AvailablePart toLoad = null;
                for(int i = page*PAGE_SIZE, stop = Math.Min(i+PAGE_SIZE, parts.Count);
                    i < stop; 
                    i++)
                {
                    var part = keys[i];
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button(parts[part], Styles.boxed_label, GUILayout.ExpandWidth(true)))
                        toLoad = part;
                    GUILayout.EndHorizontal();
                }
                if(toLoad != null && load_part != null)
                {
                    load_part(toLoad);
                    Show(false);
                }
                GUILayout.FlexibleSpace();
                if(parts.Count > PAGE_SIZE)
                    select_page();
                if(GUILayout.Button("Close", Styles.close_button, GUILayout.ExpandWidth(true)))
                    Show(false);
            }
            GUILayout.EndVertical();
            TooltipsAndDragWindow();
        }

        public void Draw(Action<AvailablePart> loadPart)
        {
            if(doShow)
            {
                LockControls();
                load_part = loadPart;
                WindowPos = GUILayout.Window(GetInstanceID(),
                                             WindowPos,
                                             DrawWindow,
                                             "Select Part",
                                             GUILayout.Width(width),
                                             GUILayout.Height(height))
                                     .clampToScreen();
            }
            else UnlockControls();
        }
    }
}

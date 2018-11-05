using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class ColorList : DragablePanel
    {
        [SerializeField]
        ColorPicker picker;

        [SerializeField]
        GameObject listContainer;

        [SerializeField]
        GameObject colorItemPrefab;

        [SerializeField]
        Text title;

        List<ColorItem> items = new List<ColorItem>();
        ToggleGroup colorItemsGroup;

        protected override void Awake()
        {
            base.Awake();
            colorItemsGroup = listContainer.GetComponent<ToggleGroup>();
        }

        public void SetTitle(string text)
        {
            title.text = text;
        }

        public void AddColored(IColored colored, string name)
        {
            if(colored == null) return;
            var itemObj = Instantiate(colorItemPrefab);
            var item = itemObj.GetComponent<ColorItem>();
            item.picker = picker;
            item.SetColored(colored, name);
            item.pickerToggle.group = colorItemsGroup;
            colorItemsGroup.RegisterToggle(item.pickerToggle);
            itemObj.transform.SetParent(listContainer.transform);
            items.Add(item);
        }

        void OnDestroy()
        {
            items.ForEach(it => Destroy(it.gameObject));
            items.Clear();
        }
    }
}
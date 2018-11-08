using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
        Button cancelButton;

        [SerializeField]
        Button applyButton;

        [SerializeField]
        Text title;

        List<ColorItem> items = new List<ColorItem>();
        ToggleGroup colorItemsGroup;

        protected virtual void Awake()
        {
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

        public void AddOnCancel(UnityAction action)
        {
            cancelButton.onClick.AddListener(action);
        }

        public void AddOnApply(UnityAction action)
        {
            applyButton.onClick.AddListener(action);
        }

        void OnDestroy()
        {
            items.ForEach(it => Destroy(it.gameObject));
            items.Clear();
        }
    }
}
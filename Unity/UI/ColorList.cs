using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class ColorList : ScreenBoundRect
    {
        [SerializeField] private ColorPicker picker;

        [SerializeField] private GameObject listContainer;

        [SerializeField] private GameObject colorItemPrefab;

        public Button closeButton;
        public Button resetButton;
        public Button restoreButton;
        public Button saveButton;

        [SerializeField] private Text title;

        private readonly List<ColorItem> items = new List<ColorItem>();
        private ToggleGroup colorItemsGroup;

        protected override void Awake()
        {
            base.Awake();
            colorItemsGroup = listContainer.GetComponent<ToggleGroup>();
            foreach(var color in Colors.All)
                AddColored(color.Value, color.Key);
        }

        public void SetTitle(string text)
        {
            title.text = text;
        }

        public void AddColored(IColored colored, string colorName)
        {
            if(colored == null) return;
            var itemObj = Instantiate(colorItemPrefab, listContainer.transform, true);
            var item = itemObj.GetComponent<ColorItem>();
            item.picker = picker;
            item.SetColored(colored, colorName);
            item.pickerToggle.group = colorItemsGroup;
            colorItemsGroup.RegisterToggle(item.pickerToggle);
            items.Add(item);
        }

        private void OnDestroy()
        {
            items.ForEach(it => Destroy(it.gameObject));
            items.Clear();
        }
    }
}

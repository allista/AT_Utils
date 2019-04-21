using UnityEngine;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class ColorItem : MonoBehaviour
    {
        [SerializeField]
        Image colorDisplay;

        [SerializeField]
        InputField colorHTML;

        [SerializeField]
        Text pickerToggleText;

        public ColorPicker picker;
        public Toggle pickerToggle;

        IColored colored;

        void Awake()
        {
            colorHTML.readOnly = true;
            pickerToggle.onValueChanged.AddListener(togglePicker);
        }

        void displayColor(Color color)
        {
            colorDisplay.color = color;
            colorHTML.text = "#" + ColorUtility.ToHtmlStringRGBA(color);
            if(picker != null && pickerToggle.isOn)
                picker.CurrentColor = color;
        }

        public void SetColored(IColored colored, string name)
        {
            this.colored = colored;
            pickerToggleText.text = name;
            displayColor(colored.color);
            colored.addOnColorChangeListner(displayColor);
        }

        public void ChangeColor(Color color)
        {
            if(colored != null)
                colored.color = color;
            else
                displayColor(color);
        }

        void togglePicker(bool toggle)
        {
            if(picker == null) return;
            if(toggle)
            {
                picker.CurrentColor = colorDisplay.color;
                picker.onValueChanged.AddListener(ChangeColor);
            }
            else
                picker.onValueChanged.RemoveListener(ChangeColor);
            if(pickerToggle.group != null)
                picker.gameObject.SetActive(pickerToggle.group.AnyTogglesOn());
            else
                picker.gameObject.SetActive(toggle);
        }
    }
}
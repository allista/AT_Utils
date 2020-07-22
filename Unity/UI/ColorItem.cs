using UnityEngine;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class ColorItem : MonoBehaviour
    {
        [SerializeField] private Image colorDisplay;

        [SerializeField] private InputField colorHTML;

        [SerializeField] private Text pickerToggleText;

        public ColorPicker picker;
        public Toggle pickerToggle;

        private IColored colored;

        private void Awake()
        {
            colorHTML.readOnly = true;
            pickerToggle.onValueChanged.AddListener(togglePicker);
        }

        private void displayColor(Color color)
        {
            colorDisplay.color = color;
            colorHTML.text = $"#{ColorUtility.ToHtmlStringRGBA(color)}";
            if(picker != null && pickerToggle.isOn)
                picker.CurrentColor = color;
        }

        public void SetColored(IColored newColored, string colorName)
        {
            this.colored = newColored;
            pickerToggleText.text = colorName;
            displayColor(newColored.color);
            newColored.addOnColorChangeListner(displayColor);
        }

        public void ChangeColor(Color color)
        {
            if(colored != null)
                colored.color = color;
            else
                displayColor(color);
        }

        private void togglePicker(bool toggle)
        {
            if(picker == null) return;
            if(toggle)
            {
                picker.CurrentColor = colorDisplay.color;
                picker.onValueChanged.AddListener(ChangeColor);
            }
            else
                picker.onValueChanged.RemoveListener(ChangeColor);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if(pickerToggle.group != null)
                picker.gameObject.SetActive(pickerToggle.group.AnyTogglesOn());
            else
                picker.gameObject.SetActive(toggle);
        }
    }
}

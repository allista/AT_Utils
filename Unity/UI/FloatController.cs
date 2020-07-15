//   FloatController.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using System;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class FloatController : RingBoundedFloatValueUI
    {
        public float min = float.MinValue;
        public override float Min { get => min; set => min = value; }

        public float max = float.MaxValue;
        public override float Max { get => max; set => max = value; }

        public override FloatEvent onValueChanged { get; } = new FloatEvent();
        public FloatEvent onDoneEditing { get; } = new FloatEvent();

        public float step = 1;
        public int decimals = 1;
        private string format = "F1";

        public Button incrementButton;
        public Button decrementButton;
        public Button doneButton;
        public InputField input;
        public Text suffix;

        private void Awake()
        {
            updateFormat();
            updateIncDecButtons();
            incrementButton.onClick.AddListener(increment);
            decrementButton.onClick.AddListener(decrement);
            if(doneButton != null)
                doneButton.onClick.AddListener(done);
            input.contentType = InputField.ContentType.DecimalNumber;
            input.onEndEdit.AddListener(parse);
            input.text = value.ToString(format);
        }

        private void OnDestroy()
        {
            input.onEndEdit.RemoveAllListeners();
            incrementButton.onClick.RemoveAllListeners();
            decrementButton.onClick.RemoveAllListeners();
            if(doneButton != null)
                doneButton.onClick.RemoveAllListeners();
        }

        public void SetDecimals(int newDecimals)
        {
            decimals = newDecimals;
            updateFormat();
        }

        private void updateFormat()
        {
            format = decimals >= 0 ? $"F{decimals}" : "R";
        }

        public void SetStep(float newStep)
        {
            step = newStep;
            updateIncDecButtons();
        }

        private void updateIncDecButtons()
        {
            if(step.Equals(0))
            {
                incrementButton.gameObject.SetActive(false);
                decrementButton.gameObject.SetActive(false);
            }
            else
            {
                incrementButton.gameObject.SetActive(true);
                decrementButton.gameObject.SetActive(true);
                var stepDisplay = FormatUtils.formatBigValue(step, "");
                var txt = incrementButton.GetComponentInChildren<Text>();
                if(txt != null)
                    txt.text = $"+{stepDisplay}";
                txt = decrementButton.GetComponentInChildren<Text>();
                if(txt != null)
                    txt.text = $"-{stepDisplay}";
            }
        }

        public override bool SetValueWithoutNotify(float newValue)
        {
            newValue = clampValue(newValue);
            if(decimals >= 0)
                newValue = (float)Math.Round(newValue, decimals);
            input.SetTextWithoutNotify(newValue.ToString(format));
            if(value.Equals(newValue))
                return false;
            value = newValue;
            return true;
        }

        private void increment() => changeValueAndNotify(value + step);

        private void decrement() => changeValueAndNotify(value - step);

        private void done() => onDoneEditing?.Invoke(value);

        private void parse(string str_value)
        {
            if(float.TryParse(str_value, out var val))
                changeValueAndNotify(val);
        }
    }
}

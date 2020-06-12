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

        public float step = 1;
        public int decimals = 1;
        private string format = "F1";

        public Button incrementButton;
        public Button decrementButton;
        public InputField input;

        private void Awake()
        {
            input.contentType = InputField.ContentType.DecimalNumber;
            input.onEndEdit.AddListener(parse);
            if(decimals >= 0)
                format = string.Format("F{0}", decimals);
            else
                format = "R";
            input.text = value.ToString(format);
            if(step.Equals(0))
            {
                incrementButton.gameObject.SetActive(false);
                decrementButton.gameObject.SetActive(false);
            }
            else
            {
                incrementButton.onClick.AddListener(increment);
                decrementButton.onClick.AddListener(decrement);
                var txt = incrementButton.GetComponentInChildren<Text>();
                if(txt != null) txt.text = string.Format("+{0}", step);
                txt = decrementButton.GetComponentInChildren<Text>();
                if(txt != null) txt.text = string.Format("-{0}", step);
            }
        }

        void OnDestroy()
        {
            input.onEndEdit.RemoveListener(parse);
            incrementButton.onClick.RemoveListener(increment);
            decrementButton.onClick.RemoveListener(decrement);
        }

        public override bool SetValueWithoutNotify(float newValue)
        {
            var val = (float)Math.Round(clampValue(newValue), decimals);
            if(!value.Equals(val))
            {
                value = val;
                input.text = value.ToString(format);
                return true;
            }
            return false;
        }

        private void increment() => changeValueAndNotify(value + step);

        private void decrement() => changeValueAndNotify(value - step);

        private void parse(string str_value)
        {
            float val;
            if(float.TryParse(str_value, out val))
                changeValueAndNotify(val);
        }
    }
}

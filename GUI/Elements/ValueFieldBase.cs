//   ValueFieldBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public abstract class ValueFieldBase<T> : ConfigNodeObject, ITypeUI<T>
    {
        [Persistent] public T _value;
        protected string svalue;
        public string format;

        string _field_name;
        protected string field_name 
        {
            get 
            {
                if(string.IsNullOrEmpty(_field_name))
                    _field_name = "filed_"+GetHashCode().ToString("D");
                return _field_name;
            }
        }

        protected ValueFieldBase(string format) 
        { 
            this.format = format;
        }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            Value = _value;
        }

        protected abstract string formated_value { get ; }

        public virtual T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                svalue = formated_value;
            }
        }

        public bool IsSet { get; protected set; }

        public abstract void Invert();
        public abstract bool UpdateValue();

        protected bool TrySetValue()
        {
            IsSet = false;
            if(Event.current.isKey && 
               (Event.current.keyCode == KeyCode.Return ||
                Event.current.keyCode == KeyCode.KeypadEnter) && 
               GUI.GetNameOfFocusedControl() == field_name)
                IsSet = UpdateValue();
            return IsSet;
        }

        public abstract bool Draw();

        public override string ToString() { return formated_value; }

        public static implicit operator T(ValueFieldBase<T> fi) { return fi._value; }
    }

    public abstract class BoundValueField<T> : ValueFieldBase<T>
    {
        public T Min;
        public T Max;
        public bool Circle;

        protected BoundValueField(string format, T min, T max, bool circle) : base(format)
        {
            Circle = circle;
            Min = min; Max = max;
            Value = _value;
        }

        public abstract T Range { get; }
        public abstract void ClampValue();
    }
}


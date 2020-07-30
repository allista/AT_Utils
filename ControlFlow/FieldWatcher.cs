using System;
using System.Diagnostics.CodeAnalysis;

namespace AT_Utils
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class FieldWatcher
    {
        private readonly BaseField field;
        private object prevValue;

        public Callback onValueChanged;
        public Func<object, object, bool> isEqual = (a, b) => a.Equals(b);

        public FieldWatcher(BaseField field)
        {
            this.field = field;
            this.field.OnValueModified += onValueSet;
        }

        ~FieldWatcher()
        {
            field.OnValueModified -= onValueSet;
        }

        private void onValueSet(object value)
        {
            if(isEqual(value, prevValue))
            {
                if(field.host != null)
                    field.FieldInfo.SetValue(field.host, prevValue);
            }
            else
            {
                prevValue = value;
                onValueChanged?.Invoke();
            }
        }
    }
}

using System;

namespace AT_Utils
{
    public class FloatFieldWatcher : FieldWatcher
    {
        public float epsilon = 1e-6f;

        public FloatFieldWatcher(BaseField field) : base(field)
        {
            isEqual = floatsEqual;
        }

        private bool floatsEqual(object a, object b)
        {
            if(a is float fA && b is float fB)
                return Math.Abs(fA - fB) <= epsilon;
            return false;
        }
    }
}

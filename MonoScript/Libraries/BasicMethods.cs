using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Libraries
{
    public static class BasicMethods
    {
        public static dynamic InvokeMethod(string methodName, object obj)
        {
            if (methodName == "InvokeMethod")
                return null;

            return typeof(BasicMethods).GetMethods().FirstOrDefault(x => x.Name == methodName)?.Invoke(null, new object[] { obj });
        }

        public static dynamic ToString(object obj)
        {
            return obj?.ToString();
        }
        public static dynamic ToNumber(object obj)
        {
            double result;

            if (double.TryParse(obj?.ToString(), out result))
                return result;

            return null;
        }
        public static dynamic ToBoolean(object obj)
        {
            bool result;

            if (bool.TryParse(obj?.ToString(), out result))
                return result;

            return null;
        }
        public static dynamic ToLower(object obj)
        {
            return obj?.ToString().ToLower();
        }
        public static dynamic ToUpper(object obj)
        {
            return obj?.ToString().ToUpper();
        }
    }
}

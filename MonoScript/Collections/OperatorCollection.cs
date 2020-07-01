using MonoScript.Models;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MonoScript.Collections
{
    public static class OperatorCollection
    {
        public static Operator GetOverloadOperator(string name) => typeof(OperatorCollection).GetProperty(name)?.GetValue(null) as Operator;
        public static Operator GetOperatorBySign(string sign)
        {
            if (string.IsNullOrEmpty(sign))
                return null;

            if (sign == "<")
                return Less;

            if (sign == "<=")
                return LessEqual;

            if (sign == ">")
                return More;

            if (sign == ">=")
                return MoreEqual;

            if (sign == "==")
                return DoubleEqual;

            if (sign == "!=")
                return NotEqual;

            if (sign[0] == '/')
                return Divide;

            if (sign[0] == '%')
                return DivideProcent;

            if (sign[0] == '*')
                return Multiply;

            if (sign[0] == '-')
                return Minus;

            if (sign[0] == '+')
                return Plus;

            return null;
        }
        public static Operator GetElement { get; } = new Operator(-1, "GetElement");
        public static Operator Less { get; } = new Operator(2, "Less");
        public static Operator LessEqual { get; } = new Operator(2, "LessEqual");
        public static Operator More { get; } = new Operator(2, "More");
        public static Operator MoreEqual { get; } = new Operator(2, "More");
        public static Operator DoubleEqual { get; } = new Operator(2, "DoubleEqual");
        public static Operator NotEqual { get; } = new Operator(2, "NotEqual");
        public static Operator Plus { get; } = new Operator(2, "Plus");
        public static Operator Minus { get; } = new Operator(2, "Minus");
        public static Operator Multiply { get; } = new Operator(2, "Multiply");
        public static Operator Divide { get; } = new Operator(2, "Divide");
        public static Operator DivideProcent { get; } = new Operator(2, "DivideProcent");
    }
}

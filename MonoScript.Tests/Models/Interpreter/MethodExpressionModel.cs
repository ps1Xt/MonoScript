using MonoScript.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Interpreter
{
    public class MethodExpressionModel
    {
        public string MethodName { get; set; }
        public int OpenBracketCount { get; set; }
        public bool HasOpenBracket { get => OpenBracketCount > 0; }

        public void Read(string expression, int index)
        {
            if (expression[index].Contains(ReservedCollection.AllowedNames))
                MethodName += expression[index];
            else if (expression[index] != '(')
                MethodName = null;
        }
    }
}

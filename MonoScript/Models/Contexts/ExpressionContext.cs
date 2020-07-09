using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Contexts
{
    public class ExpressionContext
    {
        public bool ReverseBool { get; set; }

        public ExpressionContext() { }
        public ExpressionContext(bool reverseBool)
        {
            ReverseBool = reverseBool;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Interpreter
{
    public class SquareBracketExpressionModel
    {
        public int OpenBracketCount { get; set; }
        public bool HasOpenBracket { get => OpenBracketCount > 0; }
    }
}

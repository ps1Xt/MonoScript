using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Script
{
    public class Operator
    {
        public string Name { get; }
        public int Parameters { get; }

        public Operator(string name) => Name = name;
        public Operator(int parameters, string name)
        {
            Parameters = parameters;
            Name = name;
        }
    }
}

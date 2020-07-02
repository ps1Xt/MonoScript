using MonoScript.Script.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Script
{
    public class Parent<ParentType>
    {
        public string StringValue { get; set; }
        public ParentType ObjectValue { get; set; }
    }
}

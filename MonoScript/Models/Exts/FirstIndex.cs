using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Exts
{
    public class FirstIndex
    {
        public char FirstChar { get; set; }
        public bool IsFirst { get; set; }
        public int Position { get; set; }

        public static FirstIndex Null { get; } = new FirstIndex() { IsFirst = false };
    }
}

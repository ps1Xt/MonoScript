using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Exts
{
    public class InsideQuoteModel
    {
        public bool HasQuotes { get => Quote != null; }
        public char? Quote { get; set; }
        public bool IsOnlyString { get; set; }
        public int Line { get; set; }
    }
}

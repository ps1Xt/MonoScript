using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Collections
{
    public static class PortableCharacterCollection
    {
        public static char? GetCharacterByString(string value)
        {
            switch (value)
            {
                case @"\0": return Null;
                case @"\a": return Alert;
                case @"\b": return Backspace;
                case @"\t": return Tab;
                case @"\n": return NewLine;
                case @"\v": return VerticalTab;
                case @"\f": return FormFeed;
                case @"\r": return CarriageReturn;
                case @"\\": return Space;
                case @"\'": return '\'';
                case @"\""": return '"';
                default:
                    break;
            }

            return null;
        }

        public static char Null { get; } = '\0';
        public static char Alert { get; } = '\a';
        public static char Backspace { get; } = '\b';
        public static char Tab { get; } = '\t';
        public static char NewLine { get; } = '\n';
        public static char VerticalTab { get; } = '\v';
        public static char FormFeed { get; } = '\f';
        public static char CarriageReturn { get; } = '\r';
        public static char Space { get; } = '\\';
    }
}

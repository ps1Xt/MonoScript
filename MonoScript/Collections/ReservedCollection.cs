using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Collections
{
    public static class ReservedCollection
    {
        public static string Alphabet { get; } = "qwertyuiopasdfghjklzxcvbnm";
        public static string Numbers { get; } = "0123456789";
        public static string NumberOperations { get; } = "/%*+-";
        public static string NumberSeparators { get; } = ".,";
        public static string Quotes { get; } = "'\"";
        public static string AllowedNames { get; } = Alphabet + Numbers + "_@";
        public static string WhiteSpace { get; } = "\n\r\t ";

        public static string RootNamespace { get; } = "RootSpace";
        public static string RootClass { get; } = "Program";
        public static string RootMethod { get; } = "Main";
    }
}

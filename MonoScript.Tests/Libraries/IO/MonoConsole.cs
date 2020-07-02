using MonoScript.Models.Analytics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MonoScript.Libraries.IO
{
    public static class MonoConsole
    {
        public static string NewLine = "\r\n";

        public delegate void WriteApplicationMessage(AppMessage message);
        public delegate void WriteDelegate(string text);
        public delegate string ReadLineDelegate();
        public delegate char ReadKeyDelegate();

        public static event WriteApplicationMessage WriteApplicationMessageEvent;
        public static event WriteDelegate WriteEvent;
        public static event ReadLineDelegate ReadLineEvent;
        public static event ReadKeyDelegate ReadKeyEvent;

        public static void WriteAppMessage(AppMessage message) => WriteApplicationMessageEvent?.Invoke(message);
        public static void Write(string value) => WriteEvent?.Invoke(value);
        public static void Write(object value) => WriteEvent?.Invoke(GetTextBlocksFromArray(value));
        public static void Write(int value) => WriteEvent?.Invoke(value.ToString());
        public static void Write(bool value) => WriteEvent?.Invoke(value.ToString());
        public static void WriteLine(string value) => WriteEvent?.Invoke(value + NewLine);
        public static void WriteLine(object value) => WriteEvent?.Invoke(GetTextBlocksFromArray(value + NewLine));
        public static void WriteLine(int value) => WriteEvent?.Invoke(value.ToString() + NewLine);
        public static void WriteLine(bool value) => WriteEvent?.Invoke(value ? "True" : "False" + NewLine);
        public static void WriteLine() => WriteEvent?.Invoke("\n");
        public static string ReadLine() => ReadLineEvent?.Invoke();
        public static char? ReadKey() => ReadKeyEvent?.Invoke();

        public static string GetTextBlocksFromArray(object value)
        {
            string text = string.Empty;

            if (Extensions.HasEnumerator(value) && !(value is string))
            {
                int index = 0;
                foreach (var str in value as List<dynamic>)
                {
                    if (Extensions.HasEnumerator(str))
                    {
                        if (index == (value as List<dynamic>).Count - 1)
                            text += " " + GetTextBlocksFromArray(str) + " ";
                        else
                            text += " " + GetTextBlocksFromArray(str) + ",";
                    }
                    else
                    {
                        if (index == (value as List<dynamic>).Count - 1)
                            text += " " + str.ToString() + " ";
                        else
                            text += " " + str.ToString() + ",";
                    }

                    index++;
                }

                text = string.Format("[{0}]", text);
            }
            else
                text = value.ToString();

            return text;
        }
    }
}

using MonoScript.Collections;
using MonoScript.Models;
using System.Collections.Generic;

namespace MonoScript
{
    public static class Extensions
    {
        public static bool Contains(this char chr, string arr, bool checkRegister = false)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (checkRegister && arr[i] == chr)
                    return true;
                if (!checkRegister && arr[i].ToString().ToLower() == chr.ToString().ToLower())
                    return true;
            }

            return false;
        }
        public static bool Contains(this List<string> arr, params string[] values)
        {
            foreach (var item in arr)
                foreach (var item2 in values)
                    if (item == item2)
                        return true;

            return false;
        }
        public static bool Contains(this string arr, string arr2)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < arr2.Length; j++)
                {
                    if (arr[i] == arr2[j])
                        return true;
                }
            }

            return false;
        }
        public static void IsOpenQuote(string expression, int pos, ref InsideQuoteModel quoteModel)
        {
            if (expression[pos].Contains(ReservedCollection.Quotes))
            {
                if (quoteModel.Quote == null)
                {
                    if (pos - 1 >= 0 && expression[pos - 1] == '@')
                        quoteModel.IsOnlyString = true;

                    quoteModel.Quote = expression[pos];
                }
                else if (quoteModel.Quote == expression[pos])
                {
                    if (!quoteModel.IsOnlyString)
                    {
                        if (pos - 1 >= 0)
                        {
                            if (expression[pos - 1] == '\\')
                            {
                                int backSlashCount = 0;

                                for (int i = pos - 1; i >= 0; i--)
                                {
                                    if (expression[i] == '\\')
                                        backSlashCount++;
                                    else
                                        break;
                                }

                                if (backSlashCount % 2 == 0)
                                {
                                    quoteModel.Quote = null;
                                    quoteModel.IsOnlyString = false;
                                }
                            }
                            else
                            {
                                quoteModel.Quote = null;
                                quoteModel.IsOnlyString = false;
                            }
                        }
                    }
                    else
                    {
                        quoteModel.Quote = null;
                        quoteModel.IsOnlyString = false;
                    }
                }
            }

            if (expression[pos].Contains("\r\n") && !quoteModel.IsOnlyString)
                quoteModel.Quote = null;
        }
        public static InsideQuoteModel InsideQuotes(string expression, int startIndex, int inputLine = 0)
        {
            InsideQuoteModel quoteModel = new InsideQuoteModel();
            bool endline = false;
            int backSlashCount = 0;
            int currentLine = inputLine;

            for (int i = startIndex; i >= 0; i--)
            {
                if (expression[i] == '\n' || i == 0)
                {
                    endline = true;

                    if (expression[i] == '\n')
                    {
                        quoteModel = InsideQuotes(expression, i - 1, currentLine + 1);

                        if (!quoteModel.IsOnlyString)
                            quoteModel.Quote = null;
                    }
                }

                if (endline)
                {
                    for (; i < startIndex; i++)
                    {
                        if (quoteModel.Quote == null && expression[i].Contains(ReservedCollection.Quotes))
                        {
                            if (i - 1 >= 0 && expression[i - 1] == '@')
                                quoteModel.IsOnlyString = true;

                            quoteModel.Line = currentLine;
                            quoteModel.Quote = expression[i];
                            continue;
                        }

                        if (quoteModel.Quote != null)
                        {
                            if (!quoteModel.IsOnlyString)
                            {
                                if (expression[i] == '\\')
                                    backSlashCount++;

                                if (!expression[i].Contains("\\" + ReservedCollection.Quotes))
                                    backSlashCount = 0;
                            }

                            if (expression[i] == quoteModel.Quote)
                            {
                                if (quoteModel.IsOnlyString)
                                {
                                    quoteModel.Quote = null;
                                    quoteModel.IsOnlyString = false;
                                }
                                else if (currentLine == quoteModel.Line && backSlashCount % 2 == 0)
                                {
                                    quoteModel.Quote = null;
                                    quoteModel.IsOnlyString = false;
                                }
                            }
                        }
                    }

                    break;
                }
            }

            return quoteModel;
        }
        public static string GetPrefixRegex(string suffix, string prefix = ";")
        {
            return "(([\\s{}" + prefix + "]" + suffix + $")|(^{suffix}))";
        }
        public static string SubstringIndex(this string str, int start, int last)
        {
            string sbs = string.Empty;
            for (int i = start; i < last; i++)
            {
                sbs += str[i];
            }

            return sbs;
        }
        public static string SubstringIndex(this string str, int start, string endChars)
        {
            string sbs = string.Empty;
            for (int i = start; i < str.Length && !str[i].Contains(endChars); i++)
            {
                sbs += str[i];
            }

            return sbs;
        }
        public static string RemoveIndex(this string str, int start, int last)
        {
            return str.SubstringIndex(0, start) + str.SubstringIndex(last + 1, str.Length);
        }
        public static bool HasEnumerator(dynamic obj)
        {
            try
            {
                var result = obj?.GetEnumerator();

                return !(result is null);
            }
            catch { return false; }
        }
    }
}

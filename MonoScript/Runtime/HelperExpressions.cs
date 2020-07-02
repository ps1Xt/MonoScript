using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Contexts;
using MonoScript.Models.Interpreter;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Runtime
{
    public static class HelperExpressions
    {
        public static List<string> GetStringMethodParameters(string expression, ref int index)
        {
            string tmpex = string.Empty;
            List<string> values = new List<string>();

            InsideQuoteModel quoteModel = new InsideQuoteModel();
            char? openBracketChar = null;
            int openCount = 0;

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes || openCount > 1 || (!quoteModel.HasQuotes && !expression[index].Contains(",[]()")))
                    tmpex += expression[index];

                if (!quoteModel.HasQuotes)
                {
                    if (openCount == 1 && expression[index] == ',')
                    {
                        values.Add(tmpex);
                        tmpex = string.Empty;
                        continue;
                    }

                    if (openBracketChar == null)
                    {
                        if (expression[index].Contains("(["))
                        {
                            openBracketChar = expression[index];
                            openCount++;
                        }
                    }
                    else
                    {
                        if (expression[index] == openBracketChar)
                        {
                            openCount++;
                            continue;
                        }

                        if ((openBracketChar == '(') && expression[index] == ')' || (openBracketChar == '[' && expression[index] == ']'))
                            openCount--;

                        if (openCount == 0)
                        {
                            index++;

                            if (!string.IsNullOrWhiteSpace(tmpex))
                                values.Add(tmpex);

                            break;
                        }
                    }
                }
            }

            return values;
        }
        public static List<(string Name, dynamic Value)> GetObjectMethodParameters(List<string> stringValues, FindContext context)
        {
            List<(string, dynamic)> methodInputs = new List<(string, dynamic)>();
            bool hasFieldName = false;

            for (int index = 0; index < stringValues.Count; index++)
            {
                if (index == 0)
                {
                    int indexOf = stringValues[index].IndexOf("=");

                    if (indexOf != -1 && !Extensions.InsideQuotes(stringValues[index], indexOf).HasQuotes)
                        hasFieldName = true;

                    if (hasFieldName)
                    {
                        int pos = 0;
                        string[] splitValues = stringValues[index].Split('=', 2);
                        string objName = splitValues[0].Trim(' ');

                        methodInputs.Add((objName, MonoInterpreter.ExecuteConditionalExpression(splitValues[1], context, ref pos)));
                        continue;
                    }
                }

                if (hasFieldName)
                {
                    int indexOf = stringValues[index].IndexOf("=");

                    if (indexOf != -1 && !Extensions.InsideQuotes(stringValues[index], indexOf).HasQuotes)
                    {
                        int pos = 0;
                        string[] splitValues = stringValues[index].Split('=', 2);
                        string objName = splitValues[0].Trim(' ');

                        methodInputs.Add((objName, MonoInterpreter.ExecuteConditionalExpression(splitValues[1], context, ref pos)));
                    }
                    else MLog.AppErrors.Add(new AppMessage("No assignment operator found for method object.", stringValues[index]));
                }
                else
                {
                    int pos = 0;

                    methodInputs.Add((null, MonoInterpreter.ExecuteConditionalExpression(stringValues[index], context, ref pos)));
                }
            }

            return methodInputs;
        }
        public static LocalSpace GetMethodLocalSpace(List<Field> parameters, List<(string Name, dynamic Value)> parameters2, string methodFullpath)
        {
            LocalSpace localSpace = new LocalSpace(null);

            if (parameters.Count == 0 || parameters.Count != parameters.Count)
                return localSpace;

            if (parameters2[0].Name != null)
            {
                foreach (var parameter in parameters)
                {
                    foreach (var parameter2 in parameters2)
                    {
                        if (parameter.Name == parameter2.Name)
                        {
                            if (!localSpace.Add(new Field(parameter.FullPath, parameter.ParentObject) { Modifiers = parameter.Modifiers, Value = parameter.Value }))
                                MLog.AppErrors.Add(new AppMessage("Method input parameters are repeated.", $"Path {methodFullpath}"));
                        }
                    }
                }
            }
            else
            {
                for (int index = 0; index < parameters.Count; index++)
                {
                    if (!localSpace.Add(new Field(parameters[index].FullPath, parameters[index].ParentObject) { Modifiers = parameters[index].Modifiers, Value = parameters2[index].Value }))
                                MLog.AppErrors.Add(new AppMessage("Method input parameters are repeated.", $"Path {methodFullpath}"));
                }
            }

            return localSpace;
        }
    }
}

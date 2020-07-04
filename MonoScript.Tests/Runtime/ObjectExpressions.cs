using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MonoScript.Runtime
{
    public static class ObjectExpressions
    {
        [Fact]
        public static void ExecuteThisExpression()
        {
            int index = 0;
            string expression = "this.";
            string resultString = string.Empty;
            FindContext context = new FindContext(false);

            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 't' && expression[index + 1] == 'h' && expression[index + 2] == 'i' && expression[index + 3] == 's')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            resultString = "this";
                        if (expression.Length == 4)
                            resultString = "this";
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 't' && expression[index + 1] == 'h' && expression[index + 2] == 'i' && expression[index + 3] == 's')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            resultString = "this";
                        if (expression.Length == 4)
                            resultString = "this";
                    }
                }
            }

            if (resultString == "this")
            {
                if (context.IsStaticContext)
                    MLog.AppErrors.Add(new AppMessage("Operator this cannot be called in a static class.", expression));
            }

            Assert.Equal("this", resultString);
        }
        [Fact]
        public static void ExecuteNullExpression()
        {
            int index = 0;
            string expression = "null.";
            string result = "";

            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 'n' && expression[index + 1] == 'u' && expression[index + 2] == 'l' && expression[index + 3] == 'l')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            result = null;
                        if (expression.Length == 4)
                            result = null;
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 'n' && expression[index + 1] == 'u' && expression[index + 2] == 'l' && expression[index + 3] == 'l')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            result = null;
                        if (expression.Length == 4)
                            result = null;
                    }
                }
            }

            Assert.Null(result);
        }
        [Fact]
        public static void ExecuteBooleanExpression()
        {
            int index = 1;
            string expression = ".true.";
            bool result = false;

            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 't' && expression[index + 1] == 'r' && expression[index + 2] == 'u' && expression[index + 3] == 'e')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            result = true;
                        if (expression.Length == 4)
                            result = true;
                    }

                    if (expression[index] == 'f' && expression[index + 1] == 'a' && expression[index + 2] == 'l' && expression[index + 3] == 's' && expression[index + 4] == 'e')
                    {
                        index += 4;

                        if (index + 5 >= expression.Length || !expression[index + 5].Contains(ReservedCollection.AllowedNames))
                            result = false;
                        if (expression.Length == 5)
                            result = false;
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 't' && expression[index + 1] == 'r' && expression[index + 2] == 'u' && expression[index + 3] == 'e')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            result = true;
                        if (expression.Length == 4)
                            result = true;
                    }

                    if (expression[index] == 'f' && expression[index + 1] == 'a' && expression[index + 2] == 'l' && expression[index + 3] == 's' && expression[index + 4] == 'e')
                    {
                        index += 4;

                        if (index + 5 >= expression.Length || !expression[index + 5].Contains(ReservedCollection.AllowedNames))
                            result = false;
                        if (expression.Length == 5)
                            result = false;
                    }
                }
            }

            Assert.True(result);
        }
        [Fact]
        public static void ExecuteNumberExpression()
        {
            int index = 0;
            string expression = "2. 200.d. ToString()";
            double expected = 2;

            string numberString = string.Empty;
            bool hasBodyNumber = false, hasResidue = false;
            double resultDouble = -1;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (!quoteModel.HasQuotes)
                {
                    if (expression[index].Contains(ReservedCollection.Numbers))
                    {
                        numberString += expression[index];

                        if (!hasResidue)
                            hasBodyNumber = true;
                    }
                    else
                    {
                        if (!hasResidue)
                        {
                            if (expression[index] == '.')
                            {
                                if (hasBodyNumber)
                                {
                                    if (index + 1 < expression.Length)
                                    {
                                        if (expression[index + 1].Contains(ReservedCollection.Numbers))
                                        {
                                            hasResidue = true;
                                            numberString += ",";
                                        }
                                        else
                                        {
                                            if (expression[index + 1] == ' ')
                                            {
                                                double.TryParse(numberString, out resultDouble);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        double.TryParse(numberString, out resultDouble);
                                        break;
                                    }
                                }
                                else
                                {
                                    if (index + 1 < expression.Length)
                                    {
                                        if (expression[index + 1].Contains(ReservedCollection.Numbers))
                                        {
                                            hasResidue = true;
                                            numberString += ",";
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                            }
                            if (expression[index] == ' ')
                            {
                                if (hasBodyNumber)
                                {
                                    double.TryParse(numberString, out resultDouble);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (expression[index].Contains(". "))
                            {
                                if (hasBodyNumber)
                                {
                                    double.TryParse(numberString, out resultDouble);
                                    break;
                                }
                                else
                                {
                                    if (numberString.Length >= 2)
                                    {
                                        double.TryParse(numberString, out resultDouble);
                                        break;
                                    }
                                    else
                                    {
                                        MLog.AppErrors.Add(new AppMessage("Invalid number declaration.", expression));
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                MLog.AppErrors.Add(new AppMessage("Invalid number declaration.", expression));
                                break;
                            }
                        }
                    }
                }
                else
                {
                    MLog.AppErrors.Add(new AppMessage("Invalid line declaration after number.", expression));
                    break;
                }
            }

            Assert.Equal(expected, resultDouble);
        }
        [Fact]
        public static void ExecuteStringExpression()
        {
            string expression = @"@'data\'file'ffff";
            string expected = @"data\";

            InsideQuoteModel quoteModel = new InsideQuoteModel();
            bool firstOpen = quoteModel == null ? false : quoteModel.HasQuotes;
            int index = 0;
            string resultString = string.Empty;

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes)
                {
                    firstOpen = true;

                    if (quoteModel.IsOnlyString && expression[index] != quoteModel.Quote)
                        resultString += expression[index];

                    if (!quoteModel.IsOnlyString)
                    {
                        if (expression[index] == '\\')
                        {
                            if (index + 1 < expression.Length)
                            {
                                char? portableCharacter = PortableCharacterCollection.GetCharacterByString(new string(new char[] { expression[index], expression[index + 1] }));

                                if (portableCharacter != null)
                                {
                                    index++;
                                    resultString += portableCharacter.Value;
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("Special character not found.", expression));
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("No special character specified.", expression));
                        }
                        else if (expression[index] != quoteModel.Quote)
                            resultString += expression[index];
                    }
                }
                else if (firstOpen)
                    break;
            }

            Assert.Equal(expected, resultString);
        }
        public static dynamic ExecuteArrayExpression(ref int index, string expression, FindContext context)
        {
            List<dynamic> arrayValues = new List<dynamic>();

            string tmpex = null;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes || (!quoteModel.HasQuotes && !expression[index].Contains("[],")))
                    tmpex += expression[index];

                if (!quoteModel.HasQuotes)
                {
                    if (expression[index] == ',')
                    {
                        int pos = 0;

                        var arrValue = MonoInterpreter.ExecuteConditionalExpression(tmpex, context, ref pos);

                        if (arrValue is Field field)
                            arrValue = field.Value;

                        arrayValues.Add(arrValue);

                        tmpex = null;
                        continue;
                    }

                    if (expression[index] == '(')
                    {
                        InsideQuoteModel subQuoteModel = new InsideQuoteModel();
                        int opencount = 1;
                        index++;

                        for (; index < expression.Length; index++)
                        {
                            tmpex += expression[index];
                            Extensions.IsOpenQuote(expression, index, ref subQuoteModel);

                            if (!subQuoteModel.HasQuotes)
                            {
                                if (expression[index] == '(')
                                    opencount++;

                                if (expression[index] == ')')
                                    opencount--;

                                if (opencount == 0)
                                    break;
                            }
                        }

                        continue;
                    }

                    if (expression[index] == '[')
                    {
                        index++;
                        arrayValues.Add(ExecuteArrayExpression(ref index, expression, context));
                        continue;
                    }

                    if (expression[index] == ']')
                    {
                        int pos = 0;

                        var arrValue = MonoInterpreter.ExecuteConditionalExpression(tmpex, context, ref pos);

                        if (arrValue is Field field)
                            arrValue = field.Value;

                        arrayValues.Add(arrValue);

                        break;
                    }
                }
            }

            return arrayValues;
        }
        public static dynamic ExecuteVarExpression(ref int index, string expression, FindContext context)
        {
            index++;

            for (; index < expression.Length; index++)
            {

            }
            
            return null;
        }
        public static ExecuteResult ExecuteForExpression(ref int index, string expression, FindContext context)
        {
            ExecuteResult executeResult = new ExecuteResult();


            return executeResult;
        }
        public static ExecuteResult ExecuteForeachExpression(ref int index, string expression, FindContext context)
        {
            ExecuteResult executeResult = new ExecuteResult();


            return executeResult;
        }
        public static ExecuteResult ExecuteWhileExpression(ref int index, string expression, FindContext context)
        {
            ExecuteResult executeResult = new ExecuteResult();


            return executeResult;
        }
        public static ExecuteResult ExecuteDoWhileExpression(ref int index, string expression, FindContext context)
        {
            ExecuteResult executeResult = new ExecuteResult();


            return executeResult;
        }
        public static ExecuteResult ExecuteSwitchExpression(ref int index, string expression, FindContext context)
        {
            ExecuteResult executeResult = new ExecuteResult();


            return executeResult;
        }
        public static ExecuteResult ExecuteIfExpression(ref int index, string expression, FindContext context)
        {
            ExecuteResult executeResult = new ExecuteResult();


            return executeResult;
        }
        public static dynamic ExecuteMethod(Method method, LocalSpace localSpace)
        {
            FindContext context = new FindContext(method.Modifiers.Contains("static")) { LocalSpace = localSpace, MonoType = method.ParentObject as MonoType };
            context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

            var result = MonoInterpreter.ExecuteScript(method.Content, context, ExecuteScriptContextCollection.Method);

            if (result is Field field)
                result = field.Value;

            return result;
        }
        public static dynamic ExecuteConstructor(Method method, LocalSpace localSpace)
        {
            FindContext context = new FindContext(method.Modifiers.Contains("static")) { LocalSpace = localSpace };

            if (method.ParentObject is Class objClass)
            {
                var newObj = objClass.CloneObject();

                context.MonoType = newObj;
                context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

                MonoInterpreter.ExecuteScript(method.Content, context, ExecuteScriptContextCollection.Method);

                return newObj;
            }
            
            if (method.ParentObject is Struct objStruct)
            {
                var newObj = objStruct.CloneObject();

                context.MonoType = newObj;
                context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

                MonoInterpreter.ExecuteScript(method.Content, context, ExecuteScriptContextCollection.Method);

                return newObj;
            }

            return null;
        }
    }
}

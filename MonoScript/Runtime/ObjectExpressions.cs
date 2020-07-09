﻿using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Libraries;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Models.Exts;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Runtime
{
    public static class ObjectExpressions
    {
        public static ExecuteLogicalResult ExecuteReverseBooleanExpression(ref int index, string expression, ref string tmpex, ref bool? lastBool, ref dynamic lastObj, InsideQuoteModel insideQuote, FindContext context, ExpressionContext expressionContext)
        {
            ExpressionContext newExpressionContext = new ExpressionContext(true);
            FirstIndex firstIndex = expression.FindFirstIndex(index, "(", ReservedCollection.WhiteSpace);
            index = firstIndex.Position;

            if (firstIndex.IsFirst)
            {
                var executeResult = MonoInterpreter.ExecuteConditionalExpression(expression, ref index, context, newExpressionContext);

                if (executeResult is bool)
                {
                    lastObj = !executeResult;
                    tmpex = lastObj.ToString().ToLower();

                    if (index < expression.Length && index - 1 >= 0 && expression[index - 1] == ')')
                        index--;

                    return ExecuteLogicalResult.BreakResult;
                }
                else
                    MLog.AppErrors.Add(new AppMessage("Incorrect use of the value conversion operator.", expression));
            }
            else
            {
                //!!!(!user.file && true)

            }

            return ExecuteLogicalResult.ReturnResult(null);
        }
        public static ExecuteLogicalResult ExecuteLogicalOrExpression(ref int index, string expression, ref string tmpex, ref bool? lastBool, ref dynamic lastObj, InsideQuoteModel insideQuote, FindContext context, ExpressionContext expressionContext)
        {
            if (!lastBool.HasValue)
            {
                var executeResult = MonoInterpreter.ExecuteEqualityExpression(tmpex, context);

                if (executeResult is bool)
                {
                    lastBool = executeResult;
                    lastObj = executeResult;
                    tmpex = string.Empty;
                }
                else
                {
                    MLog.AppErrors.Add(new AppMessage("Only logical entities can participate in a conditional statement.", tmpex));
                    return ExecuteLogicalResult.ReturnResult(null);
                }
            }
            else
            {
                if (!lastBool.Value)
                {
                    lastBool = null;
                    lastObj = null;
                    tmpex = string.Empty;
                    return ExecuteLogicalResult.ContinueResult;
                }
                else
                {
                    char? bracketChar = null;
                    int bracketRoundCount = 0;
                    int bracketSquareCount = 0;

                    for (; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref insideQuote);

                        if (!insideQuote.HasQuotes)
                        {
                            if (bracketChar == null && expression[index].Contains("(["))
                            {
                                if (expression[index] == '(')
                                    bracketRoundCount++;
                                if (expression[index] == '[')
                                    bracketSquareCount++;

                                bracketChar = expression[index];
                                continue;
                            }

                            if (bracketChar == '[' && expression[index] == '[')
                                bracketSquareCount++;

                            if (bracketChar == '[' && expression[index] == ']')
                                bracketSquareCount--;

                            if (bracketChar == '(' && expression[index] == '(')
                                bracketRoundCount++;

                            if (bracketChar == '(' && expression[index] == ')')
                            {
                                bracketRoundCount--;

                                if (bracketRoundCount == -1)
                                    break;
                            }

                            if (bracketChar == null && expression[index] == ')')
                                break;
                        }
                    }

                    if (index >= expression.Length || expression[index] == ')')
                    {
                        index++;
                        return ExecuteLogicalResult.ReturnResult(true);
                    }
                }
            }

            return ExecuteLogicalResult.NoneResult;
        }
        public static ExecuteLogicalResult ExecuteLogicalAndExpression(ref int index, string expression, ref string tmpex, ref bool? lastBool, ref dynamic lastObj, InsideQuoteModel insideQuote, FindContext context, ExpressionContext expressionContext)
        {
            if (!lastBool.HasValue)
            {
                var executeResult = MonoInterpreter.ExecuteEqualityExpression(tmpex, context);

                if (executeResult is bool)
                {
                    lastBool = executeResult;
                    lastObj = executeResult;
                    tmpex = string.Empty;
                }
                else
                {
                    MLog.AppErrors.Add(new AppMessage("Only logical entities can participate in a conditional statement.", tmpex));
                    return ExecuteLogicalResult.ReturnResult(null);
                }
            }
            else
            {
                if (lastBool.Value)
                {
                    tmpex = string.Empty;
                    return ExecuteLogicalResult.ContinueResult;
                }
                else
                {
                    tmpex = string.Empty;
                    char? bracketChar = null;
                    int bracketRoundCount = 0;
                    int bracketSquareCount = 0;

                    for (; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref insideQuote);

                        if (!insideQuote.HasQuotes)
                        {
                            if (bracketChar == null && expression[index].Contains("(["))
                            {
                                if (expression[index] == '(')
                                    bracketRoundCount++;
                                if (expression[index] == '[')
                                    bracketSquareCount++;

                                bracketChar = expression[index];
                                continue;
                            }

                            if (expression[index] == '|' && bracketRoundCount == 0 && bracketSquareCount == 0)
                            {
                                if (index < expression.Length && expression[index + 1] == '|')
                                {
                                    index++;
                                    continue;
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("Incorrect operator declaration or. |", tmpex));
                            }

                            if (bracketChar == '[' && expression[index] == '[')
                                bracketSquareCount++;

                            if (bracketChar == '[' && expression[index] == ']')
                                bracketSquareCount--;

                            if (bracketChar == '(' && expression[index] == '(')
                                bracketRoundCount++;

                            if (bracketChar == '(' && expression[index] == ')')
                            {
                                bracketRoundCount--;

                                if (bracketRoundCount == -1)
                                    break;
                            }

                            if (bracketChar == null && expression[index] == ')')
                                break;
                        }
                    }

                    if (index >= expression.Length || expression[index] == ')')
                    {
                        index++;
                        return ExecuteLogicalResult.ReturnResult(true);
                    }
                }
            }

            return ExecuteLogicalResult.NoneResult;
        }
        public static dynamic ExecuteDecrementExpression(ref int index, string expression, string objectPath, dynamic lastObj, FindContext context)
        {
            if (lastObj == null)
            {
                if (string.IsNullOrWhiteSpace(objectPath))
                {
                    InsideQuoteModel quoteModel = new InsideQuoteModel();
                    for (; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref quoteModel);

                        if (!quoteModel.HasQuotes)
                        {
                            if (expression[index].Contains(ReservedCollection.AllowedNames))
                                objectPath += expression[index];
                            else if (!expression[index].Contains("\r\n\t "))
                                break;
                        }
                        else
                            break;
                    }

                    lastObj = Finder.FindObject(objectPath, context);

                    if (lastObj is Field field && field.Value is double)
                        --field.Value;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use decrement is possible only for fields of type Number.", expression));
                }
                else
                {
                    lastObj = Finder.FindObject(objectPath, context);

                    if (lastObj is Field field && field.Value is double)
                        field.Value--;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use decrement is possible only for fields of type Number.", expression));
                }
            }
            else
            {
                if (lastObj is Field field && field.Value is double)
                    field.Value--;
                else
                    MLog.AppErrors.Add(new AppMessage("Use decrement is possible only for fields of type Number.", expression));
            }


            return lastObj;
        }
        public static dynamic ExecuteIncrementExpression(ref int index, string expression, string objectPath, dynamic lastObj, FindContext context)
        {
            if (lastObj == null)
            {
                if (string.IsNullOrWhiteSpace(objectPath))
                {
                    InsideQuoteModel quoteModel = new InsideQuoteModel();
                    for (; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref quoteModel);

                        if (!quoteModel.HasQuotes)
                        {
                            if (expression[index].Contains(ReservedCollection.AllowedNames))
                                objectPath += expression[index];
                            else if (!expression[index].Contains("\r\n\t "))
                                    break;
                        }
                        else
                            break;
                    }

                    lastObj = Finder.FindObject(objectPath, context);

                    if (lastObj is Field field && field.Value is double)
                        ++field.Value;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use increment is possible only for fields of type Number.", expression));
                }
                else
                {
                    lastObj = Finder.FindObject(objectPath, context);

                    if (lastObj is Field field && field.Value is double)
                        field.Value++;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use increment is possible only for fields of type Number.", expression));
                }
            }
            else
            {
                if (lastObj is Field field && field.Value is double)
                    field.Value++;
                else
                    MLog.AppErrors.Add(new AppMessage("Use increment is possible only for fields of type Number.", expression));
            }


            return lastObj;
        }
        public static dynamic ExecuteThisExpression(ref int index, string expression, FindContext context)
        {
            string resultString = string.Empty;

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
                if (context.IsStaticObject)
                {
                    MLog.AppErrors.Add(new AppMessage("Operator this cannot be called in a static class.", expression));
                    return false;
                }
                else
                    return context.MonoType;
            }

            return null;
        }
        public static dynamic ExecuteNullExpression(ref int index, string expression)
        {
            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 'n' && expression[index + 1] == 'u' && expression[index + 2] == 'l' && expression[index + 3] == 'l')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            return null;
                        if (expression.Length == 4)
                            return null;
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 'n' && expression[index + 1] == 'u' && expression[index + 2] == 'l' && expression[index + 3] == 'l')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            return null;
                        if (expression.Length == 4)
                            return null;
                    }
                }
            }

            return false;
        }
        public static dynamic ExecuteBooleanExpression(ref int index, string expression)
        {
            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 't' && expression[index + 1] == 'r' && expression[index + 2] == 'u' && expression[index + 3] == 'e')
                    {
                        index += 3;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return true;
                        if (expression.Length == 4)
                            return true;
                    }

                    if (expression[index] == 'f' && expression[index + 1] == 'a' && expression[index + 2] == 'l' && expression[index + 3] == 's' && expression[index + 4] == 'e')
                    {
                        index += 4;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return false;
                        if (expression.Length == 5)
                            return false;
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 't' && expression[index + 1] == 'r' && expression[index + 2] == 'u' && expression[index + 3] == 'e')
                    {
                        index += 3;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return true;
                        if (expression.Length == 4)
                            return true;
                    }

                    if (expression[index] == 'f' && expression[index + 1] == 'a' && expression[index + 2] == 'l' && expression[index + 3] == 's' && expression[index + 4] == 'e')
                    {
                        index += 4;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return false;
                        if (expression.Length == 5)
                            return false;
                    }
                }
            }

            return null;
        }
        public static dynamic ExecuteNumberExpression(ref int index, string expression, InsideQuoteModel quoteModel = null)
        {
            string numberString = string.Empty;
            bool hasBodyNumber = false, hasResidue = false;
            bool isNegative = false;
            double resultDouble;

            if (quoteModel == null)
                quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (!quoteModel.HasQuotes)
                {
                    if (index + 1 == expression.Length)
                    {
                        if (expression[index].Contains(ReservedCollection.Numbers))
                            numberString += expression[index];

                        if (double.TryParse(numberString, out resultDouble))
                            return resultDouble;
                    }

                    if (expression[index].Contains(ReservedCollection.Numbers))
                    {
                        numberString += expression[index];

                        if (!hasResidue)
                            hasBodyNumber = true;
                    }
                    else
                    {
                        if (expression[index] == '+')
                        {
                            if (!string.IsNullOrWhiteSpace(numberString))
                            {
                                if (index + 1 < expression.Length && expression[index + 1] == '+')
                                    MLog.AppErrors.Add(new AppMessage("Incorrect increment declaration.", expression));
                                else
                                    MLog.AppErrors.Add(new AppMessage("Invalid addition operator.", expression));

                                return null;
                            }
                            else
                                return null;
                        }

                        if (expression[index] == '-')
                        {
                            if (string.IsNullOrWhiteSpace(numberString))
                            {
                                if (!isNegative)
                                {
                                    isNegative = true;
                                    numberString += "-";
                                }
                                else
                                {
                                    MLog.AppErrors.Add(new AppMessage("Incorrect decrement declaration.", expression));
                                    return null;
                                }
                            }
                            else
                            {
                                if (index + 1 < expression.Length && expression[index + 1] == '-')
                                    MLog.AppErrors.Add(new AppMessage("Incorrect decrement declaration.", expression));
                                else
                                    MLog.AppErrors.Add(new AppMessage("Invalid subtraction operator.", expression));

                                return null;
                            }
                        }

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
                                                if (double.TryParse(numberString, out resultDouble))
                                                    return resultDouble;

                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (double.TryParse(numberString, out resultDouble))
                                            return resultDouble;

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
                                    if (double.TryParse(numberString, out resultDouble))
                                        return resultDouble;

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
                                    if (double.TryParse(numberString, out resultDouble))
                                        return resultDouble;

                                    break;
                                }
                                else
                                {
                                    if (numberString.Length >= 2)
                                    {
                                        if (double.TryParse(numberString, out resultDouble))
                                            return resultDouble;

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

            return null;
        }
        public static dynamic ExecuteStringExpression(ref int index, string expression, InsideQuoteModel quoteModel = null)
        {
            string resultString = string.Empty;
            bool firstOpen = quoteModel == null ? false : quoteModel.HasQuotes;

            if (quoteModel != null)
            {
                if (expression[index] == quoteModel.Quote)
                    index++;
            }
            else
                quoteModel = new InsideQuoteModel();


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

            return resultString;
        }
        public static dynamic ExecuteArrayExpression(ref int index, string expression, FindContext context)
        {
            List<dynamic> arrayValues = new List<dynamic>();

            string tmpex = null;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes || !expression[index].Contains("[],"))
                    tmpex += expression[index];

                if (!quoteModel.HasQuotes)
                {
                    if (expression[index] == ',')
                    {
                        int pos = 0;

                        arrayValues.Add(MonoInterpreter.ExecuteConditionalExpression(tmpex, ref pos, context));

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
                        arrayValues.Add(MonoInterpreter.ExecuteConditionalExpression(tmpex, ref pos, context));

                        break;
                    }
                }
            }

            if (arrayValues.Count == 1)
                return arrayValues[0];

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
        public static dynamic ExecuteOperatorGetElementExpression(ref int index, string expression, dynamic lastObj, FindContext context)
        {
            if (lastObj != null)
            {
                if (lastObj is MonoType objType)
                {
                    Method overloadMethod = objType.OverloadOperators.FirstOrDefault(sdef => sdef.Name == OperatorCollection.GetElement.Name);

                    if (overloadMethod != null)
                    {
                        var inputs = HelperExpressions.GetObjectMethodParameters(HelperExpressions.GetStringMethodParameters(expression, ref index), context);

                        LocalSpace methodLocalSpace = HelperExpressions.GetMethodLocalSpace(overloadMethod.Parameters, inputs, overloadMethod.FullPath);
                        lastObj = ObjectExpressions.ExecuteMethod(overloadMethod, methodLocalSpace);
                    }
                    else
                        MLog.AppErrors.Add(new AppMessage("No overload method found for GetElement statement.", $"Object {objType.Name}"));
                }
                else if (Extensions.HasEnumerator(lastObj))
                {
                    var inputs = HelperExpressions.GetObjectMethodParameters(HelperExpressions.GetStringMethodParameters(expression, ref index), context);

                    if (inputs.Count == 1 && inputs[0].Value is double numberValue)
                    {
                        lastObj = lastObj[(int)int.Parse(numberValue.ToString().Split(',')[0])];
                    }
                    else
                        MLog.AppErrors.Add(new AppMessage("Invalid array element retrieval options.", expression));
                }
                else
                    MLog.AppErrors.Add(new AppMessage("An object is not an array, structure, or class.", expression));
            }

            return lastObj;
        }
        public static dynamic ExecuteMethodExpression(ref int index, string expression, string methodPath, dynamic lastObj, FindContext context)
        {
            var inputs = HelperExpressions.GetObjectMethodParameters(HelperExpressions.GetStringMethodParameters(expression, ref index), context);

            if (lastObj is Method foundMethod)
            {
                LocalSpace methodLocalSpace = HelperExpressions.GetMethodLocalSpace(foundMethod.Parameters, inputs, foundMethod.FullPath);

                if (foundMethod.IsConstructor)
                    ExecuteConstructor(foundMethod, methodLocalSpace);
                else
                    return ExecuteMethod(foundMethod, methodLocalSpace);
            }

            if (lastObj == null)
            {
                if (inputs.Count == 1)
                {
                    if (!string.IsNullOrEmpty(inputs[0].Name))
                    {
                        if (inputs[0].Name == "obj")
                            return BasicMethods.InvokeMethod(methodPath, inputs[0].Value);
                        else
                            MLog.AppErrors.Add(new AppMessage("Input parameter with this name not found.", $"Method {methodPath}"));
                    }
                    else
                        return BasicMethods.InvokeMethod(methodPath, inputs[0].Value);

                }
                else
                    MLog.AppErrors.Add(new AppMessage("Incorrect parameters method.", $"Method {methodPath}"));
            }
            else
            {
                if (inputs.Count == 0)
                    return BasicMethods.InvokeMethod(methodPath, lastObj);
                else
                    MLog.AppErrors.Add(new AppMessage("Incorrect parameters method.", $"Method {methodPath}"));
            }

            return null;
        }
        public static ExecuteBlockResult ExecuteForExpression(ref int index, string expression, FindContext context)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();


            return executeResult;
        }
        public static ExecuteBlockResult ExecuteForeachExpression(ref int index, string expression, FindContext context)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();


            return executeResult;
        }
        public static ExecuteBlockResult ExecuteWhileExpression(ref int index, string expression, FindContext context)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();


            return executeResult;
        }
        public static ExecuteBlockResult ExecuteDoWhileExpression(ref int index, string expression, FindContext context)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();


            return executeResult;
        }
        public static ExecuteBlockResult ExecuteSwitchExpression(ref int index, string expression, FindContext context)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();


            return executeResult;
        }
        public static ExecuteBlockResult ExecuteIfExpression(ref int index, string expression, FindContext context)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();


            return executeResult;
        }
        public static dynamic ExecuteMethod(Method method, LocalSpace localSpace)
        {
            FindContext context = new FindContext(method) { LocalSpace = localSpace, MonoType = method.ParentObject as MonoType };
            context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

            return MonoInterpreter.ExecuteScript(method.Content, context, ExecuteScriptContextCollection.Method);
        }
        public static dynamic ExecuteConstructor(Method method, LocalSpace localSpace)
        {
            FindContext context = new FindContext(method) { LocalSpace = localSpace };

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

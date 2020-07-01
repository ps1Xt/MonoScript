using MonoScript.Collections;
using MonoScript.Models;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Runtime
{
    public static class ObjectExpressions
    {
        public static dynamic ExecuteStringExpression(ref int index, string expression, InsideQuoteModel quoteModel)
        {
            dynamic lastObj = string.Empty;

            for (; index < expression.Length; index++)
            {
                if (quoteModel.HasQuotes)
                {
                    if (quoteModel.IsOnlyString && expression[index] != quoteModel.Quote)
                        lastObj += expression[index];

                    if (!quoteModel.IsOnlyString)
                    {
                        if (expression[index] == '\\' && index + 1 < expression.Length && expression[index + 1] == quoteModel.Quote)
                        {
                            index++;
                            lastObj += expression[index];
                        }
                        else
                        {
                            if (expression[index] == quoteModel.Quote)
                            {
                                if ((index - 1 >= 0 && expression[index - 1] == '\\') || index == 0)
                                    lastObj += expression[index];
                            }
                            else
                                lastObj += expression[index];
                        } //to:do доделать
                    }
                }
                else
                    break;
            }

            return lastObj;
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

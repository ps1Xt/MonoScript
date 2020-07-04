using MonoScript.Analytics;
using MonoScript.Libraries;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoScript.Collections;
using MonoScript.Models.Interpreter;
using MonoScript.Models.Script;
using MonoScript.Models.Contexts;

namespace MonoScript.Runtime
{
    public class MonoInterpreter
    {
        Application app;
        public MonoInterpreter(Application app) => this.app = app;

        public Status RunApplication()
        {
            Status buildStatus = Build();

            if (buildStatus.Success)
                return Run();

            return buildStatus;
        }
        public Status Build()
        {
            Parser.ParseScriptFile(app);
            Application.InitializePublicModifiers(app);
            Application.InitializeConstructors(app);
            Application.InitializeInheritance(app);
            Application.InitializeFields(app);
            Analyzer.AnalyzeAll(app);

            var test = app.MainScript.Classes[0].Fields[0].Value;

            if (MLog.AppErrors.Count > 0)
                return Status.ErrorBuild;

            return Status.SuccessBuild;
        }
        public Status Run()
        {
            FindContext findContext = new FindContext(app.MainScript.Root.Method)
            {
                LocalSpace = new LocalSpace(null),
                MonoType = app.MainScript.Root.Class,
                ScriptFile = app.MainScript
            };

            ExecuteScript(app.MainScript.Root.Method.Content, findContext, ExecuteScriptContextCollection.Method);

            if (MLog.AppErrors.Count > 0)
                return Status.ErrorCompleted;

            return Status.SuccessfullyCompleted;
        }

        public static dynamic ExecuteScript(string script, FindContext context, ExecuteScriptContextCollection executeScriptContext)
        {
            if (script == null)
                script = string.Empty;

            string leftex = string.Empty;
            string scriptex = string.Empty;
            bool hasOperator = true;

            void Refresh()
            {
                hasOperator = true;
                scriptex = string.Empty;
                leftex = string.Empty;
            }

            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (int i = 0; i < script.Length; i++)
            {
                Extensions.IsOpenQuote(script, i, ref quoteModel);

                if (quoteModel.HasQuotes || !script[i].Contains("\n;"))
                    scriptex += script[i];

                if ((!quoteModel.HasQuotes && script[i].Contains("\n;")) || i + 1 == script.Length)
                {
                    if (quoteModel.HasQuotes)
                        MLog.AppErrors.Add(new AppMessage("The string was not closed.", script));

                    if (!hasOperator)
                    {
                        int index = 0;
                        object newObj = ExecuteConditionalExpression(scriptex, context, ref index);
                    }

                    Refresh();
                }
                

                if (quoteModel.HasQuotes && hasOperator)
                    hasOperator = false;

                if (!quoteModel.HasQuotes && hasOperator)
                {
                    if (script[i].Contains(ReservedCollection.Alphabet))
                        leftex += script[i];
                    else if (leftex == "var")
                    {
                        if (script[i] == '(')
                            leftex = string.Empty;
                    }
                    else if (!script[i].Contains(" ("))
                        leftex = string.Empty;
                }

                if (!quoteModel.HasQuotes && hasOperator)
                {
                    if (script[i].Contains("( ") && leftex != string.Empty)
                    {
                        if (leftex == "var")
                        {
                            var newObject = ObjectExpressions.ExecuteVarExpression(ref i, script, context);

                            Refresh();
                        }

                        else if (leftex == "for")
                        {
                            var executeResult = ObjectExpressions.ExecuteForExpression(ref i, script, context);

                            if (executeResult.CanExecuteNextResult(executeScriptContext))
                                return executeResult.ExecuteNextResult(executeScriptContext);

                            Refresh();
                        }

                        else if (leftex == "foreach")
                        {
                            var executeResult = ObjectExpressions.ExecuteForeachExpression(ref i, script, context);

                            if (executeResult.CanExecuteNextResult(executeScriptContext))
                                return executeResult.ExecuteNextResult(executeScriptContext);

                            Refresh();
                        }

                        else if (leftex == "while")
                        {
                            var executeResult = ObjectExpressions.ExecuteWhileExpression(ref i, script, context);

                            if (executeResult.CanExecuteNextResult(executeScriptContext))
                                return executeResult.ExecuteNextResult(executeScriptContext);

                            Refresh();
                        }

                        else if (leftex == "do")
                        {
                            var executeResult = ObjectExpressions.ExecuteDoWhileExpression(ref i, script, context);

                            if (executeResult.CanExecuteNextResult(executeScriptContext))
                                return executeResult.ExecuteNextResult(executeScriptContext);

                            Refresh();
                        }

                        else if (leftex == "switch")
                        {
                            var executeResult = ObjectExpressions.ExecuteSwitchExpression(ref i, script, context);

                            if (executeResult.CanExecuteNextResult(executeScriptContext))
                                return executeResult.ExecuteNextResult(executeScriptContext);

                            Refresh();
                        }

                        else if (leftex == "if")
                        {
                            var executeResult = ObjectExpressions.ExecuteIfExpression(ref i, script, context);

                            if (executeResult.CanExecuteNextResult(executeScriptContext))
                                return executeResult.ExecuteNextResult(executeScriptContext);

                            Refresh();
                        }

                        else
                            hasOperator = false;
                    }
                }
            }

            return null;
        }
        public static dynamic ExecuteExpression(string expression, Field destObj, FindContext context, ExecuteContextCollection executeContext)
        {
            if (expression == null)
                expression = string.Empty;

            int index = 0;

            if (destObj.Modifiers.Contains("const"))
            {
                if (executeContext == ExecuteContextCollection.Const)
                    return ExecuteConditionalExpression(expression, context, ref index);

                MLog.AppErrors.Add(new AppMessage("Operations with a constant are allowed only during the declaration.", expression));
            }
            else if (destObj.Modifiers.Contains("readonly"))
            {
                if (executeContext == ExecuteContextCollection.Readonly)
                    return ExecuteConditionalExpression(expression, context, ref index);

                MLog.AppErrors.Add(new AppMessage("Readonly operations are permitted only when declared or in the constructor body.", expression));
            }
            else
                 return ExecuteConditionalExpression(expression, context, ref index);

            return null;
        }
        public static dynamic ExecuteConditionalExpression(string expression, FindContext context, ref int pos)
        {
            if (expression == null)
                expression = string.Empty;

            string tmpex = string.Empty;
            MethodExpressionModel methodExpression = new MethodExpressionModel();
            SquareBracketExpressionModel bracketExpression = new SquareBracketExpressionModel();
            InsideQuoteModel insideQuote = new InsideQuoteModel();

            for (; pos < expression.Length; pos++)
            {
                Extensions.IsOpenQuote(expression, pos, ref insideQuote);

                if (insideQuote.HasQuotes || methodExpression.HasOpenBracket || bracketExpression.HasOpenBracket || !expression[pos].Contains("()&|"))
                    tmpex += expression[pos];

                methodExpression.Read(expression, pos);

                if (!insideQuote.HasQuotes)
                {
                    if (!methodExpression.HasOpenBracket && !bracketExpression.HasOpenBracket)
                    {
                        if (expression[pos] == '[')
                        {
                            bracketExpression.OpenBracketCount++;
                            continue;
                        }

                        if (expression[pos] == '(')
                        {
                            if (methodExpression.MethodName != null)
                            {
                                methodExpression.OpenBracketCount++;
                                tmpex += expression[pos];
                                continue;
                            }
                            
                            pos++;
                            var executeResult = ExecuteConditionalExpression(expression, context, ref pos);

                            
                        }

                        if (expression[pos] == '&')
                        {
                            if (pos + 1 >= expression.Length || expression[pos + 1] != '&')
                                MLog.AppErrors.Add(new AppMessage("Unknown operator. &", expression));

                            pos += 2;


                        }

                        if (expression[pos] == '|')
                        {
                            if (pos + 1 >= expression.Length || expression[pos + 1] != '|')
                                MLog.AppErrors.Add(new AppMessage("Unknown operator. |", expression));

                            pos += 2;




                        }

                        if (expression[pos] == ')')
                        {
                            pos++;
                            break;
                        }
                    }

                    if (pos < expression.Length)
                    {
                        if (methodExpression.HasOpenBracket)
                        {
                            if (expression[pos] == '(')
                                methodExpression.OpenBracketCount++;

                            if (expression[pos] == ')')
                            {
                                methodExpression.OpenBracketCount--;
                                continue;
                            }
                        }

                        if (bracketExpression.HasOpenBracket)
                        {
                            if (expression[pos] == '[')
                                bracketExpression.OpenBracketCount++;

                            if (expression[pos] == ']')
                            {
                                bracketExpression.OpenBracketCount--;
                                continue;
                            }
                        }
                    }
                }
            }

            if (methodExpression.OpenBracketCount > 0)
                MLog.AppErrors.Add(new AppMessage("Missing closing bracket. ')'", expression));

            return ExecuteEqualityExpression(tmpex, context);
        }
        public static dynamic ExecuteEqualityExpression(string expression, FindContext context)
        {
            if (expression == null)
                expression = string.Empty;

            dynamic result = null;
            MethodExpressionModel methodExpression = new MethodExpressionModel();
            SquareBracketExpressionModel bracketExpression = new SquareBracketExpressionModel();
            InsideQuoteModel insideQuote = new InsideQuoteModel();

            for (int i = 0; i < expression.Length; i++)
            {
                Extensions.IsOpenQuote(expression, i, ref insideQuote);

                methodExpression.Read(expression, i);

                if (!insideQuote.HasQuotes)
                {
                    if (!methodExpression.HasOpenBracket && !bracketExpression.HasOpenBracket)
                    {
                        if (expression[i] == '[')
                        {
                            bracketExpression.OpenBracketCount++;
                            continue;
                        }

                        if (expression[i] == '(')
                        {
                            if (methodExpression.MethodName != null)
                            {
                                methodExpression.OpenBracketCount++;
                                continue;
                            }
                        }

                        if (expression[i] == '<')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ExecuteArithmeticExpression(expression.Substring(0, i), context);
                                dynamic rightObj = ExecuteArithmeticExpression(expression.Substring(i + 2), context);

                                result = ExecuteEqualityObjects(leftObj, rightObj, "<=", expression, context);
                                break;
                            }
                            else
                            {
                                dynamic leftObj = ExecuteArithmeticExpression(expression.Substring(0, i), context);
                                dynamic rightObj = ExecuteArithmeticExpression(expression.Substring(i + 1), context);

                                result = ExecuteEqualityObjects(leftObj, rightObj, "<", expression, context);
                                break;
                            }
                        }

                        if (expression[i] == '>')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ExecuteArithmeticExpression(expression.Substring(0, i), context);
                                dynamic rightObj = ExecuteArithmeticExpression(expression.Substring(i + 2), context);

                                result = ExecuteEqualityObjects(leftObj, rightObj, ">=", expression, context);
                                break;
                            }
                            else
                            {
                                dynamic leftObj = ExecuteArithmeticExpression(expression.Substring(0, i), context);
                                dynamic rightObj = ExecuteArithmeticExpression(expression.Substring(i + 1), context);

                                result = ExecuteEqualityObjects(leftObj, rightObj, ">", expression, context);
                                break;
                            }
                        }

                        if (expression[i] == '=')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ExecuteArithmeticExpression(expression.Substring(0, i), context);
                                dynamic rightObj = ExecuteArithmeticExpression(expression.Substring(i + 2), context);

                                result = ExecuteEqualityObjects(leftObj, rightObj, "==", expression, context);
                                break;
                            }
                        }

                        if (expression[i] == '!')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ExecuteArithmeticExpression(expression.Substring(0, i), context);
                                dynamic rightObj = ExecuteArithmeticExpression(expression.Substring(i + 2), context);

                                result = ExecuteEqualityObjects(leftObj, rightObj, "!=", expression, context);
                                break;
                            }
                        }
                    }

                    if (i < expression.Length)
                    {
                        if (methodExpression.HasOpenBracket)
                        {
                            if (expression[i] == '(')
                                methodExpression.OpenBracketCount++;

                            if (expression[i] == ')')
                                methodExpression.OpenBracketCount--;
                        }

                        if (bracketExpression.HasOpenBracket)
                        {
                            if (expression[i] == '[')
                                bracketExpression.OpenBracketCount++;

                            if (expression[i] == ']')
                                bracketExpression.OpenBracketCount--;
                        }
                    }
                }
            }

            if (result == null)
                result = ExecuteArithmeticExpression(expression, context);

            return result;
        }
        public static dynamic ExecuteEqualityObjects(dynamic leftObj, dynamic rightObj, string equalitySign, string errorSource, FindContext context)
        {
            if (leftObj == null || rightObj == null)
            {
                MLog.AppErrors.Add(new AppMessage("You cannot use the comparison operator in a null object.", errorSource));
                return null;
            }

            MonoType leftType = leftObj as MonoType;
            MonoType rightType = rightObj as MonoType;

            if (leftType != null && rightType != null)
            {
                if (leftType.Path != rightType.Path)
                {
                    MLog.AppErrors.Add(new AppMessage("Cannot perform comparison operations with different types.", errorSource));
                    return null;
                }

                Operator oper = OperatorCollection.GetOperatorBySign(equalitySign);
                Method overloadMethod = leftType.OverloadOperators.FirstOrDefault(sdef => sdef.Name == oper.Name && sdef.Parameters.Count == oper.Parameters);

                if (overloadMethod != null)
                {
                    LocalSpace localSpace = new LocalSpace(null);
                    localSpace.Fields.Add(new Field(overloadMethod.Parameters[0].Name, null) { Value = leftObj });
                    localSpace.Fields.Add(new Field(overloadMethod.Parameters[1].Name, null) { Value = rightObj });

                    FindContext overloadMethodFindContext = new FindContext(overloadMethod);
                    overloadMethodFindContext.LocalSpace = localSpace;
                    overloadMethodFindContext.MonoType = overloadMethod.ParentObject as MonoType;
                    overloadMethodFindContext.ScriptFile = overloadMethodFindContext?.MonoType.ParentObject as ScriptFile;

                    if (!context.IsStaticObject)
                        return ExecuteScript(overloadMethod.Content, overloadMethodFindContext, ExecuteScriptContextCollection.Method);

                    MLog.AppErrors.Add(new AppMessage("Operators cannot be overridden in static classes.", errorSource));
                }
                else
                    MLog.AppErrors.Add(new AppMessage("No special comparison function found for this type. NotEqual.", string.Format("Path: {0}", leftType.Path)));

                return null;
            }

            if (leftObj is EnumValue && rightObj is EnumValue)
            {
                var leftEnumValue = leftObj as EnumValue;
                var rightEnumValue = rightObj as EnumValue;

                if (equalitySign == "==" && leftEnumValue.EnumPath == rightEnumValue.EnumPath)
                    return leftEnumValue.Value == rightEnumValue.Value;

                if (equalitySign == "!=" && leftEnumValue.EnumPath == rightEnumValue.EnumPath)
                    return leftEnumValue.Value == rightEnumValue.Value;

                MLog.AppErrors.Add(new AppMessage("Cannot perform comparison operations with different types.", errorSource));

                return null;
            }

            try
            {
                if (equalitySign == ">")
                    return leftObj > rightObj;

                if (equalitySign == ">=")
                    return leftObj >= rightObj;

                if (equalitySign == "<")
                    return leftObj < rightObj;

                if (equalitySign == "<=")
                    return leftObj <= rightObj;

                if (equalitySign == "==")
                    return leftObj == rightObj;

                if (equalitySign == "!=")
                    return leftObj != rightObj;
            }
            catch (Exception) { MLog.AppErrors.Add(new AppMessage($"Incorrect data comparison. Operator: {equalitySign}", errorSource)); }

            return null;
        }
        public static dynamic ExecuteArithmeticExpression(string expression, FindContext context)
        {
            ExecuteArithmeticObjects(ref expression, context, "/%");
            ExecuteArithmeticObjects(ref expression, context, "*");
            ExecuteArithmeticObjects(ref expression, context, "-");
            ExecuteArithmeticObjects(ref expression, context, "+");

            return ExecuteArgumentExpression(expression, context);
        }
        public static void ExecuteArithmeticObjects(ref string expression, FindContext context, string arithSign)
        {
            if (expression == null)
                expression = string.Empty;

            string leftex = null;
            string ariths = ReservedCollection.NumberOperations;
            int leng = expression.Length, leftexStartIndex = -1;
            MethodExpressionModel methodExpression = new MethodExpressionModel();
            SquareBracketExpressionModel bracketExpression = new SquareBracketExpressionModel();
            InsideQuoteModel insideQuote = new InsideQuoteModel();

            for (int i = 0; i < leng; i++)
            {
                Extensions.IsOpenQuote(expression, i, ref insideQuote);

                methodExpression.Read(expression, i);

                if (!expression[i].Contains(ariths))
                {
                    leftex += expression[i];

                    if (leftexStartIndex == -1)
                        leftexStartIndex = i;
                }
                else
                {
                    if (i + 1 < expression.Length && expression[i] == expression[i + 1])
                    {
                        i++;
                        leftex += new string(expression[i], 2);

                        if (leftexStartIndex == -1)
                            leftexStartIndex = i;

                        continue;
                    }
                }

                if (!insideQuote.HasQuotes)
                {
                    if (!methodExpression.HasOpenBracket && !bracketExpression.HasOpenBracket)
                    {
                        string rightex = null;

                        if (expression[i] == '[')
                            bracketExpression.OpenBracketCount++;

                        if (expression[i] == '(')
                        {
                            if (methodExpression.MethodName != null)
                                methodExpression.OpenBracketCount++;
                        }

                        if (arithSign.Contains(expression[i]))
                        {
                            if (i + 1 < expression.Length)
                            {
                                for (int index = i + 1; index < leng; index++)
                                {
                                    if (!expression[index].Contains(ariths))
                                        rightex += expression[index];
                                    else
                                    {
                                        if (index + 1 < expression.Length && expression[index] == expression[index + 1])
                                        {
                                            i++;
                                            rightex += new string(expression[index], 2);
                                        }
                                        else
                                            MLog.AppErrors.Add(new AppMessage("Invalid operator declaration. " + expression[index], expression));
                                    }
                                }

                                if (leftex == null) MLog.AppErrors.Add(new AppMessage($"Missing left argument in calculation operation. {expression[i]}", expression));
                                if (rightex == null) MLog.AppErrors.Add(new AppMessage($"Missing right argument in calculation operation. {expression[i]}", expression));

                                dynamic leftObj = ExecuteArgumentExpression(leftex, context);
                                dynamic rightObj = ExecuteArgumentExpression(rightex, context);

                                if (leftObj != null && rightObj != null)
                                {
                                    MonoType leftType = leftObj as MonoType;
                                    MonoType rightType = rightObj as MonoType;

                                    if (leftType != null && rightType != null)
                                    {
                                        if (leftType.Path != rightType.Path)
                                        {
                                            MLog.AppErrors.Add(new AppMessage("Mathematical operations are not possible with objects of different types.", expression));
                                            return;
                                        }

                                        Operator oper = OperatorCollection.GetOperatorBySign(expression[i].ToString());
                                        Method overloadMethod = leftType.OverloadOperators.FirstOrDefault(sdef => sdef.Name == oper.Name && sdef.Parameters.Count == oper.Parameters);

                                        if (overloadMethod != null)
                                        {
                                            LocalSpace overloadLocalSpace = new LocalSpace(null);
                                            overloadLocalSpace.Fields.Add(new Field(overloadMethod.Parameters[0].Name, null) { Value = leftObj });
                                            overloadLocalSpace.Fields.Add(new Field(overloadMethod.Parameters[1].Name, null) { Value = rightObj });

                                            if (!context.IsStaticObject)
                                            {
                                                FindContext overloadMethodFindContext = new FindContext(overloadMethod);
                                                overloadMethodFindContext.LocalSpace = overloadLocalSpace;
                                                overloadMethodFindContext.MonoType = overloadMethod.ParentObject as MonoType;
                                                overloadMethodFindContext.ScriptFile = overloadMethodFindContext?.MonoType.ParentObject as ScriptFile;

                                                dynamic result = ExecuteScript(overloadMethod.Content, overloadMethodFindContext, ExecuteScriptContextCollection.Method);
                                                string tmpname = $"monosys_{context.LocalSpace.FreeMonoSysValue}";

                                                context.LocalSpace.Fields.Add(new Field(tmpname, null) { Value = result });
                                                expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1).Insert(leftexStartIndex, tmpname);
                                            }
                                            else MLog.AppErrors.Add(new AppMessage("Operators cannot be overridden in static classes.", $"Path {leftType.Path}"));
                                        }
                                        else MLog.AppErrors.Add(new AppMessage("No special arithmetic function found for this type.", $"Path {leftType.Path}"));

                                        leng = expression.Length;
                                        i = 0; //to:do -1;

                                        continue;
                                    }

                                    try
                                    {
                                        if (expression[i] == '/')
                                            expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1).Insert(leftexStartIndex, (leftObj / rightObj).ToString().Replace(",", "."));

                                        if (expression[i] == '%')
                                            expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1).Insert(leftexStartIndex, (leftObj % rightObj).ToString().Replace(",", "."));

                                        if (expression[i] == '*')
                                            expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1).Insert(leftexStartIndex, (leftObj * rightObj).ToString().Replace(",", "."));

                                        if (expression[i] == '-')
                                            expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1).Insert(leftexStartIndex, (leftObj - rightObj).ToString().Replace(",", "."));

                                        if (expression[i] == '+')
                                            expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1).Insert(leftexStartIndex, (leftObj + rightObj).ToString().Replace(",", "."));

                                        leng = expression.Length;
                                        i = 0; //to:do -1;

                                        continue;
                                    }
                                    catch { MLog.AppErrors.Add(new AppMessage($"Incorrect arithmetic operation. Operation: {expression[i]}", expression)); }
                                }

                                MLog.AppErrors.Add(new AppMessage("Arithmetic operations are not possible with a null object.", expression));
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("Incorrect expression of an arithmetic operation.", expression));

                            return;
                        }
                    }

                    if (i < expression.Length)
                    {
                        if (methodExpression.HasOpenBracket)
                        {
                            if (expression[i] == '(')
                                methodExpression.OpenBracketCount++;

                            if (expression[i] == ')')
                                methodExpression.OpenBracketCount--;
                        }

                        if (bracketExpression.HasOpenBracket)
                        {
                            if (expression[i] == '[')
                                bracketExpression.OpenBracketCount++;

                            if (expression[i] == ']')
                                bracketExpression.OpenBracketCount--;
                        }
                    }
                }
            }
        }
        public static dynamic ExecuteArgumentExpression(string expression, FindContext context)
        {
            if (expression == null)
                expression = string.Empty;

            dynamic lastObj = null;
            string objectName = string.Empty;
            bool canMakeArray = true, canNext = false;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (int i = 0; i < expression.Length; i++)
            {
                Extensions.IsOpenQuote(expression, i, ref quoteModel);

                if (canMakeArray && !expression[i].Contains(" ["))
                    canMakeArray = false;

                if (!expression[i].Contains("[("))
                    objectName += expression[i];

                if (quoteModel.HasQuotes)
                {
                    if (lastObj == null)
                    {
                        lastObj = string.Empty;
                        objectName = string.Empty;

                        lastObj += ObjectExpressions.ExecuteStringExpression(ref i, expression, quoteModel);
                        continue;
                    }
                }

                if (!quoteModel.HasQuotes)
                {
                    if (lastObj == null && expression[i] != ' ' &&  (i == 0 || (i - 1 >= 0 && !expression[i - 1].Contains(ReservedCollection.AllowedNames))))
                    {
                        if (expression[i].Contains(ReservedCollection.Numbers) || (expression[i] == '.' && i + 1 < expression.Length && expression[i + 1].Contains(ReservedCollection.Numbers)))
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteNumberExpression(ref i, expression, quoteModel);

                            if (result is double)
                            {
                                lastObj = result;
                                objectName = string.Empty;
                            }
                            else
                                i = saveIndex;
                        }

                        if (i + 3 < expression.Length && expression[i] == 't' && expression[i + 1] == 'r' || (expression[i] == 'f' && expression[i + 1] == 'a'))
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteBooleanExpression(ref i, expression);

                            if (result is bool)
                            {
                                if (i - 1 >= 0 && expression[i - 1] == '!')
                                    result = !result;

                                lastObj = result;
                                objectName = string.Empty;
                                continue;
                            }
                            else
                                i = saveIndex;
                        }

                        if (i + 3 < expression.Length && expression[i] == 'n' && expression[i + 1] == 'u' && expression[i + 2] == 'l')
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteNullExpression(ref i, expression);

                            if (result is null)
                            {
                                lastObj = result;
                                objectName = string.Empty;
                                continue;
                            }
                            else
                                i = saveIndex;
                        }

                        if (i + 3 < expression.Length && expression[i] == 't' && expression[i + 1] == 'h' && expression[i + 2] == 'i')
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteThisExpression(ref i, expression, context);

                            if (result is MonoType)
                            {
                                lastObj = result;
                                objectName = string.Empty;
                                continue;
                            }
                            else if (result is null)
                                i = saveIndex;
                        }
                    }

                    if (canMakeArray && expression[i] == '[')
                    {
                        canMakeArray = false;
                        lastObj = ObjectExpressions.ExecuteArrayExpression(ref i, expression, context);
                        continue;
                    }

                    if (!canMakeArray && expression[i] != ' ')
                    {
                        dynamic FindInLastObjectField(string objName, dynamic destObj)
                        {
                            if (destObj is MonoType objType)
                            {
                                Field foundField = objType.Fields.FirstOrDefault(x => x.Name == objName);

                                if (foundField != null)
                                {
                                    bool? allowedAccess = foundField.Modifiers.Contains("public");

                                    if (!allowedAccess.HasValue && foundField.Modifiers.Contains("private"))
                                        allowedAccess = objType.FullPath == context.MonoType?.FullPath;

                                    if (!allowedAccess.HasValue && foundField.Modifiers.Contains("protected"))
                                        allowedAccess = (objType as Class)?.ContainsParent(context.MonoType as Class);

                                    if (allowedAccess.HasValue && allowedAccess.Value)
                                        return lastObj;

                                    MLog.AppErrors.Add(new AppMessage("Object is below access level.", $"Object {objName}"));
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("An object with this name was not found in the class or structure.", $"Object {objName}"));
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("An object is not a class or structure.", $"Object {lastObj}"));

                            return null;
                        }
                        dynamic FindInLastObjectMethod(string methodName, dynamic destObj)
                        {
                            if (destObj is MonoType objType)
                            {
                                Method foundMethod = objType.Methods.FirstOrDefault(x => x.Name == methodName);

                                if (foundMethod != null)
                                {
                                    bool? allowedAccess = foundMethod.Modifiers.Contains("public");

                                    if (!allowedAccess.HasValue && foundMethod.Modifiers.Contains("private"))
                                        allowedAccess = objType.FullPath == context.MonoType?.FullPath;

                                    if (!allowedAccess.HasValue && foundMethod.Modifiers.Contains("protected"))
                                        allowedAccess = (objType as Class)?.ContainsParent(context.MonoType as Class);

                                    if (allowedAccess.HasValue && allowedAccess.Value)
                                        return lastObj;

                                    MLog.AppErrors.Add(new AppMessage("Object is below access level.", $"Method {methodName}"));
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("An object with this name was not found in the class or structure.", $"Method {methodName}"));
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("An object is not a class or structure.", $"Method {lastObj}"));

                            return null;
                        }
                        dynamic ObjectFromField(dynamic lastObj)
                        {
                            if (lastObj is Field lastObjField)
                                lastObj = lastObjField.Value;

                            return lastObj;
                        }

                        objectName = IPath.NormalizeWithTrim(objectName);

                        if (i + 1 == expression.Length && !string.IsNullOrWhiteSpace(objectName))
                        {
                            if (lastObj == null)
                                lastObj = ObjectFromField(Finder.FindObject(objectName, context));
                            else
                                lastObj = FindInLastObjectField(objectName, lastObj);
                        }

                        if (expression[i] == '[')
                        {
                            if (!string.IsNullOrWhiteSpace(objectName))
                            {
                                if (lastObj != null)
                                    lastObj = FindInLastObjectField(objectName, lastObj);
                                else
                                    lastObj = ObjectFromField(Finder.FindObject(objectName, context, Finder.FindOption.NoStatic));
                            }

                            lastObj = ObjectExpressions.ExecuteOperatorGetElementExpression(ref i, expression, lastObj, context);
                            objectName = string.Empty;
                        }

                        if (expression[i] == '(')
                        {
                            if (!string.IsNullOrWhiteSpace(objectName))
                            {
                                if (lastObj != null)
                                    lastObj = FindInLastObjectMethod(objectName, lastObj);
                                else
                                    lastObj = ObjectFromField(Finder.FindObject(objectName, context));
                            }

                            //добавить инкремент декремент, использовать внутри этого метода Field.Value

                            lastObj = ObjectExpressions.ExecuteMethodExpression(ref i, expression, objectName, lastObj, context);
                            objectName = string.Empty;
                        }

                        if (expression[i] == '.')
                        {
                            if (!string.IsNullOrWhiteSpace(objectName))
                            {
                                if (lastObj == null)
                                {
                                    lastObj = Finder.FindObject(objectName, context);

                                    if (lastObj != null)
                                        objectName = string.Empty;
                                }
                                else
                                {
                                    if (!canNext)
                                        canNext = true;
                                    else
                                        MLog.AppErrors.Add(new AppMessage("Incorrect dot declaration.", expression));

                                    lastObj = FindInLastObjectField(objectName, lastObj);
                                    objectName = string.Empty;
                                }
                            }
                        }

                        if (expression[i] == '+')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '+')
                                lastObj = ObjectExpressions.ExecuteIncrementExpression(ref i, expression, objectName, lastObj, context);
                            else
                                MLog.AppErrors.Add(new AppMessage("Incorrect increment declaration.", expression));

                            objectName = string.Empty;
                        }

                        if (expression[i] == '-')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '-')
                                lastObj = ObjectExpressions.ExecuteDecrementExpression(ref i, expression, objectName, lastObj, context);
                            else
                                MLog.AppErrors.Add(new AppMessage("Incorrect decrement declaration.", expression));

                            objectName = string.Empty;
                        }
                    }
                }
            }

            if (lastObj is Field field)
                lastObj = field.Value;

            return lastObj;
        }
    }
}

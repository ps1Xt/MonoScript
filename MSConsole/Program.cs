using MonoScript.Analytics;
using MonoScript.Models.Contexts;
using MonoScript.Runtime;
using System;

namespace MSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int index = 0;
            var result = MonoInterpreter.ExecuteConditionalExpression("((100  +   200   *   (   10   -   50)) /  20)", ref index, new FindContext(null));
            //var result = MonoInterpreter.ExecuteConditionalExpression("(true || false || true || true).ToString()", ref index, new FindContext(null));
            //var result = MonoInterpreter.ExecuteArithmeticExpression("(true && true) || (true && true && true)", new FindContext(null));

            var errors = MLog.AppErrors;
        }
    }
}

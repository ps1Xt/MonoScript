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
            //var result = MonoInterpreter.ExecuteConditionalExpression("!(false && true || true) && true", ref index, new FindContext(null));
            var result = MonoInterpreter.ExecuteConditionalExpression("!(!user.file && true)", ref index, new FindContext(null));

            var errors = MLog.AppErrors;
        }
    }
}

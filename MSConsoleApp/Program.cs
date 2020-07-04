using MonoScript.Analytics;
using MonoScript.Libraries.IO;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Runtime;
using System;

namespace MSConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Application application = new Application();
            //application.RunApplication(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\test.txt");

            var result = MonoInterpreter.ExecuteEqualityExpression("((2)+200<700).ToString()", new FindContext(null));
            //var result = MonoInterpreter.ExecuteEqualityExpression("100+100", new FindContext(null));
            var errors = MLog.AppErrors;
        }
    }
}

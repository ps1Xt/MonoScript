using MonoScript.Libraries.IO;
using MonoScript.Models.Analytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Collections
{
    public class AppMessageCollection : List<AppMessage>
    {
        public new void Add(AppMessage message)
        {
            base.Add(message);

            MonoConsole.WriteAppMessage(message);
        }
    }
}

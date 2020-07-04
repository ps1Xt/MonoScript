using MonoScript.Models;
using MonoScript.Runtime;
using MonoScript.Script;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonoScript.Models.Analytics;
using MonoScript.Collections;

namespace MonoScript.Analytics
{
    public static class MLog
    {
        public static AppMessageCollection AppErrors { get; } = new AppMessageCollection();
        public static AppMessageCollection AppWarnings { get; } = new AppMessageCollection();
    }
}

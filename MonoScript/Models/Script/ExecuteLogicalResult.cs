using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Script
{
    public class ExecuteLogicalResult
    {
        public bool IsContinue { get; set; }
        public bool IsBreak { get; set; }
        public bool IsReturn { get; set; }
        public bool IsNone { get; set; }
        public dynamic ReturnValue { get; set; }

        public static ExecuteLogicalResult ContinueResult { get; } = new ExecuteLogicalResult() { IsContinue = true };
        public static ExecuteLogicalResult BreakResult { get; } = new ExecuteLogicalResult() { IsBreak = true };
        public static ExecuteLogicalResult NoneResult { get; } = new ExecuteLogicalResult() { IsNone = true };
        public static ExecuteLogicalResult ReturnResult(dynamic returnValue)
        {
            return new ExecuteLogicalResult() { IsReturn = true, ReturnValue = returnValue };
        }
    }
}

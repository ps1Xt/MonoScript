using MonoScript.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Script
{
    public class ExecuteResult
    {
        public dynamic ObjectResult { get; set; }
        public ExecuteResultCollection ResultType { get; set; }
        public int Count { get; set; }

        public ExecuteResult ExecuteNextResult(ExecuteScriptContextCollection executeScriptContext)
        {
            if ((executeScriptContext == ExecuteScriptContextCollection.CycleOrSwitch && ResultType == ExecuteResultCollection.Break) || (executeScriptContext == ExecuteScriptContextCollection.IfElseSwitch && ResultType == ExecuteResultCollection.Quit))
                Count--;

            return this;
        }

        public bool CanExecuteNextResult(ExecuteScriptContextCollection executeScriptContext)
        {
            if (executeScriptContext != ExecuteScriptContextCollection.Method)
                return false;

            if (ResultType == ExecuteResultCollection.Value)
                return false;

            return Count > 0 ? true : false;
        }
    }
}

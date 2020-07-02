using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Script.Elements
{
    public class EnumValue : MonoObject, IObjectParent
    {
        public object ParentObject { get; }
        public string EnumPath { get; set; }
        public int Value { get; set; }
        public EnumValue(string fullpath, string enumPath, object parentObject)
        {
            FullPath = fullpath;
            EnumPath = enumPath;
            ParentObject = parentObject;
        }
        public EnumValue CloseObject() => (EnumValue)MemberwiseClone();
    }
}

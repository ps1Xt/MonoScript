using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Types;
using System.Collections.Generic;
using System.Linq;

namespace MonoScript.Script.Elements
{
    public class Method : MonoObject, IModifier, IObjectParent
    {
        public object ParentObject { get; }
        public string Content { get; set; }
        public bool IsConstructor
        {
            get
            {
                if (ParentObject != null && (ParentObject as MonoType)?.Name == Name)
                    return true;

                return false;
            }
        }
        public List<Field> Parameters { get; set; } = new List<Field>();
        public List<string> Modifiers { get; set; } = new List<string>();
        public List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>()
        {
            new string[] { "public", "static" },
            new string[] { "private", "static" },
            new string[] { "protected", "static" },
        };
        public Method(string fullpath, object parentObject, params string[] modifiers)
        {
            FullPath = fullpath;
            ParentObject = parentObject;
            AddModifiers(modifiers.ToList());
        }
        public Method CloneObject()
        {
            Method method = (Method)MemberwiseClone();
            method.Parameters = Parameters.ToList();
            method.Modifiers = Modifiers.ToList();
            method.AllowedModifierGroups = AllowedModifierGroups.ToList();

            return method;
        }

        public void AddModifiers(List<string> modifiers)
        {
            Modifiers.AddRange(modifiers);

            if (!Modifiers.Contains("public", "protected", "private"))
                Modifiers.Add("public");

            bool hasError = false;

            foreach (var group in AllowedModifierGroups)
            {
                int count = 0;
                foreach (var modifier in modifiers)
                {
                    if (!hasError && !group.Contains(modifier))
                        hasError = true;

                    if (group.Contains(modifier))
                        count++;
                }

                if (!hasError || count == modifiers.Count)
                    return;
            }

            if (hasError)
                MLog.AppErrors.Add(new AppMessage("Invalid modifier group.", $"Path {FullPath}"));
        }

        public static List<Field> GetParameters(string exfields, string methodPath, object parentObject)
        {
            if (exfields == null)
                return new List<Field>();

            List<Field> parameters = new List<Field>();
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            string name = null, value = null;

            for (int i = 0; i < exfields.Length; i++)
            {
                Extensions.IsOpenQuote(exfields, i, ref quoteModel);

                if (value == null && !exfields[i].Contains("=,"))
                    name += exfields[i];

                if (value != null && (quoteModel.HasQuotes || (!quoteModel.HasQuotes && !exfields[i].Contains("=,"))))
                    value += exfields[i];

                if (!quoteModel.HasQuotes)
                {
                    if (exfields[i] == '=')
                        value = "";

                    if (exfields[i] == ',')
                    {
                        if (value == "")
                            MLog.AppErrors.Add(new AppMessage("The field does not have a value.", $"Method: {methodPath}"));

                        parameters.Add(new Field(IPath.CombinePath(name.Trim(' '), methodPath), parentObject) { Value = value });

                        name = null;
                        value = null;
                    }
                }
            }

            if (name != null)
            {
                if (value == "")
                    MLog.AppErrors.Add(new AppMessage("The field does not have a value.", $"Method: {methodPath}"));

                parameters.Add(new Field(IPath.CombinePath(name.Trim(' '), methodPath), parentObject) { Value = value });
            }

            return parameters;
        }
        public static string CreateMethodRegex { get; } = Extensions.GetPrefixRegex("def") + $"\\s+{ObjectNameRegex}\\s*\\(";
    }
}

using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Models.Exts;
using MonoScript.Runtime;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Types;
using System.Collections.Generic;
using System.Linq;

namespace MonoScript.Script.Elements
{
    public class Field : MonoObject, IModifier, IObjectParent
    {
        public object ParentObject { get; }
        public dynamic Value { get; set; }
        public List<string> Modifiers { get; set; } = new List<string>();
        public List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>() 
        { 
            new string[] { "public", "static", "readonly" },
            new string[] { "private", "static", "readonly" },
            new string[] { "protected", "static", "readonly" },
            new string[] { "public", "const" },
            new string[] { "private", "const" },
            new string[] { "protected", "const" },
        };
        public Field(string fullpath, object parentObject, params string[] modifiers)
        {
            FullPath = fullpath;
            ParentObject = parentObject;
            AddModifiers(modifiers.ToList());
        }
        public Field CloneObject()
        {
            Field field = (Field)MemberwiseClone();
            field.Modifiers = Modifiers.ToList();
            field.AllowedModifierGroups = AllowedModifierGroups.ToList();

            return field;
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

        public static string CreateFieldRegex { get; } = $"{Extensions.GetPrefixRegex("var")}\\s+{ObjectNameRegex}";

        public static (string, int) GetFieldValue(string script, int pos)
        {
            string value = null;
            bool hasValue = false;
            int savePos = pos;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (; pos < script.Length; pos++)
            {
                if (!hasValue && script[pos] == '=')
                {
                    hasValue = true;
                    continue;
                }

                if (!hasValue && script[pos].Contains(";\n"))
                    return (null, savePos);

                if (hasValue)
                {
                    if (quoteModel.HasQuotes || (!quoteModel.HasQuotes && !script[pos].Contains(";\n")))
                        value += script[pos];

                    Extensions.IsOpenQuote(script, pos, ref quoteModel);

                    if (!quoteModel.HasQuotes && script[pos].Contains(";\n"))
                        break;
                }
            }

            return (value, pos);
        }
        public static void InitializeFields(List<Field> fields)
        {
            foreach (Field field in fields)
            {
                FindContext context = new FindContext(field);
                ExecuteContextCollection executeContext = ExecuteContextCollection.Default;
                context.MonoType = field.ParentObject is MonoType ? field.ParentObject as MonoType : ((field.ParentObject as Method)?.ParentObject as MonoType);
                context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

                if (field.Modifiers.Contains("readonly"))
                    executeContext = ExecuteContextCollection.Readonly;

                if (field.Modifiers.Contains("const"))
                    executeContext = ExecuteContextCollection.Const;

                field.Value = MonoInterpreter.ExecuteExpression(field.Value, field, context, executeContext);
            }
        }

    }
}

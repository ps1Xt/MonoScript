using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
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
            new string[] { "public", "static", "new", "readonly" },
            new string[] { "private", "static", "new", "readonly" },
            new string[] { "protected", "static", "new", "readonly" },
            new string[] { "public", "const", "new" },
            new string[] { "private", "const", "new" },
            new string[] { "protected", "const", "new" },
            new string[] { "public", "sealed", "new" },
            new string[] { "protected", "sealed", "new" },
            new string[] { "public", "inherit" },
            new string[] { "protected", "inherit" }
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
                FindContext context = new FindContext(field.Modifiers.Contains("static"));
                context.MonoType = field.ParentObject is MonoType ? field.ParentObject as MonoType : ((field.ParentObject as Method)?.ParentObject as MonoType);
                context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

                ExecuteContextCollection executeContext = ExecuteContextCollection.Default;

                if (field.Modifiers.Contains("readonly"))
                    executeContext = ExecuteContextCollection.Readonly;

                if (field.Modifiers.Contains("const"))
                    executeContext = ExecuteContextCollection.Const;

                var executeResult = MonoInterpreter.ExecuteExpression(field.Value, field, context, executeContext);
                field.Value = executeResult;

                if (executeResult is Field executeField)
                    field.Value = executeField.Value;
            }
        }

    }
}

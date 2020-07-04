using MonoScript.Models;
using MonoScript.Script;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoScript.Runtime;
using MonoScript.Models.Analytics;

namespace MonoScript.Models.Application
{
    public class Application
    {
        public ScriptFile MainScript { get; private set; }
        public List<ScriptFile> ImportedScripts { get; private set; } = new List<ScriptFile>();

        public Status RunApplication(string source)
        {
            MainScript = new ScriptFile(source, null);

            return new MonoInterpreter(this).RunApplication();
        }

        public static void InitializePublicModifiers(Application app)
        {
            if (!app.MainScript.Root.Class.Modifiers.Contains("public", "protected", "private"))
                app.MainScript.Root.Class.Modifiers.Add("public");

            if (!app.MainScript.Root.Method.Modifiers.Contains("public", "protected", "private"))
                app.MainScript.Root.Method.Modifiers.Add("public");

            foreach (var obj in app.MainScript.Classes)
            {
                if (!obj.Modifiers.Contains("public", "protected", "private"))
                    obj.Modifiers.Add("public");

                foreach (var subObj in obj.Methods)
                {
                    if (!subObj.Modifiers.Contains("public", "protected", "private"))
                        subObj.Modifiers.Add("public");
                }

                foreach (var subObj in obj.Fields)
                {
                    if (!subObj.Modifiers.Contains("public", "protected", "private"))
                        subObj.Modifiers.Add("public");
                }
            }

            foreach (var obj in app.MainScript.Structs)
            {
                if (!obj.Modifiers.Contains("public", "protected", "private"))
                    obj.Modifiers.Add("public");

                foreach (var subObj in obj.Methods)
                {
                    if (!subObj.Modifiers.Contains("public", "protected", "private"))
                        subObj.Modifiers.Add("public");
                }

                foreach (var subObj in obj.Fields)
                {
                    if (!subObj.Modifiers.Contains("public", "protected", "private"))
                        subObj.Modifiers.Add("public");
                }
            }

            foreach (var obj in app.MainScript.Enums)
            {
                if (!obj.Modifiers.Contains("public", "protected", "private"))
                    obj.Modifiers.Add("public");
            }

            foreach (var import in app.ImportedScripts)
            {
                if (!import.Root.Class.Modifiers.Contains("public", "protected", "private"))
                    import.Root.Class.Modifiers.Add("public");

                if (!import.Root.Method.Modifiers.Contains("public", "protected", "private"))
                    import.Root.Method.Modifiers.Add("public");

                foreach (var obj in import.Classes)
                {
                    if (!obj.Modifiers.Contains("public", "protected", "private"))
                        obj.Modifiers.Add("public");

                    foreach (var subObj in obj.Methods)
                    {
                        if (!subObj.Modifiers.Contains("public", "protected", "private"))
                            subObj.Modifiers.Add("public");
                    }

                    foreach (var subObj in obj.Fields)
                    {
                        if (!subObj.Modifiers.Contains("public", "protected", "private"))
                            subObj.Modifiers.Add("public");
                    }
                }

                foreach (var obj in import.Structs)
                {
                    if (!obj.Modifiers.Contains("public", "protected", "private"))
                        obj.Modifiers.Add("public");

                    foreach (var subObj in obj.Methods)
                    {
                        if (!subObj.Modifiers.Contains("public", "protected", "private"))
                            subObj.Modifiers.Add("public");
                    }

                    foreach (var subObj in obj.Fields)
                    {
                        if (!subObj.Modifiers.Contains("public", "protected", "private"))
                            subObj.Modifiers.Add("public");
                    }
                }

                foreach (var obj in import.Enums)
                {
                    if (!obj.Modifiers.Contains("public", "protected", "private"))
                        obj.Modifiers.Add("public");
                }
            }
        }
        public static void InitializeConstructors(Application app)
        {
            app.MainScript.Root.Class.TryAddConstructor();

            foreach (var obj in app.MainScript.Classes)
                obj.TryAddConstructor();

            foreach (var obj in app.MainScript.Structs)
                obj.TryAddConstructor();

            foreach (var import in app.MainScript.Imports)
            {
                foreach (var obj in import.Classes)
                    obj.TryAddConstructor();

                foreach (var obj in import.Structs)
                    obj.TryAddConstructor();
            }
        }
        public static void InitializeFields(Application app)
        {
            #region Initialize Fields

            #region Constant

            for (int i = app.ImportedScripts.Count - 1; i >= 0; i--)
            {
                foreach (var obj in app.ImportedScripts[i].Classes)
                    Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("const")).ToList());

                foreach (var obj in app.ImportedScripts[i].Structs)
                    Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("const")).ToList());
            }

            foreach (var obj in app.MainScript.Classes)
                Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("const")).ToList());

            foreach (var obj in app.MainScript.Structs)
                Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("const")).ToList());

            #endregion

            #region Static

            for (int i = app.ImportedScripts.Count - 1; i >= 0; i--)
            {
                foreach (var obj in app.ImportedScripts[i].Classes)
                    Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("static")).ToList());

                foreach (var obj in app.ImportedScripts[i].Structs)
                    Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("static")).ToList());
            }

            foreach (var obj in app.MainScript.Classes)
                Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("static")).ToList());

            foreach (var obj in app.MainScript.Structs)
                Field.InitializeFields(obj.Fields.Where(x => x.Modifiers.Contains("static")).ToList());

            #endregion

            #region Default

            for (int i = app.ImportedScripts.Count - 1; i >= 0; i--)
            {
                foreach (var obj in app.ImportedScripts[i].Classes)
                    Field.InitializeFields(obj.Fields.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());

                foreach (var obj in app.ImportedScripts[i].Structs)
                    Field.InitializeFields(obj.Fields.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());
            }

            foreach (var obj in app.MainScript.Classes)
                Field.InitializeFields(obj.Fields.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());

            foreach (var obj in app.MainScript.Structs)
                Field.InitializeFields(obj.Fields.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());

            #endregion

            #endregion

            #region Initialize Methods Field

            #region Constant

            for (int i = app.ImportedScripts.Count - 1; i >= 0; i--)
            {
                foreach (var obj in app.ImportedScripts[i].Classes)
                    foreach (var method in obj.Methods)
                        Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("const")).ToList());

                foreach (var obj in app.ImportedScripts[i].Structs)
                    foreach (var method in obj.Methods)
                        Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("const")).ToList());
            }

            foreach (var obj in app.MainScript.Classes)
                foreach (var method in obj.Methods)
                    Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("const")).ToList());

            foreach (var obj in app.MainScript.Structs)
                foreach (var method in obj.Methods)
                    Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("const")).ToList());

            #endregion

            #region Static

            for (int i = app.ImportedScripts.Count - 1; i >= 0; i--)
            {
                foreach (var obj in app.ImportedScripts[i].Classes)
                    foreach (var method in obj.Methods)
                        Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("static")).ToList());

                foreach (var obj in app.ImportedScripts[i].Structs)
                    foreach (var method in obj.Methods)
                        Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("static")).ToList());
            }

            foreach (var obj in app.MainScript.Classes)
                foreach (var method in obj.Methods)
                    Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("static")).ToList());

            foreach (var obj in app.MainScript.Structs)
                foreach (var method in obj.Methods)
                    Field.InitializeFields(method.Parameters.Where(x => x.Modifiers.Contains("static")).ToList());

            #endregion

            #region Default

            for (int i = app.ImportedScripts.Count - 1; i >= 0; i--)
            {
                foreach (var obj in app.ImportedScripts[i].Classes)
                    foreach (var method in obj.Methods)
                        Field.InitializeFields(method.Parameters.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());

                foreach (var obj in app.ImportedScripts[i].Structs)
                    foreach (var method in obj.Methods)
                        Field.InitializeFields(method.Parameters.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());
            }

            foreach (var obj in app.MainScript.Classes)
                foreach (var method in obj.Methods)
                    Field.InitializeFields(method.Parameters.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());

            foreach (var obj in app.MainScript.Structs)
                foreach (var method in obj.Methods)
                    Field.InitializeFields(method.Parameters.Where(x => !x.Modifiers.Contains("const") && !x.Modifiers.Contains("static")).ToList());

            #endregion

            #endregion
        }
        public static void InitializeInheritance(Application app)
        {
            List<IInherit<Class>> inherits = new List<IInherit<Class>>();
            inherits.AddRange(app.MainScript.Classes);

            foreach (var obj in app.ImportedScripts)
                inherits.AddRange(obj.Classes);

            IInherit<Class>.InitializeInheritance(inherits);
        }
    }
}

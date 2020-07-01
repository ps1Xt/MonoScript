using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Runtime;
using MonoScript.Script;
using MonoScript.Script.Basic;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Analytics
{
    public static class Analyzer
    {
        public static void AnalyzeAll(Application app)
        {
            ScanImports(app);
            ScanUsings(app);
            ScanPaths(app);
            ScanTypes(app);
            ScanElements(app);
        }

        public static void ScanImports(Application app)
        {
            List<string> usingsWarnings = new List<string>();

            foreach (var obj in app.MainScript.Imports)
            {
                if (app.MainScript.Imports.Count(x => x.Source == obj.Source) > 1 && !usingsWarnings.Contains(obj.Source))
                {
                    usingsWarnings.Add(obj.Source);
                    MLog.AppErrors.Add(new AppMessage("Script file is connected more than once.", $"Script {app.MainScript.Source}, Import {obj.Source}"));
                }
            }

            foreach (var importScript in app.ImportedScripts)
            {
                foreach (var obj in importScript.Imports)
                {
                    if (app.MainScript.Imports.Count(x => x.Source == obj.Source) > 1 && !usingsWarnings.Contains(obj.Source))
                    {
                        usingsWarnings.Add(obj.Source);
                        MLog.AppErrors.Add(new AppMessage("Script file is connected more than once.", $"Script {importScript.Source}, Import {obj.Source}"));
                    }
                }
            }
        }
        public static void ScanUsings(Application app)
        {
            List<string> usingsErrors = new List<string>();
            List<string> usingsWarnings = new List<string>();

            foreach (var obj in app.MainScript.Usings)
            {

                if (obj.AsName != null && app.MainScript.Usings.Count(x => x.AsName == obj.AsName) > 1 && !usingsErrors.Contains(obj.AsName))
                {
                    usingsErrors.Add(obj.AsName);
                    MLog.AppErrors.Add(new AppMessage("Using name duplicates more than once.", $"Name {obj.AsName}"));
                }

                if (obj.AsName == null && app.MainScript.Usings.Count(x => x.Path == obj.Path) > 1 && !usingsWarnings.Contains(obj.Path))
                {
                    usingsWarnings.Add(obj.Path);
                    MLog.AppWarnings.Add(new AppMessage("Namespace usings more than once", $"Path {obj.Path}"));
                }
            }

            foreach (var importScript in app.ImportedScripts)
            {
                usingsErrors = new List<string>();
                usingsWarnings = new List<string>();

                foreach (var obj in importScript.Usings)
                {
                    usingsErrors = new List<string>();
                    usingsWarnings = new List<string>();

                    if (obj.AsName != null && app.MainScript.Usings.Count(x => x.AsName == obj.AsName) > 1 && !usingsErrors.Contains(obj.AsName))
                    {
                        usingsErrors.Add(obj.AsName);
                        MLog.AppErrors.Add(new AppMessage("Using name duplicates more than once.", $"Name {obj.AsName}"));
                    }

                    if (obj.AsName == null && app.MainScript.Usings.Count(x => x.Path == obj.Path) > 1 && !usingsWarnings.Contains(obj.Path))
                    {
                        usingsWarnings.Add(obj.Path);
                        MLog.AppWarnings.Add(new AppMessage("Namespace usings more than once", $"Path {obj.Path}"));
                    }
                }
            }
        }
        public static void ScanPaths(Application app)
        {
            List<string> paths = new List<string>();
            List<MonoObject> monoObject = new List<MonoObject>();
            monoObject.AddRange(app.MainScript.Namespaces);
            monoObject.AddRange(app.MainScript.Classes);
            monoObject.AddRange(app.MainScript.Structs);
            monoObject.AddRange(app.MainScript.Enums);

            foreach (var obj in monoObject)
            {
                if (monoObject.Count(x => x.FullPath == obj.FullPath) > 1 && !paths.Contains(obj.FullPath))
                {
                    paths.Add(obj.FullPath);
                    MLog.AppErrors.Add(new AppMessage("Each object must have a unique path.", $"Path {obj.FullPath}"));
                }
            }

            foreach (var importScript in app.ImportedScripts)
            {
                paths = new List<string>();
                monoObject = new List<MonoObject>();
                monoObject.AddRange(importScript.Namespaces);
                monoObject.AddRange(importScript.Classes);
                monoObject.AddRange(importScript.Structs);
                monoObject.AddRange(importScript.Enums);

                foreach (var obj in monoObject)
                {
                    if (monoObject.Count(x => x.FullPath == obj.FullPath) > 1 && !paths.Contains(obj.FullPath))
                    {
                        paths.Add(obj.FullPath);
                        MLog.AppErrors.Add(new AppMessage("Each object must have a unique path.", $"Path {obj.FullPath}"));
                    }
                }
            }
        }
        public static void ScanTypes(Application app)
        {
            foreach (var obj in app.MainScript.Classes)
            {
                if (obj.Modifiers.Contains("static"))
                {
                    if (obj.OverloadOperators.Count > 0)
                        MLog.AppErrors.Add(new AppMessage("In a class with static modifier there should not be methods of redefinition of operators.", $"Path {obj.FullPath}"));

                    foreach (var subObj in obj.Methods)
                    {
                        if (!subObj.Modifiers.Contains("static"))
                            MLog.AppErrors.Add(new AppMessage("Each method of a static class must have a static modifier.", $"Path {subObj.FullPath}"));
                    }

                    foreach (var subObj in obj.Fields)
                    {
                        if (!subObj.Modifiers.Contains("static"))
                            MLog.AppErrors.Add(new AppMessage("Each field of a static class must have a static modifier.", $"Path {subObj.FullPath}"));
                    }
                }

                if (obj.Modifiers.Contains("readonly"))
                {
                    if (obj.Methods.Count > 0 || obj.OverloadOperators.Count > 0)
                        MLog.AppErrors.Add(new AppMessage("In a class with readonly modifier, there should not be methods with readonly modifier.", $"Path {obj.FullPath}"));

                    foreach (var subObj in obj.Fields)
                    {
                        if (!subObj.Modifiers.Contains("readonly"))
                            MLog.AppErrors.Add(new AppMessage("Each field of a readonly class must have a readonly modifier.", $"Path {subObj.FullPath}"));
                    }
                }
            }
            foreach (var obj in app.MainScript.Structs)
            {
                if (obj.Modifiers.Contains("readonly"))
                {
                    if (obj.Methods.Count > 0 || obj.OverloadOperators.Count > 0)
                        MLog.AppErrors.Add(new AppMessage("In a struct with readonly modifier, there should not be methods with readonly modifier.", $"Path {obj.FullPath}"));

                    foreach (var subObj in obj.Fields)
                    {
                        if (!subObj.Modifiers.Contains("readonly"))
                            MLog.AppErrors.Add(new AppMessage("Each field of a readonly struct must have a readonly modifier.", $"Path {subObj.FullPath}"));
                    }
                }
            }
        }
        public static void ScanElements(Application app)
        {
            List<string> paths;

            foreach (var obj in app.MainScript.Classes)
            {
                paths = new List<string>();
                foreach (var subObj in obj.Methods)
                {
                    if (subObj.Name == (subObj.ParentObject as MonoType)?.Name)
                    {
                        if (subObj.Modifiers.Contains("static"))
                        {
                            if (subObj.Modifiers.Contains("public", "protected", "virtual", "ovveride", "inherit", "sealed"))
                                MLog.AppErrors.Add(new AppMessage("The static constructor can only have the modifier private.", $"Path {subObj.FullPath}"));
                        }
                        else
                        {
                            if (subObj.Modifiers.Contains("virtual", "ovveride", "inherit", "sealed"))
                                MLog.AppErrors.Add(new AppMessage("The constructor can only have the modifier public, private, protected, static.", $"Path {subObj.FullPath}"));
                        }
                    }
                    if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                    {
                        paths.Add(subObj.Name);
                        MLog.AppErrors.Add(new AppMessage("The method with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                    }
                }

                paths = new List<string>();
                foreach (var subObj in obj.OverloadOperators)
                {
                    if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                    {
                        paths.Add(subObj.Name);
                        MLog.AppErrors.Add(new AppMessage("The method overload operators with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                    }
                }

                paths = new List<string>();
                foreach (var subObj in obj.Fields)
                {
                    if (obj.Fields.Count(x => x.Name == subObj.Name) > 1 && !paths.Contains(subObj.Name))
                    {
                        paths.Add(subObj.Name);
                        MLog.AppErrors.Add(new AppMessage("The object contains fields that are repeated.", $"Path {subObj.FullPath}"));
                    }
                }
            }
            foreach (var obj in app.MainScript.Structs)
            {
                paths = new List<string>();
                foreach (var subObj in obj.Methods)
                {
                    if (subObj.Modifiers.Contains("static"))
                    {
                        if (subObj.Modifiers.Contains("public", "protected", "virtual", "ovveride", "inherit", "sealed"))
                            MLog.AppErrors.Add(new AppMessage("The static constructor can only have the modifier private.", $"Path {subObj.FullPath}"));
                    }
                    else
                    {
                        if (subObj.Modifiers.Contains("virtual", "ovveride", "inherit", "sealed"))
                            MLog.AppErrors.Add(new AppMessage("The constructor can only have the modifier public, private, protected, static.", $"Path {subObj.FullPath}"));
                    }

                    if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                    {
                        paths.Add(subObj.Name);
                        MLog.AppErrors.Add(new AppMessage("The method with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                    }
                }

                paths = new List<string>();
                foreach (var subObj in obj.OverloadOperators)
                {
                    if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                    {
                        paths.Add(subObj.Name);
                        MLog.AppErrors.Add(new AppMessage("The method overload operators with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                    }
                }

                paths = new List<string>();
                foreach (var subObj in obj.Fields)
                {
                    if (obj.Fields.Count(x => x.Name == subObj.Name) > 1 && !paths.Contains(subObj.Name))
                    {
                        paths.Add(subObj.Name);
                        MLog.AppErrors.Add(new AppMessage("The object contains fields that are repeated.", $"Path {subObj.FullPath}"));
                    }
                }
            }

            foreach (var importScript in app.ImportedScripts)
            {
                foreach (var obj in importScript.Classes)
                {
                    paths = new List<string>();
                    foreach (var subObj in obj.Methods)
                    {
                        if (subObj.Modifiers.Contains("static"))
                        {
                            if (subObj.Modifiers.Contains("public", "protected", "virtual", "ovveride", "inherit", "sealed"))
                                MLog.AppErrors.Add(new AppMessage("The static constructor can only have the modifier private.", $"Path {subObj.FullPath}"));
                        }
                        else
                        {
                            if (subObj.Modifiers.Contains("virtual", "ovveride", "inherit", "sealed"))
                                MLog.AppErrors.Add(new AppMessage("The constructor can only have the modifier public, private, protected, static.", $"Path {subObj.FullPath}"));
                        }

                        if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                        {
                            paths.Add(subObj.Name);
                            MLog.AppErrors.Add(new AppMessage("The method with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                        }
                    }

                    paths = new List<string>();
                    foreach (var subObj in obj.OverloadOperators)
                    {
                        if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                        {
                            paths.Add(subObj.Name);
                            MLog.AppErrors.Add(new AppMessage("The method overload operators with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                        }
                    }

                    paths = new List<string>();
                    foreach (var subObj in obj.Fields)
                    {
                        if (obj.Fields.Count(x => x.Name == subObj.Name) > 1 && !paths.Contains(subObj.Name))
                        {
                            paths.Add(subObj.Name);
                            MLog.AppErrors.Add(new AppMessage("The object contains fields that are repeated.", $"Path {subObj.FullPath}"));
                        }
                    }
                }
                foreach (var obj in importScript.Structs)
                {
                    paths = new List<string>();
                    foreach (var subObj in obj.Methods)
                    {
                        if (subObj.Modifiers.Contains("static"))
                        {
                            if (subObj.Modifiers.Contains("public", "protected", "virtual", "ovveride", "inherit", "sealed"))
                                MLog.AppErrors.Add(new AppMessage("The static constructor can only have the modifier private.", $"Path {subObj.FullPath}"));
                        }
                        else
                        {
                            if (subObj.Modifiers.Contains("virtual", "ovveride", "inherit", "sealed"))
                                MLog.AppErrors.Add(new AppMessage("The constructor can only have the modifier public, private, protected, static.", $"Path {subObj.FullPath}"));
                        }

                        if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                        {
                            paths.Add(subObj.Name);
                            MLog.AppErrors.Add(new AppMessage("The method with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                        }
                    }

                    paths = new List<string>();
                    foreach (var subObj in obj.OverloadOperators)
                    {
                        if (obj.Methods.Count(x => x.Name == subObj.Name && x.Parameters.Count == subObj.Parameters.Count) > 1 && !paths.Contains(subObj.Name))
                        {
                            paths.Add(subObj.Name);
                            MLog.AppErrors.Add(new AppMessage("The method overload operators with the same name by the number of input parameters is repeated.", $"Path {subObj.FullPath}"));
                        }
                    }

                    paths = new List<string>();
                    foreach (var subObj in obj.Fields)
                    {
                        if (obj.Fields.Count(x => x.Name == subObj.Name) > 1 && !paths.Contains(subObj.Name))
                        {
                            paths.Add(subObj.Name);
                            MLog.AppErrors.Add(new AppMessage("The object contains fields that are repeated.", $"Path {subObj.FullPath}"));
                        }
                    }
                }
            }
        }
    }
}

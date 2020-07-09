using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Models;
using MonoScript.Models.Application;
using MonoScript.Runtime;
using MonoScript.Script;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonoScript.Models.Analytics;
using MonoScript.Models.Exts;

namespace MonoScript
{
    public class Parser
    {
        Application app;
        string script;
        bool checkscript;
        public Parser(Application app) => this.app = app;

        public static void ParseScriptFile(Application app)
        {
            Parser parser = new Parser(app);
            parser.ParseScriptFile(app.MainScript.Source);
        }
        public ScriptFile ParseScriptFile(string source)
        {
            if (File.Exists(source))
            {
                ScriptFile scriptFile = app.MainScript;

                if (app.MainScript.Source != source)
                {
                    scriptFile = new ScriptFile(source, null);
                    app.ImportedScripts.Add(scriptFile);
                }

                if (checkscript && scriptFile.Source == app.MainScript.Source)
                    MLog.AppErrors.Add(new AppMessage("You cannot include a script file that may link to itself.", string.Format("ImportPath: {0}, Path: {1}", scriptFile.Source, app.MainScript.Source)));
                else
                    checkscript = true;

                string path = Regex.Replace(Path.GetFileNameWithoutExtension(scriptFile.Source), "[^A-z0-9@_]", "_");
                script = File.ReadAllText(scriptFile.Source);

                if (!MonoObject.IsCorrectPath(path))
                    path = path.Insert(0, "_");

                scriptFile.SetRoots(path, scriptFile.Root.Class.Name, scriptFile.Root.Method.Name);
                scriptFile.Root.Method.Content = ExtractAll(scriptFile);

                return scriptFile;
            }
            else
            {
                string combinePath = Path.Combine(Path.GetDirectoryName(app.MainScript.Source), source);
                
                if (!Path.HasExtension(source) && File.Exists(source + ".ms"))
                    combinePath = source + ".ms";
                else if (!File.Exists(combinePath)) 
                    MLog.AppErrors.Add(new AppMessage("Bad import. File not found.", string.Format("Path: {0}", source)));

                if (checkscript && combinePath == app.MainScript.ImportName)
                    MLog.AppErrors.Add(new AppMessage("You cannot include a script file that may link to itself.", string.Format("ImportPath: {0}, Path: {1}", combinePath, app.MainScript.Source)));
                else
                    checkscript = true;

                ScriptFile scriptFile = new ScriptFile(combinePath, source);
                app.ImportedScripts.Add(scriptFile);

                string path = Regex.Replace(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(scriptFile.Source)), "[^A-z0-9@_]", "_");
                script = File.ReadAllText(scriptFile.Source);

                if (!MonoObject.IsCorrectPath(path))
                    path = path.Insert(0, "_");

                scriptFile.SetRoots(path, scriptFile.Root.Class.Name, scriptFile.Root.Method.Name);
                scriptFile.Root.Method.Content = ExtractAll(scriptFile);

                return scriptFile;
            }
        }
        public string ExtractAll(ScriptFile scriptFile)
        {
            script = ExtractSingleRemark();
            script = ExtractMultiRemark();
            script = ExtractImports(scriptFile);
            script = ExtractUsings(scriptFile);
            script = ExtractNamespaces(scriptFile, scriptFile.Root.Namespace.FullPath);
            script = ExtractClasses(scriptFile, scriptFile.Root.Namespace.FullPath);
            script = ExtractStructs(scriptFile, scriptFile.Root.Namespace.FullPath);
            script = ExtractEnums(scriptFile, scriptFile.Root.Namespace.FullPath);
            script = ExtractMethods(scriptFile.Root.Class.Methods, scriptFile.Root.Class.FullPath, scriptFile.Root.Class);

            return script;
        }
        public string ExtractSingleRemark()
        {
            while (true)
            {
                Match match = Regex.Match(script, MonoObject.SingleRemarkRegex);

                if (match.Success)
                {
                    if (!Extensions.InsideQuotes(script, match.Index).HasQuotes)
                        script = script.Remove(match.Index, match.Length);
                    else
                    {
                        string newScript = script.Remove(match.Index + match.Length);

                        script = ExtractSingleRemark();

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractMultiRemark()
        {
            while (true)
            {
                Match match = Regex.Match(script, MonoObject.MultiRemarkRegex, RegexOptions.Singleline);

                if (match.Success)
                {
                    if (!Extensions.InsideQuotes(script, match.Index).HasQuotes)
                        script = script.Remove(match.Index, match.Length);
                    else
                    {
                        string newScript = script.Remove(match.Index + match.Length);

                        script = ExtractMultiRemark();

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractImports(ScriptFile scriptFile)
        {
            while (true)
            {
                Match match = Regex.Match(script, ScriptFile.ImportRegex);

                if (match.Success)
                {
                    if (!Extensions.InsideQuotes(script, match.Index).HasQuotes)
                    {
                        string source = Regex.Match(match.Value, "(\".*\")|('.*')").Value;
                        string savescript = script;

                        scriptFile.Imports.Add(ParseScriptFile(source.Remove(0, 1).Remove(source.Length - 2)));

                        script = savescript.Remove(match.Index, match.Length);
                    }
                    else
                    {
                        string newScript = script.Remove(match.Index + match.Length);
                        script = script.Substring(match.Index + match.Length);
                        script = ExtractImports(scriptFile);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractUsings(ScriptFile scriptFile)
        {
            while (true)
            {
                Match match = Regex.Match(script, Using.UsingRegex);

                if (match.Success)
                {
                    MatchHelper mhelper = new MatchHelper(match, script, false, false, ";");

                    if (!Extensions.InsideQuotes(script, mhelper.FirstIndex).HasQuotes)
                    {
                        string[] usingInfos = Regex.Replace(match.Value, Extensions.GetPrefixRegex("using"), "").Split(" as ");

                        scriptFile.Usings.Add(new Using(IPath.Normalize(usingInfos[0]), usingInfos.Length == 2 ? IPath.Normalize(usingInfos[1]) : null));

                        script = script.RemoveIndex(mhelper.FirstIndex, mhelper.LastIndex);
                    }
                    else
                    {
                        string newScript = script.Remove(mhelper.LastIndex);

                        script = script.Substring(mhelper.LastIndex);
                        script = ExtractUsings(scriptFile);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractNamespaces(ScriptFile scriptFile, string parentPath)
        {
            while (true)
            {
                Match match = Regex.Match(script, Namespace.CreateNamespaceRegex);

                if (match.Success)
                {
                    MatchHelper mhelper = new MatchHelper(match, script, true, false);

                    if (!Extensions.InsideQuotes(script, mhelper.FirstIndex).HasQuotes && mhelper.IsCorrect())
                    {
                        string path = IPath.CombinePath(IPath.Normalize(match.Value.Replace("namespace", "")), parentPath);

                        scriptFile.Namespaces.Add(new Namespace(path, scriptFile));

                        string savescript = script;
                        script = script.SubstringIndex(mhelper.OpenBracket, mhelper.CloseBracket);
                        script = ExtractNamespaces(scriptFile, path);
                        script = ExtractClasses(scriptFile, path);
                        script = ExtractStructs(scriptFile, path);
                        script = ExtractEnums(scriptFile, path);

                        script = savescript.RemoveIndex(mhelper.FirstIndex, mhelper.CloseBracket);
                    }
                    else
                    {
                        string newScript = script.Remove(mhelper.OpenBracket);

                        script = script.Substring(mhelper.OpenBracket);
                        script = ExtractNamespaces(scriptFile, parentPath);
                        script = ExtractClasses(scriptFile, parentPath);
                        script = ExtractStructs(scriptFile, parentPath);
                        script = ExtractEnums(scriptFile, parentPath);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractClasses(ScriptFile scriptFile, string parentPath)
        {
            while (true)
            {
                Match match = Regex.Match(script, Class.CreateClassRegex);

                if (match.Success)
                {
                    MatchHelper mhelper = new MatchHelper(match, script);

                    if (!Extensions.InsideQuotes(script, mhelper.FirstIndex).HasQuotes && mhelper.IsCorrect())
                    {
                        string[] name_parent = Regex.Replace(Extensions.SubstringIndex(script, match.Index + Regex.Match(match.Value, Extensions.GetPrefixRegex("class")).Index, "{"), Extensions.GetPrefixRegex("class"), "").Split(":");

                        Class obj = new Class(IPath.CombinePath(IPath.Normalize(name_parent[0]), parentPath), scriptFile);
                        obj.AddModifiers(mhelper.Modifiers);
                        obj.Parent.StringValue = name_parent.Length == 2 ? IPath.Normalize(name_parent[1]) : null;

                        scriptFile.Classes.Add(obj);

                        string savescript = script;
                        script = script.SubstringIndex(mhelper.OpenBracket, mhelper.CloseBracket);
                        script = ExtractClasses(scriptFile, obj.FullPath);
                        script = ExtractStructs(scriptFile, obj.FullPath);
                        script = ExtractEnums(scriptFile, obj.FullPath);
                        script = ExtractMethods(obj.Methods, obj.FullPath, obj);
                        script = ExtractFields(obj.Fields, obj.FullPath, obj);

                        script = savescript.RemoveIndex(mhelper.FirstIndex, mhelper.CloseBracket);
                    }
                    else
                    {
                        string newScript = script.Remove(mhelper.OpenBracket);

                        script = script.Substring(mhelper.OpenBracket);
                        script = ExtractClasses(scriptFile, parentPath);
                        script = ExtractStructs(scriptFile, parentPath);
                        script = ExtractEnums(scriptFile, parentPath);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractStructs(ScriptFile scriptFile, string parentPath)
        {
            while (true)
            {
                Match match = Regex.Match(script, Struct.CreateStructRegex);

                if (match.Success)
                {
                    MatchHelper mhelper = new MatchHelper(match, script);

                    if (!Extensions.InsideQuotes(script, mhelper.FirstIndex).HasQuotes && mhelper.IsCorrect())
                    {
                        string path = IPath.CombinePath(Regex.Replace(Extensions.SubstringIndex(script, match.Index + Regex.Match(match.Value, Extensions.GetPrefixRegex("struct")).Index, "{"), Extensions.GetPrefixRegex("struct"), ""), parentPath);

                        Struct obj = new Struct(path, scriptFile);
                        obj.AddModifiers(mhelper.Modifiers);

                        scriptFile.Structs.Add(obj);

                        string savescript = script;
                        script = script.SubstringIndex(mhelper.OpenBracket, mhelper.CloseBracket);
                        script = ExtractClasses(scriptFile, obj.Path);
                        script = ExtractStructs(scriptFile, obj.Path);
                        script = ExtractEnums(scriptFile, obj.Path);
                        script = ExtractMethods(obj.Methods, obj.Path, obj);
                        script = ExtractFields(obj.Fields, obj.Path, obj);

                        script = savescript.RemoveIndex(mhelper.FirstIndex, mhelper.CloseBracket);
                    }
                    else
                    {
                        string newScript = script.Remove(mhelper.OpenBracket);

                        script = script.Substring(mhelper.OpenBracket);
                        script = ExtractClasses(scriptFile, parentPath);
                        script = ExtractStructs(scriptFile, parentPath);
                        script = ExtractEnums(scriptFile, parentPath);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractEnums(ScriptFile scriptFile, string parentPath)
        {
            while (true)
            {
                Match match = Regex.Match(script, MonoEnum.CreateEnumRegex);

                if (match.Success)
                {
                    MatchHelper mhelper = new MatchHelper(match, script);

                    if (!Extensions.InsideQuotes(script, mhelper.FirstIndex).HasQuotes && mhelper.IsCorrect())
                    {
                        string path = IPath.CombinePath(Regex.Replace(Extensions.SubstringIndex(script, match.Index + Regex.Match(match.Value, Extensions.GetPrefixRegex("enum")).Index, "{").Replace(" ", ""), Extensions.GetPrefixRegex("enum"), ""), parentPath);

                        MonoEnum obj = new MonoEnum(IPath.Normalize(path), scriptFile);
                        obj.AddModifiers(mhelper.Modifiers);
                        obj.Values.AddRange(MonoEnum.GetEnumValues(script.SubstringIndex(mhelper.OpenBracket, mhelper.CloseBracket), obj.FullPath, obj));

                        scriptFile.Enums.Add(obj);

                        script = script.RemoveIndex(mhelper.FirstIndex, mhelper.CloseBracket);
                    }
                    else
                    {
                        string newScript = script.Remove(mhelper.OpenBracket);

                        script = script.Substring(mhelper.OpenBracket);
                        script = ExtractEnums(scriptFile, parentPath);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractMethods(List<Method> methods, string parentPath, object parentObject)
        {
            while (true)
            {
                Match match = Regex.Match(script, Method.CreateMethodRegex);

                if (match.Success)
                {
                    MatchHelper mhelper = new MatchHelper(match, script);

                    if (!Extensions.InsideQuotes(script, mhelper.FirstIndex).HasQuotes && mhelper.IsCorrect())
                    {
                        string path = IPath.Normalize(Regex.Replace(Extensions.SubstringIndex(script, match.Index + Regex.Match(match.Value, Extensions.GetPrefixRegex("def")).Index, "("), Extensions.GetPrefixRegex("def"), ""));

                        Method obj = new Method(IPath.CombinePath(path, parentPath), parentObject);
                        obj.AddModifiers(mhelper.Modifiers);
                        obj.Content = script.SubstringIndex(mhelper.OpenBracket, mhelper.CloseBracket);
                        obj.Parameters.AddRange(Method.GetParameters(script.SubstringIndex(match.Index + match.Length, script.LastIndexOf(')', mhelper.OpenBracket)), obj.FullPath, obj));

                        methods.Add(obj);

                        script = script.RemoveIndex(mhelper.FirstIndex, mhelper.CloseBracket);
                    }
                    else
                    {
                        string newScript = script.Remove(mhelper.OpenBracket);

                        script = script.Substring(mhelper.OpenBracket);
                        script = ExtractMethods(methods, parentPath, parentObject);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }
        public string ExtractFields(List<Field> fields, string parentPath, object parentObject)
        {
            while (true)
            {
                Match match = Regex.Match(script, Field.CreateFieldRegex);

                if (match.Success)
                {
                    MatchHelper mhelper = new MatchHelper(match, script, false, true);

                    if (!Extensions.InsideQuotes(script, match.Index).HasQuotes)
                    {
                        Field obj = new Field(IPath.Normalize(IPath.CombinePath(Regex.Replace(match.Value, Extensions.GetPrefixRegex("var" + "\\s+"), ""), parentPath)), parentObject);
                        obj.AddModifiers(mhelper.Modifiers);
                        var fieldValue = Field.GetFieldValue(script, mhelper.LastIndex + 1);
                        obj.Value = fieldValue.Item1;

                        fields.Add(obj);

                        script = script.RemoveIndex(mhelper.FirstIndex, fieldValue.Item2);
                    }
                    else
                    {
                        string newScript = script.Remove(match.Index);

                        script = script.Substring(match.Index);
                        script = ExtractFields(fields, parentPath, parentObject);

                        return newScript + script;
                    }
                }
                else break;
            }

            return script;
        }

        private class MatchHelper
        {
            public int FirstIndex { get; set; }
            public int LastIndex { get; set; }
            public int OpenBracket { get; set; } = -1;
            public int CloseBracket { get; set; } = -1;
            public ModifierCollection Modifiers { get; set; }
            public MatchHelper(Match match, string script, bool haveBracket = true, bool haveModifiers = true, string contains = "{};")
            {
                FirstIndex = match.Value[0].Contains(contains) ? match.Index + 1 : match.Index;
                LastIndex = match.Index + match.Length - 1;

                if (haveBracket)
                {
                    int count = 0, i = match.Index;
                    InsideQuoteModel quoteModel = new InsideQuoteModel();

                    for (; i < script.Length; i++)
                    {
                        Extensions.IsOpenQuote(script, i, ref quoteModel);

                        if (!quoteModel.HasQuotes)
                        {
                            if (script[i] == '{')
                            {
                                if (OpenBracket == -1)
                                    OpenBracket = i + 1;

                                count++;
                            }

                            if (script[i] == '}')
                            {
                                count--;

                                if (count == 0)
                                    break;
                            }
                        }
                    }

                    if (count == 0)
                        CloseBracket = i;
                }

                if (haveModifiers)
                {
                    Modifiers = ModifierCollection.GetModifiers(script, match.Index);

                    if (Modifiers.FirstIndex != -1)
                        FirstIndex = Modifiers.FirstIndex;
                }
            }

            public bool IsCorrect()
            {
                if (OpenBracket == -1 && CloseBracket == -1)
                {
                    OpenBracket = 0;
                    CloseBracket = 0;

                    return false;
                }

                return true;
            }
        }
    }
}

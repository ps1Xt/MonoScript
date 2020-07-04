using MonoScript.Runtime;
using MonoScript.Script;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;

namespace MonoScript.Models
{
    public static class Finder
    {
        public static dynamic FindObject(string path, FindContext context)
        {
            if (context.LocalSpace != null)
            {
                string[] splitPaths = IPath.SplitPath(path);

                if (splitPaths.Length > 1)
                {
                    Field foundField = context.LocalSpace.Find(splitPaths[0]);

                    if (foundField != null)
                    {
                        object foundObj = foundField.Value;

                        for (int i = 1; i < splitPaths.Length; i++)
                        {
                            if (foundObj is MonoType)
                            {
                                var obj = (foundObj as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[i]);

                                if (obj != null)
                                {
                                    bool fine = obj.Modifiers.Contains("public");

                                    if (!fine && obj.Modifiers.Contains("private"))
                                        fine = (obj.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                    if (!fine && obj.Modifiers.Contains("protected") && obj.ParentObject is Class objClass)
                                        fine = objClass.ContainsParent(context.MonoType as Class);

                                    if (fine)
                                        foundObj = obj;
                                    else
                                        return null;
                                }
                                else
                                {
                                    var method = (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i]);

                                    if (method != null && i + 1 == splitPaths.Length)
                                    {
                                        bool fine = method.Modifiers.Contains("public");

                                        if (!fine && method.Modifiers.Contains("private"))
                                            fine = (method.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                        if (!fine && method.Modifiers.Contains("protected") && method.ParentObject is Class objClass)
                                            fine = objClass.ContainsParent(context.MonoType as Class);

                                        if (fine)
                                            return method;

                                        return null;
                                    }
                                }

                                continue;
                            }

                            if (foundObj is EnumValue)
                            {
                                if (i + 1 != splitPaths.Length)
                                    return null;

                                return foundObj;
                            }

                            if (foundObj is Field)
                            {
                                bool fine = (foundObj as Field).Modifiers.Contains("public");

                                if (!fine && (foundObj as Field).Modifiers.Contains("private"))
                                    fine = ((foundObj as Field).ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                if (!fine && (foundObj as Field).Modifiers.Contains("protected") && (foundObj as Field).ParentObject is Class objClass)
                                    fine = objClass.ContainsParent(context.MonoType as Class);

                                if (fine)
                                {
                                    if (i + 1 == splitPaths.Length)
                                        return foundObj;
                                    else
                                        foundObj = (foundObj as Field).Value;
                                }
                                else
                                    return null;
                            }
                        }

                        return foundObj;
                    }
                }
                else
                {
                    var foundObj = context.LocalSpace.Find(IPath.NormalizeWithTrim(path));

                    if (foundObj != null)
                        return foundObj.Value;
                }
            }

            if (context.MonoType != null)
            {
                string[] splitPaths = IPath.SplitPath(IPath.NormalizeWithTrim(path));

                if (splitPaths.Length > 1)
                {
                    Field foundField = context.MonoType.Fields.FirstOrDefault(x => x.Name == splitPaths[0]);

                    if (foundField != null)
                    {
                        object foundObj = foundField.Value;

                        for (int i = 1; i < splitPaths.Length; i++)
                        {
                            if (foundObj is MonoType)
                            {
                                var obj = (foundObj as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[i]);

                                if (obj != null)
                                {
                                    bool fine = obj.Modifiers.Contains("public");

                                    if (!fine && obj.Modifiers.Contains("private"))
                                        fine = (obj.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                    if (!fine && obj.Modifiers.Contains("protected") && obj.ParentObject is Class objClass)
                                        fine = objClass.ContainsParent(context.MonoType as Class);

                                    if (fine)
                                        foundObj = obj;
                                    else
                                        return null;
                                }
                                else
                                {
                                    var method = (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i]);

                                    if (method != null && i + 1 == splitPaths.Length)
                                    {
                                        bool fine = method.Modifiers.Contains("public");

                                        if (!fine && method.Modifiers.Contains("private"))
                                            fine = (method.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                        if (!fine && method.Modifiers.Contains("protected") && method.ParentObject is Class objClass)
                                            fine = objClass.ContainsParent(context.MonoType as Class);

                                        if (fine)
                                            return method;

                                        return null;
                                    }
                                }

                                continue;
                            }

                            if (foundObj is EnumValue)
                            {
                                if (i + 1 != splitPaths.Length)
                                    return null;

                                return foundObj;
                            }

                            if (foundObj is Field)
                            {
                                bool fine = (foundObj as Field).Modifiers.Contains("public");

                                if (!fine && (foundObj as Field).Modifiers.Contains("private"))
                                    fine = ((foundObj as Field).ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                if (!fine && (foundObj as Field).Modifiers.Contains("protected") && (foundObj as Field).ParentObject is Class objClass)
                                    fine = objClass.ContainsParent(context.MonoType as Class);

                                if (fine)
                                {
                                    if (i + 1 == splitPaths.Length)
                                        return foundObj;
                                    else
                                        foundObj = (foundObj as Field).Value;
                                }
                                else
                                    return null;
                            }
                        }

                        return foundObj;
                    }
                }
                else
                {
                    var foundMethod = context.MonoType.Methods.FirstOrDefault(x => x.Name == IPath.NormalizeWithTrim(path));

                    if (foundMethod != null)
                        return foundMethod;

                    var foundObj = context.MonoType.Fields.FirstOrDefault(x => x.Name == IPath.NormalizeWithTrim(path));

                    if (foundObj != null)
                        return foundObj;
                }
            }

            //if (context.ScriptFile != null && context.MonoType != null)
            //{

            //}

            return null;
        }
        public static dynamic FindObject(string path, FindContext context, FindOption findOption)
        {
            if (findOption == FindOption.None)
                return FindObject(path, context);

            if (findOption == FindOption.NoStatic)
            {

            }

            if (findOption == FindOption.OnlyStatic)
            {

            }

            return null;
        }


        public enum FindOption
        {
            None, OnlyStatic, NoStatic
        }
    }
}
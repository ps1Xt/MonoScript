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
        public static object FindObject(string path, FindContext context, int methodParameters = -1)
        {
            return null;

            if (context.LocalSpace != null)
            {
                context.SearchResult = FindContextType.LocalSpace;

                string[] splitPaths = IPath.SplitPath(path.Trim(' '));

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
                                    var method = (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i] && x.Parameters.Count == methodParameters);

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
                                    foundObj = (foundObj as Field).Value;
                                else
                                    return null;
                            }
                        }

                        return foundObj;
                    }
                }
                else
                {
                    var foundObj = context.LocalSpace.Find(path.Trim(' '));

                    if (foundObj != null)
                        return foundObj.Value;
                }
            }

            if (context.MonoType != null)
            {
                context.SearchResult = FindContextType.MonoType;

                string[] splitPaths = IPath.SplitPath(path.Trim(' '));

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
                                    var method = (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i] && x.Parameters.Count == methodParameters);

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
                                    foundObj = (foundObj as Field).Value;
                                else
                                    return null;
                            }
                        }

                        return foundObj;
                    }
                }
                else
                {
                    var foundMethod = context.MonoType.Methods.FirstOrDefault(x => x.Name == path.Trim(' ') && x.Parameters.Count == methodParameters);

                    if (foundMethod != null)
                        return foundMethod;

                    var foundObj = context.MonoType.Fields.FirstOrDefault(x => x.Name == path.Trim(' '));

                    if (foundObj != null)
                        return foundObj.Value;
                }
            }

            if (context.ScriptFile != null && context.MonoType != null)
            {
                context.SearchResult = FindContextType.ScriptFileWithMonoType;

                #region FindToLocalPathTypes[Types,Fields,Methods]

                #region Namespaces

                List<Namespace> namespaces = new List<Namespace>();
                namespaces.AddRange(context.ScriptFile.Namespaces.Where(x => context.MonoType.FullPath.StartsWith(x.FullPath)).ToList());

                if (context.MonoType.FullPath.StartsWith(context.ScriptFile.Root.Namespace.FullPath))
                    namespaces.Add(context.ScriptFile.Root.Namespace);

                #endregion


                #region Types

                List<MonoType> monoTypes = new List<MonoType>();
                monoTypes.AddRange(context.ScriptFile.Classes.Where(x => context.MonoType.FullPath.StartsWith(x.Path)));
                monoTypes.AddRange(context.ScriptFile.Structs.Where(x => context.MonoType.FullPath.StartsWith(x.Path)));

                if (context.MonoType.FullPath.StartsWith(context.ScriptFile.Root.Class.Path))
                    monoTypes.Add(context.ScriptFile.Root.Class);

                //foreach (var obj in monoTypes.ToList())
                //{
                //    monoTypes.AddRange(context.ScriptFile.Classes.Where(x => x.Path == obj.FullPath));
                //    monoTypes.AddRange(context.ScriptFile.Structs.Where(x => x.Path == obj.FullPath));
                //}

                //переделать поиск

                #endregion









                foreach (var obj in monoTypes)
                {
                    bool fine = true;

                    string[] splitFindPaths = IPath.SplitPath(path);
                    string[] splitObjPaths = IPath.SplitPath(obj.FullPath);

                    for (int i = 0; i < splitObjPaths.Length; i++)
                    {
                        object foundObj = monoTypes.FirstOrDefault(x => x.Name == splitObjPaths[i]);

                        if (foundObj == null)
                            foundObj = namespaces.FirstOrDefault(x => x.Name == splitObjPaths[i]);

                        if (foundObj != null)
                        {
                            if (foundObj is Namespace)
                                continue;

                            if (foundObj is MonoType foundType)
                            {
                                if (foundType.Modifiers.Contains("private"))
                                {
                                    if (i + 1 < splitObjPaths.Length)
                                    {
                                        fine = false;
                                        break;
                                    }
                                    else
                                    {
                                        if (IPath.ContainsWithLevelAccess(context.MonoType.FullPath, foundType.FullPath))
                                            fine = true;
                                        else
                                            fine = false;
                                    }
                                }
                            }
                            else
                            {
                                fine = false;
                                break;
                            }
                        }
                        else
                        {
                            fine = false;
                            break;
                        }
                    }

                    if (fine)
                    {
                        if (obj.Name == path || obj.FullPath == path)
                            return obj;

                        if (splitFindPaths[0] == obj.Name)
                        {
                            foreach (var subObj in obj.Fields.Where(x => x.Modifiers.Contains("static")))
                            {
                                fine = subObj.Modifiers.Contains("public");

                                if (!fine && subObj.Modifiers.Contains("private"))
                                    fine = context.MonoType.FullPath == obj.FullPath;

                                if (fine)
                                {
                                    if (splitFindPaths.Length == 2)
                                    {
                                        if (subObj.Name == splitFindPaths[1])
                                            return subObj;
                                    }
                                    else
                                    {
                                        if (subObj.Name == splitFindPaths[1])
                                        {
                                            Field lastObj = (subObj.Value as MonoType)?.Fields.FirstOrDefault(x => x.Name == splitFindPaths[2] && !x.Modifiers.Contains("static"));

                                            if (lastObj != null)
                                            {
                                                fine = lastObj.Modifiers.Contains("public");

                                                if (!fine && lastObj.Modifiers.Contains("private"))
                                                    fine = context.MonoType.FullPath == (lastObj.ParentObject as MonoType)?.FullPath;

                                                if (!fine && lastObj.Modifiers.Contains("protected") && lastObj.ParentObject is Class objClass)
                                                    fine = objClass.ContainsParent(context.MonoType as Class);

                                                if (fine)
                                                {
                                                    if (splitFindPaths.Length == 3)
                                                        return lastObj;

                                                    for (int i = 3; i < splitFindPaths.Length; i++)
                                                    {
                                                        if (i + 1 < splitFindPaths.Length)
                                                        {
                                                            fine = lastObj.Modifiers.Contains("public");

                                                            if (!fine && lastObj.Modifiers.Contains("private"))
                                                                fine = context.MonoType.FullPath == (lastObj.ParentObject as MonoType)?.FullPath;

                                                            if (!fine && lastObj.Modifiers.Contains("protected") && lastObj.ParentObject is Class subObjClass)
                                                                fine = subObjClass.ContainsParent(context.MonoType as Class);

                                                            if (fine)
                                                            {
                                                                lastObj = (lastObj.Value as MonoType)?.Fields.FirstOrDefault(x => x.Name == splitFindPaths[i] && !x.Modifiers.Contains("static"));

                                                                if (lastObj == null)
                                                                    break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            fine = lastObj.Modifiers.Contains("public");

                                                            if (!fine && lastObj.Modifiers.Contains("private"))
                                                                fine = context.MonoType.FullPath == (lastObj.ParentObject as MonoType)?.FullPath;

                                                            if (!fine && lastObj.Modifiers.Contains("protected") && lastObj.ParentObject is Class subObjClass)
                                                                fine = subObjClass.ContainsParent(context.MonoType as Class);

                                                            if (fine)
                                                            {
                                                                lastObj = (lastObj.Value as MonoType)?.Fields.FirstOrDefault(x => x.Name == splitFindPaths[i] && !x.Modifiers.Contains("static"));

                                                                if (lastObj != null)
                                                                    return lastObj;
                                                                else
                                                                {
                                                                    Method lastObjMethod = (lastObj.Value as MonoType)?.Methods.FirstOrDefault(x => x.Name == splitFindPaths[i] && !x.Modifiers.Contains("static") && x.Parameters.Count == methodParameters);

                                                                    fine = lastObjMethod.Modifiers.Contains("public");

                                                                    if (!fine && lastObjMethod.Modifiers.Contains("private"))
                                                                        fine = context.MonoType.FullPath == (lastObjMethod.ParentObject as MonoType)?.FullPath;

                                                                    if (fine)
                                                                        return lastObjMethod;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Method lastObjMethod = (subObj.Value as MonoType)?.Methods.FirstOrDefault(x => x.Name == splitFindPaths[2] && x.Modifiers.Contains("static") && x.Parameters.Count == methodParameters);

                                                if (lastObjMethod != null && splitFindPaths.Length == 3)
                                                {
                                                    fine = lastObjMethod.Modifiers.Contains("public");

                                                    if (!fine && lastObjMethod.Modifiers.Contains("private"))
                                                        fine = context.MonoType.FullPath == (lastObjMethod.ParentObject as MonoType)?.FullPath;

                                                    if (fine)
                                                        return lastObjMethod;
                                                }
                                            }

                                            return null;
                                        }
                                    }
                                }
                            }

                            if (splitFindPaths.Length == 2)
                            {
                                foreach (var subObj in obj.Methods.Where(x => x.Modifiers.Contains("static")))
                                {
                                    if (subObj.Name == splitFindPaths[1] && subObj.Parameters.Count == methodParameters)
                                    {
                                        fine = subObj.Modifiers.Contains("public");

                                        if (!fine && subObj.Modifiers.Contains("private"))
                                            fine = context.MonoType.FullPath == obj.FullPath;

                                        if (fine)
                                            return subObj;

                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region FindToLocalPathEnums[Enums, EnumValues]

                List<MonoEnum> monoEnums = new List<MonoEnum>();
                monoEnums.AddRange(context.ScriptFile.Enums.Where(x => context.MonoType.FullPath.StartsWith(x.Path)));

                foreach (var obj in monoEnums)
                {
                    string[] splitFindPaths = IPath.SplitPath(path);

                    if (path.StartsWith(obj.FullPath))
                        for (int i = 0; i < splitFindPaths.Length; i++)
                            if (splitFindPaths[i] == obj.Name)
                                splitFindPaths = splitFindPaths.Skip(i).ToArray();

                    if (obj.Name == path || obj.FullPath == path)
                        return obj;

                    if (splitFindPaths[0] == obj.Name)
                    {
                        if (splitFindPaths.Length == 2)
                        {
                            foreach (var subObj in obj.Values)
                            {
                                if (subObj.Name == splitFindPaths[1])
                                    return subObj;
                            }
                        }

                        return null;
                    }
                }

                #endregion

                #region FindWithUsings[Types, Enums, EnumsValue]

                monoTypes = new List<MonoType>();
                monoTypes.AddRange(context.ScriptFile.Classes);
                monoTypes.AddRange(context.ScriptFile.Structs);

                foreach (var obj in monoTypes)
                {
                    bool fine = obj.Modifiers.Contains("public");

                    if (!fine && obj.Modifiers.Contains("private"))
                        fine = IPath.ContainsWithLevelAccess(context.MonoType.FullPath, obj.FullPath);

                    if (obj.FullPath == path && fine)
                        return obj;

                    string[] splitPaths = IPath.SplitPath(path);

                    foreach (var objUsing in context.ScriptFile.Usings)
                    {
                        string newPath = null;

                        if (!string.IsNullOrEmpty(objUsing.AsName))
                        {
                            if (objUsing.AsName == splitPaths[0])
                            {
                                newPath = IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path);

                                if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                    return obj;
                            }
                        }
                        else
                        {
                            newPath = IPath.CombinePath(path, objUsing.Path);

                            if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                return obj;
                        }

                        if (newPath == null)
                            newPath = path;

                        if (fine && splitPaths.Length > 1)
                        {
                            if (newPath.StartsWith(obj.FullPath))
                            {
                                splitPaths = IPath.SplitPath(newPath.Remove(0, obj.FullPath.Length + 1));

                                Field objField = obj.Fields.FirstOrDefault(x => x.Name == splitPaths[0] && x.Modifiers.Contains("static"));

                                if (objField != null)
                                {
                                    fine = objField.Modifiers.Contains("public");

                                    if (!fine && objField.Modifiers.Contains("private"))
                                        fine = context.MonoType.FullPath == (objField.ParentObject as MonoType)?.FullPath;

                                    if (!fine && objField.Modifiers.Contains("protected") && objField.ParentObject is Class objClass)
                                        fine = objClass.ContainsParent(context.MonoType as Class);

                                    if (fine)
                                    {
                                        if (splitPaths.Length == 1)
                                            return objField;

                                        if (splitPaths.Length > 1)
                                        {
                                            objField = (objField.Value as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[1]);

                                            if (objField != null)
                                            {
                                                for (int i = 2; i < splitPaths.Length; i++)
                                                {
                                                    if (i + 1 < splitPaths.Length)
                                                    {
                                                        if (objField.Name == splitPaths[i])
                                                        {
                                                            fine = objField.Modifiers.Contains("public");

                                                            if (!fine && objField.Modifiers.Contains("private"))
                                                                fine = context.MonoType.FullPath == (objField.ParentObject as MonoType)?.FullPath;

                                                            if (!fine && objField.Modifiers.Contains("protected") && objField.ParentObject is Class subObjClass)
                                                                fine = subObjClass.ContainsParent(context.MonoType as Class);

                                                            if (fine)
                                                                objField = (objField.Value as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[i]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Method objMethod = (objField.Value as MonoType)?.Methods.FirstOrDefault(x => x.Name == splitPaths[1] && !x.Modifiers.Contains("static") && x.Parameters.Count == methodParameters);

                                                        if (objMethod != null && splitPaths.Length == 2)
                                                        {
                                                            fine = objMethod.Modifiers.Contains("public");

                                                            if (!fine && objMethod.Modifiers.Contains("private"))
                                                                fine = context.MonoType.FullPath == (objMethod.ParentObject as MonoType)?.FullPath;

                                                            if (fine)
                                                                return objMethod;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return null;
                                }
                                else
                                {
                                    Method objMethod = obj.Methods.FirstOrDefault(x => x.Name == splitPaths[0] && x.Modifiers.Contains("static") && x.Parameters.Count == methodParameters);

                                    if (objMethod != null && splitPaths.Length == 1)
                                    {
                                        fine = objMethod.Modifiers.Contains("public");

                                        if (!fine && objMethod.Modifiers.Contains("private"))
                                            fine = context.MonoType.FullPath == (objMethod.ParentObject as MonoType)?.FullPath;

                                        if (fine)
                                            return objMethod;

                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var obj in context.ScriptFile.Enums)
                {
                    if (obj.FullPath == path)
                        return obj;

                    string[] splitPaths = IPath.SplitPath(path);

                    foreach (var objUsing in context.ScriptFile.Usings)
                    {
                        string newPath = null;

                        if (!string.IsNullOrEmpty(objUsing.AsName))
                        {
                            if (objUsing.AsName == splitPaths[0])
                            {
                                newPath = IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path);

                                if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                    return obj;
                            }
                        }
                        else
                        {
                            newPath = IPath.CombinePath(path, objUsing.Path);

                            if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                return obj;
                        }

                        if (newPath != null && splitPaths.Length > 1)
                        {
                            if (newPath == obj.FullPath)
                                return obj;

                            if (newPath.StartsWith(obj.FullPath))
                            {
                                splitPaths = IPath.SplitPath(newPath.Remove(0, obj.FullPath.Length + 1));

                                return obj.Values.FirstOrDefault(x => x.Name == splitPaths[0]);
                            }
                        }
                    }
                }

                #endregion

                #region FindWithImports[Types, Enums, EnumsValue]

                foreach (var import in context.ScriptFile.Imports)
                {
                    monoTypes = new List<MonoType>();
                    monoTypes.AddRange(import.Classes);
                    monoTypes.AddRange(import.Structs);

                    foreach (var obj in monoTypes)
                    {
                        bool fine = obj.Modifiers.Contains("public");

                        if (!fine && obj.Modifiers.Contains("private"))
                            fine = IPath.ContainsWithLevelAccess(context.MonoType.FullPath, obj.FullPath);

                        if (fine && obj.FullPath == path)
                            return obj;

                        string[] splitPaths = IPath.SplitPath(path);

                        foreach (var objUsing in import.Usings)
                        {
                            string newPath = null;

                            if (!string.IsNullOrEmpty(objUsing.AsName))
                            {
                                if (objUsing.AsName == splitPaths[0])
                                {
                                    newPath = IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path);

                                    if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                        return obj;
                                }
                            }
                            else
                            {
                                newPath = IPath.CombinePath(path, objUsing.Path);

                                if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                    return obj;
                            }

                            if (newPath == null)
                                newPath = path;

                            if (splitPaths.Length > 1)
                            {
                                if (newPath.StartsWith(obj.FullPath))
                                {
                                    splitPaths = IPath.SplitPath(newPath.Remove(0, obj.FullPath.Length + 1));

                                    Field objField = obj.Fields.FirstOrDefault(x => x.Name == splitPaths[0] && x.Modifiers.Contains("static"));

                                    if (objField != null)
                                    {
                                        fine = objField.Modifiers.Contains("public");

                                        if (!fine && objField.Modifiers.Contains("private"))
                                            fine = context.MonoType.FullPath == (objField.ParentObject as MonoType)?.FullPath;

                                        if (!fine && objField.Modifiers.Contains("protected") && objField.ParentObject is Class objClass)
                                            fine = objClass.ContainsParent(context.MonoType as Class);

                                        if (fine)
                                        {
                                            if (splitPaths.Length == 1)
                                                return objField;

                                            if (splitPaths.Length > 1)
                                            {
                                                objField = (objField.Value as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[1]);

                                                if (objField != null)
                                                {
                                                    for (int i = 2; i < splitPaths.Length; i++)
                                                    {
                                                        if (i + 1 < splitPaths.Length)
                                                        {
                                                            if (objField.Name == splitPaths[i])
                                                            {
                                                                fine = objField.Modifiers.Contains("public");

                                                                if (!fine && objField.Modifiers.Contains("private"))
                                                                    fine = context.MonoType.FullPath == (objField.ParentObject as MonoType)?.FullPath;

                                                                if (!fine && objField.Modifiers.Contains("protected") && objField.ParentObject is Class subObjClass)
                                                                    fine = subObjClass.ContainsParent(context.MonoType as Class);

                                                                if (fine)
                                                                    objField = (objField.Value as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[i]);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Method objMethod = (objField.Value as MonoType)?.Methods.FirstOrDefault(x => x.Name == splitPaths[1] && !x.Modifiers.Contains("static") && x.Parameters.Count == methodParameters);

                                                            if (objMethod != null && splitPaths.Length == 2)
                                                            {
                                                                fine = objMethod.Modifiers.Contains("public");

                                                                if (!fine && objMethod.Modifiers.Contains("private"))
                                                                    fine = context.MonoType.FullPath == (objMethod.ParentObject as MonoType)?.FullPath;

                                                                if (fine)
                                                                    return objMethod;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return null;
                                    }
                                    else
                                    {
                                        Method objMethod = obj.Methods.FirstOrDefault(x => x.Name == splitPaths[0] && x.Modifiers.Contains("static") && x.Parameters.Count == methodParameters);

                                        if (objMethod != null && splitPaths.Length == 1)
                                        {
                                            fine = objMethod.Modifiers.Contains("public");

                                            if (!fine && objMethod.Modifiers.Contains("private"))
                                                fine = context.MonoType.FullPath == (objMethod.ParentObject as MonoType)?.FullPath;

                                            if (fine)
                                                return objMethod;

                                            return null;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (var obj in import.Enums)
                    {
                        string[] splitPaths = IPath.SplitPath(path);

                        foreach (var objUsing in import.Usings)
                        {
                            string newPath = null;

                            if (!string.IsNullOrEmpty(objUsing.AsName))
                            {
                                if (objUsing.AsName == splitPaths[0])
                                {
                                    newPath = IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path);

                                    if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                        return obj;
                                }
                            }
                            else
                            {
                                newPath = IPath.CombinePath(path, objUsing.Path);

                                if (splitPaths.Length == 1 && newPath == obj.FullPath)
                                    return obj;
                            }

                            if (newPath != null && splitPaths.Length > 1)
                            {
                                if (newPath == obj.FullPath)
                                    return obj;

                                if (newPath.StartsWith(obj.FullPath))
                                {
                                    splitPaths = IPath.SplitPath(newPath.Remove(0, obj.FullPath.Length + 1));

                                    var objField = obj.Values.FirstOrDefault(x => x.Name == splitPaths[1]);
                                }
                            }
                        }
                    }
                }

                #endregion
            }

            context.SearchResult = FindContextType.Empty;
            return null;
        }
    }
}
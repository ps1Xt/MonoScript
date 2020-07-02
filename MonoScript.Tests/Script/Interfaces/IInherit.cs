using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Script;
using MonoScript.Script;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Script.Interfaces
{
    public interface IInherit<TParent, TChild>
    {
        public Parent<TParent> Parent { get; set; }
        public TChild Child { get; set; }
        public void InheritParent();

        public static void InitializeInheritance(List<IInherit<TParent, TChild>> inherits)
        {
            foreach (var inherit in inherits)
                inherit.InheritParent();
        }
        public static void GetErrors(MonoType child, MonoType parent)
        {
            if (parent == null)
            {
                MLog.AppErrors.Add(new AppMessage("Parent class was not found.", $"Path {child.FullPath}"));
                return;
            }

            if (child.FullPath == parent.FullPath)
                MLog.AppErrors.Add(new AppMessage("You cannot inherit yourself.", $"Path {child.FullPath}"));

            if (parent.Modifiers.Contains("static") || parent.Modifiers.Contains("sealed"))
                MLog.AppErrors.Add(new AppMessage("You cannot inherit a class that has static or sealed modifiers.", $"Path {parent.FullPath}"));

            if (parent.Modifiers.Contains("private") && !IPath.ContainsWithLevelAccess(child.FullPath, parent.FullPath))
                MLog.AppErrors.Add(new AppMessage("The class is hidden by the private modifier.", $"Path {parent.FullPath}"));
        }
    }
}

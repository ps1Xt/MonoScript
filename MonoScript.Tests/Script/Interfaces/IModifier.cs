using MonoScript.Collections;
using MonoScript.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoScript.Script.Interfaces
{
    public interface IModifier
    {
        public List<string> Modifiers { get; set; }
        public List<string[]> AllowedModifierGroups { get; set; }
        public void AddModifiers(List<string> modifiers);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UGoap.Unity
{
    public class SubclassSelectorAttribute : PropertyAttribute
    {
        public string[] Subclasses;
        
        public SubclassSelectorAttribute(Type type)
        {
            IEnumerable<string> subclasses = new []{ "None" };
            var subtypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type))
                .Aggregate(subclasses, (s, t) => s.Append(t.FullName));

            Subclasses = subclasses.ToArray();
        }
    }
}
using UnityEditor;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using LUGoap.Learning;
using LUGoap.Unity.ScriptableObjects;

namespace LUGoap.Unity
{
    [CustomEditor(typeof(ActionConfig))]
    public class ActionEditor : ActionBaseEditor
    {
        protected override void Initialize()
        {
            // Find all subclasses of BaseClass
            _derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Base.Action))
                               && !type.IsSubclassOf(typeof(LearningAction)))
                .ToArray();

            _derivedTypeNames = new string[_derivedTypes.Length + 1];
            _derivedTypeNames[0] = "None";
            for (var i = 1; i < _derivedTypeNames.Length; i++)
            {
                var derivedType = _derivedTypes[i - 1];
                var derivedTypeName = derivedType.Name.Replace("Action", "");
                _derivedTypeNames[i] = Regex.Replace(derivedTypeName, "(?<!^)([A-Z])", " $1");
            }

            // Set the selected type index to match the currently assigned type (if any)
            if (_actionProperty.managedReferenceValue != null)
            {
                Type currentType = _actionProperty.managedReferenceValue.GetType();
                _selectedTypeIndex = Array.IndexOf(_derivedTypes, currentType) + 1;
            }
        }
    }
}
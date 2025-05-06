using UnityEditor;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using QGoap.Unity.ScriptableObjects;

namespace QGoap.Unity
{
    [CustomEditor(typeof(LearningActionConfig))]
    public class LearningActionEditor : ActionBaseEditor
    {
        protected override void Initialize()
        {
            _derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Learning.LearningAction)))
                .OrderBy(type => type.Name)
                .ToArray();

            _derivedTypeNames = new string[_derivedTypes.Length + 1];
            _derivedTypeNames[0] = "None";
            for (var i = 1; i < _derivedTypeNames.Length; i++)
            {
                var derivedType = _derivedTypes[i-1];
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

        protected override void AdditionalGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Learning", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_learningData"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_succeedReward"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_failReward"));
            
            EditorGUI.indentLevel--;
        }
    }
}
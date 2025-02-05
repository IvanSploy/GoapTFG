using UnityEditor;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using LUGoap.Learning;
using LUGoap.Unity.ScriptableObjects;
using UnityEngine;

namespace LUGoap.Unity
{
    public abstract class ActionBaseEditor : UnityEditor.Editor
    {
        protected SerializedProperty _actionProperty;
        protected Type[] _derivedTypes;
        protected string[] _derivedTypeNames;
        protected int _selectedTypeIndex;

        public void OnEnable()
        {
            _actionProperty = serializedObject.FindProperty("_actionData");
            Initialize();
        }
        
        protected abstract void Initialize();
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Generic", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cost"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_preconditions"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_effects"), true);

            EditorGUI.indentLevel--;

            AdditionalGUI();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom", EditorStyles.boldLabel);
            
            var popupStyle = new GUIStyle(EditorStyles.popup)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            
            int newSelectedIndex = EditorGUILayout.Popup(_selectedTypeIndex, _derivedTypeNames, popupStyle);
            EditorGUILayout.Space();
            
            if (newSelectedIndex != _selectedTypeIndex)
            {
                // Change instance type
                _selectedTypeIndex = newSelectedIndex;
                _actionProperty.managedReferenceValue = _selectedTypeIndex != 0
                    ? Activator.CreateInstance(_derivedTypes[_selectedTypeIndex - 1])
                    : null;
            }

            var iterator = _actionProperty.Copy();
            
            if (_actionProperty.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                if (iterator.NextVisible(true))
                {
                    EditorGUILayout.PropertyField(iterator, new GUIContent(iterator.displayName), true);
                    while (iterator.NextVisible(false))
                    {
                        EditorGUILayout.PropertyField(iterator, new GUIContent(iterator.displayName), true);
                    }
                }
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void AdditionalGUI() { }
    }
}
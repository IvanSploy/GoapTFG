using System;
using UnityEditor;
using UnityEngine;

namespace UGoap.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var subclassSelector = (SubclassSelectorAttribute)attribute;

            int currentIndex = Array.IndexOf(subclassSelector.Subclasses, property.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, subclassSelector.Subclasses);

            property.stringValue = subclassSelector.Subclasses[newIndex];
        }
    }
}
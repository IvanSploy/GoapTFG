using UnityEditor;
using UnityEngine;

namespace UGoap.Unity.Utils
{
    [CustomPropertyDrawer(typeof(SerializablePair<,>))]
    public class PairDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var keyRect = new Rect(position.x, position.y, position.width / 2, position.height);
            var valueRect = new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("Key"), GUIContent.none);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
        
        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    var container = new VisualElement();
        //    container.style.flexDirection = FlexDirection.Column;
        //    
        //    var keyField = new PropertyField(property.FindPropertyRelative("Key"));
        //    var valueField = new PropertyField(property.FindPropertyRelative("Value"));
        //    
        //    container.Add(keyField);
        //    container.Add(valueField);
        //    
        //    return container;
        //}
    }
}
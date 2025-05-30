using System;
using System.Globalization;
using QGoap.Unity.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using static QGoap.Base.BaseTypes;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity.Editor
{
    #region ScriptableObjects

    public static class GoapItemsEditor
    {
        [CustomEditor(typeof(StateConfig))]
        public class StateEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Main Data", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Properties"), true);
                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }
        }

        [CustomEditor(typeof(GoalConfig))]
        public class GoalEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                
                EditorGUILayout.LabelField("Main Data", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Properties"), true);
                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }
        }

        #endregion

        #region PropertyDrawers

        [CustomPropertyDrawer(typeof(Property))]
        public class StatePropertyDrawer : PropertyDrawer
        {
            public static float LABEL_SIZE = 50f;
            public static float PADDING = 3f;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                // No se necesita label en este
                //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("" + (int.Parse( label.text.Split(" ")[1]) + 1)));

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                //      Calculate rects
                var height = (position.height - PADDING * 2) * 0.5f;

                //First row
                var labelNameWidth = LABEL_SIZE;
                var nameWidth = position.width - labelNameWidth;

                //Second row
                var labelValueWidth = LABEL_SIZE;
                var valueWidth = position.width - labelValueWidth;

                //Rects creation
                var labelNameRect = new Rect(position.x, position.y + PADDING, labelNameWidth, height);
                var nameRect = new Rect(position.x + labelNameWidth, position.y + PADDING, nameWidth, height);
                var labelValueRect = new Rect(position.x, position.y + PADDING + height, labelNameWidth, height);
                var valueRect = new Rect(position.x + labelNameWidth, position.y + PADDING + height, valueWidth,
                    height);

                //       Draw fields
                DrawName(property, labelNameRect, nameRect);
                
                EditorGUI.LabelField(labelValueRect, "Value");
                DrawValue(property, valueRect);

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property) * 2.5f;
            }
        }

        [CustomPropertyDrawer(typeof(ConditionProperty))]
        public class ConditionPropertyDrawer : PropertyDrawer
        {
            public static float LABEL_SIZE = 50f;
            public static float PADDING = 3f;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                // No se necesita label en este
                //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("" + (int.Parse( label.text.Split(" ")[1]) + 1)));

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                //      Calculate rects
                //StandardAction
                var height = (position.height - PADDING * 2) * 0.5f;

                //First row
                var labelNameWidth = LABEL_SIZE;
                var nameWidth = position.width - labelNameWidth;

                //Second row
                var labelValueWidth = LABEL_SIZE;
                var conditionWidth = 40f;
                var valueWidth = position.width - conditionWidth - labelValueWidth;

                //Rects creation
                var labelNameRect = new Rect(position.x, position.y + PADDING, labelNameWidth, height);
                var nameRect = new Rect(position.x + labelNameWidth, position.y + PADDING, nameWidth, height);

                var labelValueRect = new Rect(position.x, position.y + PADDING + height, labelValueWidth, height);
                var conditionRect = new Rect(position.x + labelValueWidth, position.y + PADDING + height,
                    conditionWidth, height);
                var valueRect = new Rect(position.x + labelValueWidth + conditionWidth + PADDING,
                    position.y + PADDING + height,
                    valueWidth - PADDING, height);

                //      Draw fields
                DrawName(property, labelNameRect, nameRect);
                
                EditorGUI.LabelField(labelValueRect, "Value");
                
                SerializedProperty conditionProperty = property.FindPropertyRelative("condition");
                ConditionType conditionType = (ConditionType)conditionProperty.enumValueIndex;
                string[] conditionTexts =
                {
                    "=",
                    "!=",
                    "<",
                    "<=",
                    ">",
                    ">="
                };

                int selected = EditorGUI.Popup(conditionRect, (int)conditionType, conditionTexts);
                conditionProperty.enumValueIndex = selected;
                
                DrawValue(property, valueRect);

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property) * 2.5f;
            }
        }

        [CustomPropertyDrawer(typeof(EffectProperty))]
        public class EffectPropertyDrawer : PropertyDrawer
        {
            public static float LABEL_SIZE = 50f;
            public static float PADDING = 3f;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                // No se necesita label en este
                //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("" + (int.Parse( label.text.Split(" ")[1]) + 1)));

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                //      Calculate rects
                //StandardAction
                var height = (position.height - PADDING * 2) * 0.5f;

                //First row
                var labelNameWidth = LABEL_SIZE;
                var nameWidth = position.width - labelNameWidth;

                //Second row
                var labelValueWidth = LABEL_SIZE;
                var effectWidth = 35f;
                var valueWidth = position.width - effectWidth - labelValueWidth;

                //Rects creation
                var labelNameRect = new Rect(position.x, position.y + PADDING, labelNameWidth, height);
                var nameRect = new Rect(position.x + labelNameWidth, position.y + PADDING,
                    nameWidth, height);

                var labelValueRect = new Rect(position.x, position.y + PADDING + height, labelValueWidth, height);
                var effectRect = new Rect(position.x + labelValueWidth, position.y + PADDING + height,
                    effectWidth, height);
                var valueRect = new Rect(position.x + labelValueWidth + effectWidth + PADDING, position.y + PADDING + height,
                    valueWidth - PADDING, height);

                //      Draw fields
                DrawName(property, labelNameRect, nameRect);
                
                EditorGUI.LabelField(labelValueRect, "Value");
                
                SerializedProperty effectProperty = property.FindPropertyRelative("effect");
                EffectType effectType = (EffectType)effectProperty.enumValueIndex;
                string[] effectTexts =
                {
                    "=",
                    "+",
                    "-",
                    "×",
                    "÷",
                    //"%"
                };

                var style = new GUIStyle(EditorStyles.popup)
                {
                    fontSize = 17
                };

                int selected = EditorGUI.Popup(effectRect, (int)effectType, effectTexts, style);
                effectProperty.enumValueIndex = selected;
                DrawValue(property, valueRect);
                
                // Set indent back to what it was
                EditorGUI.indentLevel = indent;

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property) * 2.5f;
            }
        }

        public static void DrawName(SerializedProperty property, Rect labelNameRect, Rect nameRect)
        {
            EditorGUI.LabelField(labelNameRect, "Name");
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            
            if (!Enum.TryParse(nameProperty.stringValue, out PropertyKey name))
            {
                //Legacy
                if(int.TryParse(nameProperty.boxedValue.ToString(), out int intName))
                {
                    name = (PropertyKey)intName;
                }
            }
            nameProperty.stringValue = EditorGUI.EnumPopup(nameRect, name).ToString();
        }
        
        public static void DrawValue(SerializedProperty property, Rect valueRect)
        {
            SerializedProperty value = property.FindPropertyRelative("value");
            Enum.TryParse(property.FindPropertyRelative("name").stringValue, out PropertyKey name);
            var typeValue = GetPropertyType(name);

            switch (typeValue)
            {
                case PropertyType.Boolean:
                    value.stringValue = EditorGUI.Toggle(valueRect, (bool)ParseValue(name, value.stringValue))
                        .ToString();
                    break;
                case PropertyType.Integer:
                    value.stringValue = EditorGUI.IntField(valueRect, (int)ParseValue(name, value.stringValue))
                        .ToString();
                    break;
                case PropertyType.Float:
                    value.stringValue = EditorGUI.FloatField(valueRect, (float)ParseValue(name, value.stringValue))
                        .ToString(CultureInfo.InvariantCulture);
                    break;
                case PropertyType.Enum:
                    int selectedIndex = 0;
                    for (int i = 0; i < EnumNames[name].Length; i++)
                    {
                        if (EnumNames[name][i].Equals(value.stringValue))
                        {
                            selectedIndex = i;
                            break;
                        }
                    }
                    value.stringValue = EnumNames[name][EditorGUI.Popup(valueRect, selectedIndex, EnumNames[name])];
                    break;
                case PropertyType.String:
                default:
                    value.stringValue = EditorGUI.TextField(valueRect, value.stringValue);
                    break;
            }
        }
        
        #endregion
    }
}
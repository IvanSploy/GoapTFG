using System.Globalization;
using GoapTFG.UGoap.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using static GoapTFG.Base.BaseTypes;
using static GoapTFG.UGoap.CodeGenerator.EnumGenerator;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Editor
{
    #region ScriptableObjects

    public static class UGoapItemEditor
    {
        [CustomEditor(typeof(UGoapState))]
        public class StateEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Main Data", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stateProperties"), true);
                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }
        }

        [CustomEditor(typeof(UGoapGoal))]
        public class GoalEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Auto Generate Names");
                UGoapGoal.GenerateGoalNames = EditorGUILayout.Toggle(UGoapGoal.GenerateGoalNames);

                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("Generate Goal Names"))
                {
                    CreateGoalEnum();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Main Data", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("goalProperties"), true);
                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }
        }

        [CustomEditor(typeof(UGoapActionBase), true)]
        public class ActionEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                UGoapActionBase actionScriptableObject = (UGoapActionBase)target;

                EditorGUILayout.LabelField("Action Data", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                actionScriptableObject.SetCost(EditorGUILayout.IntField("Cost", actionScriptableObject.GetCost()));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("preconditions"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);

                //Guarda los cambios realizados en los property fields.
                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }
        }

        #endregion

        #region PropertyDrawers

        [CustomPropertyDrawer(typeof(UGoapPropertyManager.Property))]
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
                EditorGUI.LabelField(labelNameRect, "Name");
                EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
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

        [CustomPropertyDrawer(typeof(UGoapPropertyManager.ConditionProperty))]
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
                //Generic
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
                EditorGUI.LabelField(labelNameRect, "Name");
                EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
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

        [CustomPropertyDrawer(typeof(UGoapPropertyManager.EffectProperty))]
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
                //Generic
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
                EditorGUI.LabelField(labelNameRect, "Name");
                EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
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
                    "%"
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

        public static void DrawValue(SerializedProperty property, Rect valueRect)
        {
            SerializedProperty value = property.FindPropertyRelative("value");
            UGoapPropertyManager.PropertyList name = (UGoapPropertyManager.PropertyList)property.FindPropertyRelative("name").enumValueIndex;
            var typeValue = UGoapPropertyManager.GetType(name);

            switch (typeValue)
            {
                case UGoapPropertyManager.PropertyType.Boolean:
                    value.stringValue = EditorGUI.Toggle(valueRect, (bool)ParseValue(name, value.stringValue))
                        .ToString();
                    break;
                case UGoapPropertyManager.PropertyType.Integer:
                    value.stringValue = EditorGUI.IntField(valueRect, (int)ParseValue(name, value.stringValue))
                        .ToString();
                    break;
                case UGoapPropertyManager.PropertyType.Float:
                    value.stringValue = EditorGUI.FloatField(valueRect, (float)ParseValue(name, value.stringValue))
                        .ToString(CultureInfo.InvariantCulture);
                    break;
                case UGoapPropertyManager.PropertyType.Enum:
                    var rangeValues = new int[EnumNames[name].Length];
                    for (var i = 0; i < rangeValues.Length; i++) { rangeValues[i] = i; }
                    value.stringValue = EditorGUI.IntPopup(valueRect, (int)ParseValue(name, value.stringValue),
                        EnumNames[name], rangeValues).ToString();
                    break;
                case UGoapPropertyManager.PropertyType.String:
                default:
                    value.stringValue = EditorGUI.TextArea(valueRect, value.stringValue);
                    break;
            }
        }
        
        #endregion
    }
}
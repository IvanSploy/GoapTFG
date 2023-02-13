using UnityEditor;
using UnityEngine;
using static GoapTFG.Unity.GoapItemEditor;


namespace GoapTFG.Unity
{
    #region ScriptableObjects
    
    [CustomEditor(typeof(StateScriptableObject))]
    public class GoapItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            StateScriptableObject stateScriptableObject = (StateScriptableObject)target;
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Main Data", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
        
            stateScriptableObject.stateName = EditorGUILayout.TextField("Name", stateScriptableObject.stateName);
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stateProperties"), true);
            serializedObject.ApplyModifiedProperties();
        
            EditorGUI.indentLevel--;
        }
    }
    
    [CustomEditor(typeof(GoalScriptableObject))]
    public class GoalEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GoalScriptableObject goalScriptableObject = (GoalScriptableObject)target;
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Main Data", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
        
            goalScriptableObject.goalName = EditorGUILayout.TextField("Name", goalScriptableObject.goalName);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("goalProperties"), true);
            serializedObject.ApplyModifiedProperties();
        
            EditorGUI.indentLevel--;
        }
    }
    
    [CustomEditor(typeof(ActionScriptableObject))]
    public class ActionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ActionScriptableObject actionScriptableObject = (ActionScriptableObject)target;
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Action Data", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
        
            actionScriptableObject.actionName = EditorGUILayout.TextField("Name", actionScriptableObject.actionName);
            actionScriptableObject.cost = EditorGUILayout.IntField("Cost", actionScriptableObject.cost);
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

    [CustomPropertyDrawer(typeof(PropertyManager.Property))]
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
            var valueRect = new Rect(position.x + labelNameWidth, position.y + PADDING + height, valueWidth, height);

            //       Draw fields
            EditorGUI.LabelField(labelNameRect, "Name");
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.LabelField(labelValueRect, "Value");
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    
        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) * 2.5f;
        }
    }
    
    [CustomPropertyDrawer(typeof(PropertyManager.ConditionProperty))]
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
            var conditionRect = new Rect(position.x + labelValueWidth, position.y + PADDING + height, conditionWidth, height);
            var valueRect = new Rect(position.x + labelValueWidth + conditionWidth + PADDING, position.y + PADDING + height,
                valueWidth - PADDING, height);

            //      Draw fields
            EditorGUI.LabelField(labelNameRect, "Name");
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.LabelField(labelValueRect, "Value");
            EditorGUI.PropertyField(conditionRect, property.FindPropertyRelative("condition"), GUIContent.none);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    
        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) * 2.5f;
        }
    }
    
    [CustomPropertyDrawer(typeof(PropertyManager.EffectProperty))]
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
            var effectWidth = 50f;
            var valueWidth = position.width - effectWidth - labelValueWidth;
            
            //Rects creation
            var labelNameRect = new Rect(position.x, position.y + PADDING, labelNameWidth, height);
            var nameRect = new Rect(position.x + labelNameWidth, position.y + PADDING, nameWidth, height);
            
            var labelValueRect = new Rect(position.x, position.y + PADDING + height, labelValueWidth, height);
            var effectRect = new Rect(position.x + labelValueWidth, position.y + PADDING + height, effectWidth, height);
            var valueRect = new Rect(position.x + labelValueWidth + effectWidth + PADDING, position.y + PADDING + height,
                valueWidth - PADDING, height);

            //      Draw fields
            EditorGUI.LabelField(labelNameRect, "Name");
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.LabelField(labelValueRect, "Value");
            EditorGUI.PropertyField(effectRect, property.FindPropertyRelative("effect"), GUIContent.none);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    
        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) * 2.5f;
        }
    }
    
    #endregion
}
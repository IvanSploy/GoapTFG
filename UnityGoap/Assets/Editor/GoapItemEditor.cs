using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static GoapItem.ItemType;

[CustomEditor(typeof(GoapItem))]
public class GoapItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GoapItem goapItem = (GoapItem)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Main Data", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        
        goapItem.nameItem = EditorGUILayout.TextField("Name", goapItem.nameItem);

        switch (goapItem.type)
        {
            case State:
            default:
                DrawState();
                break;
            case Goal:
                DrawGoal(goapItem);
                break;
            case Action:
                DrawAction(goapItem);
                break;
        }
        
        EditorGUI.indentLevel--;
    }

    void DrawState()
    {
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stateProperties"), true);
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawGoal(GoapItem goapItem)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Priority Level");
            goapItem.priority = EditorGUILayout.Slider(goapItem.priority, 0, 20);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goalProperties"), true);
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawAction(GoapItem goapItem)
    {
        goapItem.cost = EditorGUILayout.IntField("Cost", goapItem.cost);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("preconditions"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);
        
        //Guarda los cambios realizados en los property fields.
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(PropertyManager.Property))]
public class PGPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // No se necesita label en este
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("" + (int.Parse( label.text.Split(" ")[1]) + 1)));

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var padding = 3;
        var newHeight = (position.height - padding * 2) * 0.5f;
        var nameRect = new Rect(position.x, position.y + padding, position.width, newHeight);
        var valueRect = new Rect(position.x, position.y + padding + newHeight, position.width,
            newHeight);

        // Draw fields
        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"));
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"));

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property) * 2.5f;
    }
}
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UGoap.Unity.Utils
{
    [CustomPropertyDrawer(typeof(SerializablePair<,>))]
    public class PairDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            
            var keyField = new PropertyField(property.FindPropertyRelative("Key"));
            keyField.label = "";
            keyField.style.marginRight = new StyleLength(new Length(1, LengthUnit.Percent));
            keyField.style.width = new StyleLength(new Length(50, LengthUnit.Percent));
            
            var valueField = new PropertyField(property.FindPropertyRelative("Value"));
            valueField.label = "";
            valueField.style.width = new StyleLength(new Length(49, LengthUnit.Percent));
            
            container.Add(keyField);
            container.Add(valueField);
            
            return container;
        }
    }
}
using System;
using System.Collections.Generic;
using static UGoap.Base.BaseTypes;

namespace UGoap.Base
{
    public static partial class UGoapPropertyManager
    {
        #region PropertyDefinitions
        [Serializable]
        public enum PropertyType
        {
            Boolean = 0,
            Integer = 1,
            Float = 2,
            String = 3,
            Enum = 4,
        }
        
        [Serializable]
        public class Property {
            public PropertyKey name;
            public string value;

            public Property(PropertyKey name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }

        [Serializable]
        public class ConditionProperty : Property{
            public ConditionType condition;

            public ConditionProperty(PropertyKey name, ConditionType condition, string value) : base(name, value)
            {
                this.condition = condition;
            }
        }
        
        [Serializable]
        public class EffectProperty : Property {
            public EffectType effect;

            public EffectProperty(PropertyKey name, EffectType effect, string value) : base(name, value)
            {
                this.effect = effect;
            }
        }
        #endregion

        #region Getters

        public static PropertyType GetPropertyType(PropertyKey property)
        {
            return PropertyTypes[property];
        }
        
        #endregion
        
        #region Parsers

        public static object ParseValue(PropertyKey name, string value)
        {
            object result;
            var type = GetPropertyType(name);
            switch (type)
            {
                case PropertyType.Boolean:
                    try
                    {
                        result = bool.Parse(value);
                    }
                    catch (FormatException)
                    {
                        result = false;
                    }
                    break;
                //case PropertyType.Enum:
                case PropertyType.Integer:
                    try
                    {
                        result = int.Parse(value);
                    }
                    catch (FormatException)
                    {
                        result = 0;
                    }
                    break;
                case PropertyType.Float:
                    try
                    {
                        result = float.Parse(value.Replace(".", ","));
                    }
                    catch(FormatException)
                    {
                        result = 0f;
                    }
                    break;
                case PropertyType.Enum:
                case PropertyType.String:
                default:
                    result = value;
                    break;
            }
            return result;
        }
        
        private static object ParseValue(Property prop)
        {
            var name = prop.name;
            var value = prop.value;
            return ParseValue(name, value);
        }

        #endregion

        #region Usos externos
        
        /// <summary>
        /// Converts a Property into a value inside a State.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="goapState">State that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<Property> properties, in GoapState goapState)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, in goapState);
            }
        }
    
        
        /// <summary>
        /// Converts a ConditionProperty into a value inside a State.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">State that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<ConditionProperty> properties, in GoapConditions state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, in state);
            }
        }
    
        /// <summary>
        /// Converts an EffectProperty into a value inside a State.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">State that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<EffectProperty> properties, in GoapEffects state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, in state);
            }
        }
        
        #endregion

        #region Converters
        private static void ApplyProperty(Property property, in GoapState pg)
        {
            pg.Set(property.name, ParseValue(property));
        }
        
        private static void ApplyProperty(ConditionProperty property, in GoapConditions pg)
        {
            pg.Set(property.name, new ConditionValue(ParseValue(property), property.condition));
        }

        private static void ApplyProperty(EffectProperty property, in GoapEffects pg)
        {
            pg.Set(property.name, new EffectValue(ParseValue(property), property.effect));
        } 
        #endregion
    }
}

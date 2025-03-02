using System;
using System.Collections.Generic;
using static LUGoap.Base.BaseTypes;

namespace LUGoap.Base
{
    public static partial class PropertyManager
    {
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
            public string name;
            public string value;

            public Property(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }

        [Serializable]
        public class ConditionProperty : Property{
            public ConditionType condition;

            public ConditionProperty(string name, ConditionType condition, string value) : base(name, value)
            {
                this.condition = condition;
            }
        }
        
        [Serializable]
        public class EffectProperty : Property {
            public EffectType effect;

            public EffectProperty(string name, EffectType effect, string value) : base(name, value)
            {
                this.effect = effect;
            }
        }
        
        public static PropertyType GetPropertyType(PropertyKey property)
        {
            if (property == PropertyKey.None) return PropertyType.Boolean;
            return PropertyTypes[property];
        }
        
        public static Type GetType(PropertyKey property)
        {
            var pType = GetPropertyType(property);
            return GetType(pType);
        }

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
        
        private static Type GetType(PropertyType pType)
        {
            return pType switch
            {
                PropertyType.Boolean => typeof(bool),
                PropertyType.Integer => typeof(int),
                PropertyType.Float => typeof(float),
                PropertyType.String => typeof(string),
                PropertyType.Enum => typeof(string),
                _ => typeof(bool)
            };
        }
        
        private static PropertyKey ParseName(Property prop)
        {
            return ParseName(prop.name);
        }
        
        private static PropertyKey ParseName(string name)
        {
            Enum.TryParse(name, out PropertyKey key);
            return key;
        }
        
        private static object ParseValue(Property prop)
        {
            var name = ParseName(prop.name);
            var value = prop.value;
            return ParseValue(name, value);
        }

        #region Utilities
        
        /// <summary>
        /// Converts a Property into a value inside a State.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">State that will include the new Property.</param>
        public static void ApplyProperties(this State state, List<Property> properties)
        {
            foreach (var property in properties)
            {
                state.ApplyProperty(property);
            }
        }
    
        
        /// <summary>
        /// Converts a ConditionProperty into a value inside a State.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="group">State that will include the new Property.</param>
        public static void ApplyProperties(this ConditionGroup group, List<ConditionProperty> properties)
        {
            foreach (var property in properties)
            {
                group.ApplyProperty(property);
            }
        }
    
        /// <summary>
        /// Converts an EffectProperty into a value inside a State.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="group">State that will include the new Property.</param>
        public static void ApplyProperties(this EffectGroup group, List<EffectProperty> properties)
        {
            foreach (var property in properties)
            {
                group.ApplyProperty(property);
            }
        }
        
        #endregion

        #region Converters
        private static void ApplyProperty(this State pg, Property property)
        {
            pg.Set(ParseName(property), ParseValue(property));
        }
        
        private static void ApplyProperty(this ConditionGroup pg, ConditionProperty property)
        {
            pg.Set(ParseName(property), property.condition, ParseValue(property));
        }

        private static void ApplyProperty(this EffectGroup pg, EffectProperty property)
        {
            pg.Set(ParseName(property), new Effect(ParseValue(property), property.effect));
        } 
        #endregion
    }
}

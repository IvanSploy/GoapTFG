using System;
using System.Collections.Generic;
using static QGoap.Base.BaseTypes;

namespace QGoap.Base
{
    public static partial class PropertyManager
    {
        [Serializable]
        public class Property {
            public string name;
            public string value;

            public Property(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
            
            public void Apply(State state)
            {
                var key = ParseName(name);
                state.Set(key, ParseValue(key, value));
            }
        }

        [Serializable]
        public class ConditionProperty : Property{
            public ConditionType condition;

            public ConditionProperty(string name, ConditionType condition, string value) : base(name, value)
            {
                this.condition = condition;
            }
            
            public void Apply(ConditionGroup conditions)
            {
                var key = ParseName(name);
                conditions.Set(key, condition, ParseValue(key, value));
            }
        }
        
        [Serializable]
        public class EffectProperty : Property {
            public EffectType effect;

            public EffectProperty(string name, EffectType effect, string value) : base(name, value)
            {
                this.effect = effect;
            }
            
            public void Apply(EffectGroup effects)
            {
                var key = ParseName(name);
                effects.Set(key, effect, ParseValue(key, value));
            }
        }
        
        public static Type GetType(PropertyKey key)
        {
            var pType = GetPropertyType(key);
            return GetType(pType);
        }
        
        public static PropertyType GetPropertyType(PropertyKey key)
        {
            if (key == PropertyKey.None) return PropertyType.Boolean;
            return PropertyTypes[key];
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
        
        private static PropertyKey ParseName(string name)
        {
            Enum.TryParse(name, out PropertyKey key);
            return key;
        }
        
        public static object ParseValue(PropertyKey key, string value)
        {
            object result;
            var type = GetPropertyType(key);
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
                property.Apply(state);
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
                property.Apply(group);
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
                property.Apply(group);
            }
        }
        
        #endregion
    }
}

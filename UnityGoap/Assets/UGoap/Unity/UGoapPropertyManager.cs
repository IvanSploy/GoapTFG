using System;
using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.BaseTypes;

namespace UGoap.Unity
{
    public static class UGoapPropertyManager
    {
        //CONFIGURACIÓN DE LAS PROPIEDADES DEL EJEMPLO
        [Serializable]
        public enum PropertyKey {
            Target,
            Seeds,
            Fish,
            Money,
            Happiness,
            Fatigue,
            MentalState
        }
        
        [Serializable]
        public enum PropertyType
        {
            Boolean = 0,
            Integer = 1,
            Float = 2,
            String = 3,
            Enum = 4
        }

        private static readonly Dictionary<PropertyKey, PropertyType> PropertyTypes = new()
        {
            { PropertyKey.Target, PropertyType.String },
            { PropertyKey.Seeds, PropertyType.Integer },
            { PropertyKey.Fish, PropertyType.Integer },
            { PropertyKey.Money, PropertyType.Float },
            { PropertyKey.Happiness, PropertyType.Float },
            { PropertyKey.Fatigue, PropertyType.Float },
            { PropertyKey.MentalState, PropertyType.Enum }        
        };

        public static Dictionary<PropertyKey, string[]> EnumNames = new()
        {
            { PropertyKey.MentalState, new [] { "Good", "Tired", "Burnout" }} 
        };

        #region PropertyDefinitions
        
        //CONFIGURACIÓN PROPIEDADES 
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
                case PropertyType.Integer:
                case PropertyType.Enum:
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
        /// Converts a Property into a value inside a StateGroup.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">StateGroup that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<Property> properties, in StateGroup<PropertyKey, object> state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, in state);
            }
        }
    
        
        /// <summary>
        /// Converts a ConditionProperty into a value inside a StateGroup.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">StateGroup that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<ConditionProperty> properties, in ConditionGroup<PropertyKey, object> state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, in state);
            }
        }
    
        /// <summary>
        /// Converts an EffectProperty into a value inside a StateGroup.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">StateGroup that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<EffectProperty> properties, in EffectGroup<PropertyKey, object> state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, in state);
            }
        }
        
        #endregion

        #region Converters
        private static void ApplyProperty(Property property, in StateGroup<PropertyKey, object> pg)
        {
            pg.Set(property.name, new GoapValue<object>(ParseValue(property)));
        }
        
        private static void ApplyProperty(ConditionProperty property, in ConditionGroup<PropertyKey, object> pg)
        {
            pg.Set(property.name, new ConditionValue<object>(ParseValue(property), property.condition));
        }

        private static void ApplyProperty(EffectProperty property, in EffectGroup<PropertyKey, object> pg)
        {
            pg.Set(property.name, new EffectValue<object>(ParseValue(property), property.effect));
        } 
        #endregion
    }
}

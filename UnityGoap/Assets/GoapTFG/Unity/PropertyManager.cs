using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Base.BaseTypes;

namespace GoapTFG.Unity
{
    public static class PropertyManager
    {
        //CONFIGURACIÓN DE LAS PROPIEDADES DEL EJEMPLO
        [Serializable]
        public enum PropertyList {
            WoodCount,
            StoneCount,
            GoldCount,
            Target,
            StateOfTarget,
            IsIdle
        }

        private static readonly Dictionary<PropertyList, PropertyType> ProperTypes = new()
        {
            { PropertyList.WoodCount, PropertyType.Integer },
            { PropertyList.StoneCount, PropertyType.Integer },
            { PropertyList.GoldCount, PropertyType.Float },
            { PropertyList.Target, PropertyType.String },
            { PropertyList.StateOfTarget, PropertyType.TargetState },
            { PropertyList.IsIdle, PropertyType.Boolean }
        };
        
        [Serializable]
        public enum PropertyType
        {
            Boolean = 0,
            Integer = 1,
            Float = 2,
            String = 3,
            TargetState = 4
        }

        public static string[] TargetStateNames = 
            { 
                "Reached",
                "Going",
                "Ready" 
            };
        
        #region Getters
        
        private static PropertyType GetType(Property property)
        {
            return ProperTypes[property.name];
        }
        
        public static PropertyType GetType(PropertyList property)
        {
            return ProperTypes[property];
        }
        
        #endregion

        #region PropertyDefinitions
        
        //CONFIGURACIÓN PROPIEDADES 
        [Serializable]
        public class Property {
            public PropertyList name;
            public string value;

            public Property(PropertyList name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }

        [Serializable]
        public class ConditionProperty : Property{
            public ConditionType condition;

            public ConditionProperty(PropertyList name, ConditionType condition, string value) : base(name, value)
            {
                this.condition = condition;
            }
        }
        
        [Serializable]
        public class EffectProperty : Property {
            public EffectType effect;

            public EffectProperty(PropertyList name, EffectType effect, string value) : base(name, value)
            {
                this.effect = effect;
            }
        }
        #endregion
        
        #region Parsers

        // ReSharper disable Unity.PerformanceAnalysis
        public static object ParseValue(PropertyList name, string value)
        {
            object result;
            var type = GetType(name);
            switch (type)
            {
                case PropertyType.Boolean:
                    try
                    {
                        result = bool.Parse(value);
                    }
                    catch (FormatException e)
                    {
                        Debug.LogError(e.Message);
                        result = false;
                    }
                    break;
                case PropertyType.Integer:
                case PropertyType.TargetState:
                    try
                    {
                        result = int.Parse(value);
                    }
                    catch (FormatException e)
                    {
                        Debug.LogError(e.Message);
                        result = 0;
                    }
                    break;
                case PropertyType.Float:
                    try
                    {
                        result = float.Parse(value.Replace(".", ","));
                    }
                    catch(FormatException e)
                    {
                        Debug.LogError(e.Message);
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
        /// Converts a Property into a value inside a PropertyGroup.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">PropertyGroup that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<Property> properties, ref PropertyGroup<PropertyList, object> state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, ref state);
            }
        }
    
        
        /// <summary>
        /// Converts a ConditionProperty into a value inside a PropertyGroup.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">PropertyGroup that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<ConditionProperty> properties, ref PropertyGroup<PropertyList, object> state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, ref state);
            }
        }
    
        /// <summary>
        /// Converts an EffectProperty into a value inside a PropertyGroup.
        /// </summary>
        /// <param name="properties">Property to be converted.</param>
        /// <param name="state">PropertyGroup that will include the new Property.</param>
        public static void AddIntoPropertyGroup(List<EffectProperty> properties, ref PropertyGroup<PropertyList, object> state)
        {
            foreach (var property in properties)
            {
                ApplyProperty(property, ref state);
            }
        }
        
        #endregion

        #region Converters
        private static void ApplyProperty(Property property, ref PropertyGroup<PropertyList, object> pg)
        {
            pg.Set(property.name, ParseValue(property));
        }
        
        private static void ApplyProperty(ConditionProperty property, ref PropertyGroup<PropertyList, object> pg)
        {
            pg.Set(property.name, ParseValue(property), property.condition);
        }

        private static void ApplyProperty(EffectProperty property, ref PropertyGroup<PropertyList, object> pg)
        {
            pg.Set(property.name, ParseValue(property), property.effect);
        } 
        #endregion
    }
}

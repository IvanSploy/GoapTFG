using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;

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
            Active
        }

        private static readonly Dictionary<PropertyList, PropertyType> ProperTypes = new()
        {
            { PropertyList.WoodCount, PropertyType.Integer },
            { PropertyList.StoneCount, PropertyType.Integer },
            { PropertyList.GoldCount, PropertyType.Float },
            { PropertyList.Target, PropertyType.String },
            { PropertyList.StateOfTarget, PropertyType.TargetState },
            { PropertyList.Active, PropertyType.Boolean }
        };
        
        [Serializable]
        public enum PropertyType
        {
            Boolean = 0,
            Integer = 1,
            Float = 2,
            TargetState = 3,
            String = 9
        }

        public static string[] TargetStateNames = new[]
        {
            "Reached",
            "Going",
            "Ready"
        };
        
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<Goal<PropertyList, object>, PropertyGroup<PropertyList, object>, int> GetCustomHeuristic()
        {
            return (goal, worldState) =>
            {
                var heuristic = 0;
                foreach (var name in goal.GetState().GetKeys())
                {
                    if(!worldState.HasConflict(name, goal.GetState())) continue;
                    switch (GetType(name))
                    {
                        case PropertyType.Integer:
                            if (worldState.HasKey(name))
                                heuristic += Math.Abs((int)goal.GetState().Get(name) - (int)worldState.Get(name));
                            else heuristic += (int)goal.GetState().Get(name);
                            break;
                        case PropertyType.Float:
                            if (worldState.HasKey(name))
                                heuristic += (int)Mathf.Abs((float)goal.GetState().Get(name) - (float)worldState.Get(name));
                            else heuristic += (int)goal.GetState().Get(name);
                            break;
                        default:
                            if (!worldState.HasKey(name) || !goal.GetState().Get(name).Equals(worldState.Get(name))) 
                                heuristic += 1;
                            break;
                    }
                }
                return heuristic;
            };
            return null;
        }
        
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
        
        //CONFIGURACIÓN PREDICADOS 
        [Serializable]
        public enum ConditionList {
            Eq,
            Ne,
            Lt,
            Le,
            Gt,
            Ge
        }
        
        [Serializable]
        public enum EffectList {
            Set,
            Add,
            Sub,
            Mul,
            Div,
            Mod
        }
        
        [Serializable]
        public class Property {
            public PropertyList name;
            
            public bool boolValue;
            public int intValue;
            public float floatValue;
            public string stringValue;

            public Property(PropertyList name)
            {
                this.name = name;
            }
        }

        [Serializable]
        public class ConditionProperty : Property{
            public ConditionList condition;

            public ConditionProperty(PropertyList name, ConditionList condition) : base(name)
            {
                this.condition = condition;
            }
        }
        
        [Serializable]
        public class EffectProperty : Property {
            public EffectList effect;

            public EffectProperty(PropertyList name, EffectList effect) : base(name)
            {
                this.effect = effect;
            }
        }
        #endregion
        
        #region Parsers

        /*private static object ParseValue(Property prop)
        {
            object result;
            var type = GetType(prop);
            var value = prop.value;
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
        }*/
        
        private static Func<object, object, bool> ParseCondition(ConditionProperty prop)
        {
            Func<object, object, bool> result;
            var condition = prop.condition;
            var type = GetType(prop);
            switch (condition)
            {
                case ConditionList.Eq:
                default:
                    //Por defecto no es necesario ningún predicado independiente del tipo.
                    result = null;
                    break;
                case ConditionList.Ne:
                    result = (a, b) => !a.Equals(b);
                    break;
                case ConditionList.Lt:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a < (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a < (float)b;
                            break;
                        case PropertyType.String:
                            result = (a, b) => String.Compare((string)a, (string)b, StringComparison.Ordinal) < 0;
                            break;
                    }
                    break;
                case ConditionList.Gt:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a > (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a > (float)b;
                            break;
                        case PropertyType.String:
                            result = (a, b) => String.Compare((string)a, (string)b, StringComparison.Ordinal) > 0;
                            break;
                    }
                    break;
                case ConditionList.Le:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a <= (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) =>
                            {
                                bool test = (float)a <= (float)b;
                                return test;
                            };
                            break;
                        case PropertyType.String:
                            result = (a, b) => String.Compare((string)a, (string)b, StringComparison.Ordinal) <= 0;
                            break;
                    }
                    break;
                case ConditionList.Ge:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a >= (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a >= (float)b;
                            break;
                        case PropertyType.String:
                            result = (a, b) => String.Compare((string)a, (string)b, StringComparison.Ordinal) >= 0;
                            break;
                    }
                    break;
            }
            return result;
        }
        
        private static Func<object, object, object> ParseEffect(EffectProperty prop)
        {
            Func<object, object, object> result;
            var effect = prop.effect;
            var type = GetType(prop);
            switch (effect)
            {
                case EffectList.Set:
                default:
                    //Por defecto no es necesario ningún predicado independiente del tipo.
                    result = null;
                    break;
                case EffectList.Add:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a + (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a + (float)b;
                            break;
                        case PropertyType.String:
                            result = (a, b) => (string)a + "\n" + (string)b;
                            break;
                    }
                    break;
                case EffectList.Sub:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a - (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a - (float)b;
                            break;
                        case PropertyType.String:
                            result = (a, b) => ((string)a).Replace((string)b, "");
                            break;
                    }
                    break;
                case EffectList.Mul:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a * (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a * (float)b;
                            break;
                        case PropertyType.String:
                            result = null;
                            break;
                    }
                    break;
                case EffectList.Div:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a / (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a / (float)b;
                            break;
                        case PropertyType.String:
                            result = null;
                            break;
                    }
                    break;
                case EffectList.Mod:
                    switch (type)
                    {
                        case PropertyType.Boolean:
                        case PropertyType.TargetState:
                        default:
                            result = null;
                            break;
                        case PropertyType.Integer:
                            result = (a, b) => (int)a % (int)b;
                            break;
                        case PropertyType.Float:
                            result = (a, b) => (float)a % (float)b;
                            break;
                        case PropertyType.String:
                            result = null;
                            break;
                    }
                    break;
            }
            return result;
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
            pg.Set(property.name, GetValue(property));
        }
        
        private static void ApplyProperty(ConditionProperty property, ref PropertyGroup<PropertyList, object> pg)
        {
            var predicate = ParseCondition(property);
            pg.Set(property.name, GetValue(property), predicate);
        }

        private static void ApplyProperty(EffectProperty property, ref PropertyGroup<PropertyList, object> pg)
        {
            var predicate = ParseEffect(property);
            pg.Set(property.name, GetValue(property), predicate);
        }

        private static object GetValue(Property property)
        {
            switch (GetType(property.name))
            {
                case PropertyType.Boolean:
                    return property.boolValue;
                case PropertyType.Integer:
                case PropertyType.TargetState:
                    return property.intValue;
                case PropertyType.Float:
                    return property.floatValue;
                case PropertyType.String:
                default:
                    return property.stringValue;
            }
        }
        #endregion
    }
}

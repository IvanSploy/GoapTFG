﻿using System;
using Unity.VisualScripting;

namespace UGoap.Base
{
    public static class BaseTypes
    {
        [Serializable]
        public enum ConditionType {
            Equal,
            NotEqual,
            LessThan,
            LessOrEqual,
            GreaterThan,
            GreaterOrEqual
        }
        
        [Serializable]
        public enum EffectType {
            Set,
            Add,
            Subtract,
            Multiply,
            Divide
        }
        
        public static bool Evaluate(this ConditionValue conditionValue, object a)
        {
            return Evaluate(a, conditionValue.ConditionType, conditionValue.Value);
        }
        
        public static bool Evaluate(object a, ConditionType condition, object b)
        {
            if (a.GetType() != b.GetType()) return false;
            
            IComparable comparableA = a as IComparable;
            IComparable comparableB = b as IComparable;
            
            if (comparableA == null || comparableB == null)
            {
                var equals = a.Equals(b);
                return condition == ConditionType.Equal ? equals : !equals;
            }
            
            bool result;
            switch (condition)
            {
                case ConditionType.Equal:
                default:
                    result = a.Equals(b);
                    break;
                case ConditionType.NotEqual:
                    result = !a.Equals(b);
                    break;
                case ConditionType.LessThan:
                    result = comparableA.CompareTo(comparableB) < 0;
                    break;
                case ConditionType.GreaterThan:
                    result = comparableA.CompareTo(comparableB) > 0;
                    break;
                case ConditionType.LessOrEqual:
                    result = comparableA.CompareTo(comparableB) <= 0;
                    break;
                case ConditionType.GreaterOrEqual:
                    result = comparableA.CompareTo(comparableB) >= 0;
                    break;
            }
            return result;
        }


        public static object Evaluate(this EffectValue effectValue, object a)
        {
            return Evaluate(a, effectValue.EffectType, effectValue.Value);
        }
        
        public static object Evaluate(object a, EffectType effect, object b)
        {
            if (a.GetType() != b.GetType())
                throw new ArgumentException("Evaluated properties doesnt have the same types. Check the assigns.");
            
            object result;
            switch (effect)
            {
                case EffectType.Set:
                default:
                    result = b;
                    break;
                case EffectType.Add:
                    result = a switch
                    {
                        int i => i + (int)b,
                        float f => f + (float)b,
                        string s => s + "\n" + (string)b,
                        _ => b
                    };
                    break;
                case EffectType.Subtract:
                    result = a switch
                    {
                        int i => i - (int)b,
                        float f => f - (float)b,
                        string s => s.Replace((string)b, ""),
                        _ => b
                    };
                    break;
                case EffectType.Multiply:
                    result = a switch
                    {
                        int i => i * (int)b,
                        float f => f * (float)b,
                        _ => b
                    };
                    break;
                case EffectType.Divide:
                    result = a switch
                    {
                        int i => i / (int)b,
                        float f => f / (float)b,
                        _ => b
                    };
                    break;
            }
            return result;
        }
        
        public static object GetDefault(this object value)
        {
            object defaultValue = value switch
            {
                int => 0,
                long => 0,
                float => 0f,
                double => 0d,
                string => "",
                _ => null,
            };
            return defaultValue;
        }
    }
}
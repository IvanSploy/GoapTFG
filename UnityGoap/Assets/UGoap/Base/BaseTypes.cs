using System;
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
        
        public static bool EvaluateCondition(object a, object b, ConditionType condition)
        {
            if (a.GetType() != b.GetType()) return false;
            
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
                    result = a switch
                    {
                        int i => i < (int)b,
                        float f => f < (float)b,
                        string s => string.Compare(s, (string)b, StringComparison.Ordinal) < 0,
                        _ => a.Equals(b)
                    };
                    break;
                case ConditionType.GreaterThan:
                    result = a switch
                    {
                        int i => i > (int)b,
                        float f => f > (float)b,
                        string s => string.Compare(s, (string)b, StringComparison.Ordinal) > 0,
                        _ => a.Equals(b)
                    };
                    break;
                case ConditionType.LessOrEqual:
                    result = a switch
                    {
                        int i => i <= (int)b,
                        float f => f <= (float)b,
                        string s => string.Compare(s, (string)b, StringComparison.Ordinal) <= 0,
                        _ => a.Equals(b)
                    };
                    break;
                case ConditionType.GreaterOrEqual:
                    result = a switch
                    {
                        int i => i >= (int)b,
                        float f => f >= (float)b,
                        string s => string.Compare(s, (string)b, StringComparison.Ordinal) >= 0,
                        _ => a.Equals(b)
                    };
                    break;
            }
            return result;
        }
        
        public static object EvaluateEffect(object a, object b, EffectType effect)
        {
            if (a.GetType() != b.GetType()) return false;
            
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
        
        public static object GetDefaultValue(object value)
        {
            return value is string ? "" : value.GetType().Default();
        }
    }
}
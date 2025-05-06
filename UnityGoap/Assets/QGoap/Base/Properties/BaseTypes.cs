using System;
using static QGoap.Base.PropertyManager;

namespace QGoap.Base
{
    public static class BaseTypes
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

        public static object Evaluate(this Effect effect, object a)
        {
            return Evaluate(a, effect.Type, effect.Value);
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
                        _ => b
                    };
                    break;
                case EffectType.Subtract:
                    result = a switch
                    {
                        int i => i - (int)b,
                        float f => f - (float)b,
                        _ => b
                    };
                    break;
                case EffectType.Multiply:
                    result = a switch
                    {
                        //int i => i * (int)b,
                        float f => f * (float)b,
                        _ => b
                    };
                    break;
                case EffectType.Divide:
                    result = a switch
                    {
                        //int i => i / (int)b,
                        float f => f / (float)b,
                        _ => b
                    };
                    break;
            }
            return result;
        }
        
        public static int GetDefaultDistance(PropertyKey key, object a)
        {
            var b = GetDefault(key);
            if (a.Equals(b)) return 0;
            
            return a switch
            {
                int iValue => iValue,
                float fValue => RoundAwayFromZero(fValue),
                string sValue => sValue.GetHashCode(),
                _ => 1
            };
        }

        public static int RoundAwayFromZero(this float value)
        {
            if (value >= 0) return (int)Math.Ceiling(value);
            return (int)Math.Floor(value);
        }

        public static object GetDefault(this PropertyKey key)
        {
            object defaultValue = GetPropertyType(key) switch
            {
                PropertyType.Boolean => false,
                PropertyType.Integer => 0,
                PropertyType.Float => 0f,
                PropertyType.String => "",
                PropertyType.Enum => "",
                _ => throw new ArgumentOutOfRangeException()
            };
            return defaultValue;
        }
    }
}
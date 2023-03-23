using System;

namespace GoapTFG.Base
{
    public static class BaseTypes
    {
        [Serializable]
        public enum ConditionType {
            Eq,
            Ne,
            Lt,
            Le,
            Gt,
            Ge
        }
        
        [Serializable]
        public enum EffectType {
            Set,
            Add,
            Sub,
            Mul,
            Div,
            Mod
        }
        
        public static bool EvaluateCondition(object a, object b, ConditionType condition)
        {
            if (a.GetType() != b.GetType()) return false;
            
            bool result;
            switch (condition)
            {
                case ConditionType.Eq:
                default:
                    result = a.Equals(b);
                    break;
                case ConditionType.Ne:
                    result = !a.Equals(b);
                    break;
                case ConditionType.Lt:
                    result = a switch
                    {
                        int i => i < (int)b,
                        float f => f < (float)b,
                        string s => string.Compare(s, (string)b, StringComparison.Ordinal) < 0,
                        _ => a.Equals(b)
                    };
                    break;
                case ConditionType.Gt:
                    result = a switch
                    {
                        int i => i > (int)b,
                        float f => f > (float)b,
                        string s => string.Compare(s, (string)b, StringComparison.Ordinal) > 0,
                        _ => a.Equals(b)
                    };
                    break;
                case ConditionType.Le:
                    result = a switch
                    {
                        int i => i <= (int)b,
                        float f => f <= (float)b,
                        string s => string.Compare(s, (string)b, StringComparison.Ordinal) <= 0,
                        _ => a.Equals(b)
                    };
                    break;
                case ConditionType.Ge:
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
                case EffectType.Sub:
                    result = a switch
                    {
                        int i => i - (int)b,
                        float f => f - (float)b,
                        string s => s.Replace((string)b, ""),
                        _ => b
                    };
                    break;
                case EffectType.Mul:
                    result = a switch
                    {
                        int i => i * (int)b,
                        float f => f * (float)b,
                        _ => b
                    };
                    break;
                case EffectType.Div:
                    result = a switch
                    {
                        int i => i / (int)b,
                        float f => f / (float)b,
                        _ => b
                    };
                    break;
                case EffectType.Mod:
                    result = a switch
                    {
                        int i => i % (int)b,
                        float f => f % (float)b,
                        _ => b
                    };
                    break;
            }
            return result;
        }
    }
}
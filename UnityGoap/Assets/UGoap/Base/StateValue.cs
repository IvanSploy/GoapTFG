using static UGoap.Base.BaseTypes;

namespace UGoap.Base
{
    public class ConditionValue
    {
        public readonly object Value;
        public readonly ConditionType ConditionType;

        public ConditionValue(object value, ConditionType conditionType)
        {
            Value = value;
            ConditionType = conditionType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            ConditionValue other = (ConditionValue)obj;
            return Value.Equals(other.Value) && ConditionType == other.ConditionType;
        }

        public override int GetHashCode()
        {
            var displaced = Value.GetHashCode() << 1;
            return displaced + (int)ConditionType;
        }
    }
    
    public class EffectValue
    {
        public readonly object Value;
        public readonly EffectType EffectType;

        public EffectValue(object value, EffectType effectType)
        {
            Value = value;
            EffectType = effectType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            EffectValue other = (EffectValue)obj;
            return Value.Equals(other.Value) && EffectType == other.EffectType;
        }

        public override int GetHashCode()
        {
            var displaced = Value.GetHashCode() << 1;
            return displaced + (int)EffectType;
        }
    }
}
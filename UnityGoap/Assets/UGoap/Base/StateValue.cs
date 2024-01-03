using static UGoap.Base.BaseTypes;

namespace UGoap.Base
{
    public class ConditionValue<TValue>
    {
        public readonly TValue Value;
        public readonly ConditionType ConditionType;

        public ConditionValue(TValue value, ConditionType conditionType)
        {
            Value = value;
            ConditionType = conditionType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            ConditionValue<TValue> other = (ConditionValue<TValue>)obj;
            return Value.Equals(other.Value) && ConditionType == other.ConditionType;
        }

        public override int GetHashCode()
        {
            var displaced = Value.GetHashCode() << 1;
            return displaced + (int)ConditionType;
        }
    }
    
    public class EffectValue<TValue>
    {
        public readonly TValue Value;
        public readonly EffectType EffectType;

        public EffectValue(TValue value, EffectType effectType)
        {
            Value = value;
            EffectType = effectType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            EffectValue<TValue> other = (EffectValue<TValue>)obj;
            return Value.Equals(other.Value) && EffectType == other.EffectType;
        }

        public override int GetHashCode()
        {
            var displaced = Value.GetHashCode() << 1;
            return displaced + (int)EffectType;
        }
    }
}
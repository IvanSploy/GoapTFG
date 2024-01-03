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

        protected bool Equals(ConditionValue<TValue> other)
        {
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

        protected bool Equals(EffectValue<TValue> other)
        {
            return Value.Equals(other.Value) && EffectType == other.EffectType;
        }

        public override int GetHashCode()
        {
            var displaced = Value.GetHashCode() << 1;
            return displaced + (int)EffectType;
        }
    }
}
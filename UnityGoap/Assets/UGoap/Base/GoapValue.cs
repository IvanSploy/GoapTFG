using static UGoap.Base.BaseTypes;

namespace UGoap.Base
{
    public class GoapValue<TValue>
    {
        public TValue Value;

        public GoapValue(TValue value)
        {
            Value = value;
        }
        
        public GoapValue(GoapValue<TValue> value)
        {
            Value = value.Value;
        }
        
        // Implicit conversion operator
        public static implicit operator TValue(GoapValue<TValue> propertyValue)
        {
            return propertyValue.Value;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            GoapValue<TValue> goapValue = (GoapValue<TValue>)obj;
            return Value.Equals(goapValue.Value);
        }
    }
    
    public class ConditionValue<TValue> : GoapValue<TValue>
    {
        public ConditionType ConditionType;

        public ConditionValue(TValue value, ConditionType conditionType) : base(value)
        {
            ConditionType = conditionType;
        }
        
        public ConditionValue(GoapValue<TValue> value, ConditionType conditionType) : base(value.Value)
        {
            ConditionType = conditionType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            ConditionValue<TValue> conditionValue = (ConditionValue<TValue>)obj;
            return Value.Equals(conditionValue.Value) && ConditionType.Equals(conditionValue.ConditionType);
        }
    }
    
    public class EffectValue<TValue> : GoapValue<TValue>
    {
        public EffectType EffectType;

        public EffectValue(TValue value, EffectType effectType) : base(value)
        {
            EffectType = effectType;
        }
        
        public EffectValue(GoapValue<TValue> value, EffectType effectType) : base(value.Value)
        {
            EffectType = effectType;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            EffectValue<TValue> effectValue = (EffectValue<TValue>)obj;
            return Value.Equals(effectValue.Value) && EffectType.Equals(effectValue.EffectType);
        }
    }
}
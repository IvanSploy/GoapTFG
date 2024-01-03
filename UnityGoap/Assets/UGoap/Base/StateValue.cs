using static UGoap.Base.BaseTypes;

namespace UGoap.Base
{
    public class StateValue<TValue>
    {
        public TValue Value;

        public StateValue(TValue value)
        {
            Value = value;
        }
        
        public StateValue(StateValue<TValue> value)
        {
            Value = value.Value;
        }
        
        // Implicit conversion operator
        public static implicit operator TValue(StateValue<TValue> propertyValue)
        {
            return propertyValue.Value;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            StateValue<TValue> stateValue = (StateValue<TValue>)obj;
            return Value.Equals(stateValue.Value);
        }
    }
    
    public class ConditionValue<TValue> : StateValue<TValue>
    {
        public ConditionType ConditionType;

        public ConditionValue(TValue value, ConditionType conditionType) : base(value)
        {
            ConditionType = conditionType;
        }
        
        public ConditionValue(StateValue<TValue> value, ConditionType conditionType) : base(value.Value)
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
    
    public class EffectValue<TValue> : StateValue<TValue>
    {
        public EffectType EffectType;

        public EffectValue(TValue value, EffectType effectType) : base(value)
        {
            EffectType = effectType;
        }
        
        public EffectValue(StateValue<TValue> value, EffectType effectType) : base(value.Value)
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
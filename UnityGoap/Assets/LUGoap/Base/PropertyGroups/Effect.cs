using static LUGoap.Base.BaseTypes;

namespace LUGoap.Base
{
    public class Effect
    {
        public readonly object Value;
        public readonly EffectType Type;

        public Effect(object value, EffectType type)
        {
            Value = value;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            Effect other = (Effect)obj;
            return Value.Equals(other.Value) && Type == other.Type;
        }

        public override int GetHashCode()
        {
            var displaced = Value.GetHashCode() << 1;
            return displaced + (int)Type;
        }
    }
}
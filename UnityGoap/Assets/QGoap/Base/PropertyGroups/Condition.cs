using System;
using System.Collections.Generic;
using System.Linq;
using static QGoap.Base.BaseTypes;

namespace QGoap.Base
{
    public class IntCondition : RangeCondition<int>
    {
        public IntCondition(RangeCondition<int> condition) : base(condition) { }
        public IntCondition(BaseTypes.ConditionType conditionType, int value) : base(conditionType, value) { }

        protected override void Initialize()
        {
            MinValue = int.MinValue;
            MaxValue = int.MaxValue;
        }
        
        public override void ApplyEffect(BaseTypes.EffectType type, object value)
        {
            base.ApplyEffect(type, value);
            
            if(MinValue != int.MinValue) MinValue = (int)Evaluate(MinValue, type, value);
            if(MaxValue != int.MaxValue) MaxValue = (int)Evaluate(MaxValue, type, value);
        }

        protected override int GetDistance(int value)
        {
            return GetCloserValue(value) - value;
        }

        protected override int GetCloserValue(int value)
        {
            if (RequiredValue != null) return (int)RequiredValue;

            if (value < MinValue) value = MinValue;
            if (value > MaxValue) value = MaxValue;
            
            return SearchCloser(value);
        }

        private int SearchCloser(int start)
        {
            if (!ExcludedValues.Contains(start))
            {
                if ((start != MinValue || MinInclusive) && (start != MaxValue || MaxInclusive))
                {
                    return start;
                }
            }
            
            int leftValue = start - 1;
            int rightValue = start + 1;
            while (leftValue >= MinValue || rightValue <= MaxValue)
            {
                if(!MinInclusive && leftValue.Equals(MinValue)) leftValue--;
                if(!MaxInclusive && rightValue.Equals(MaxValue)) rightValue++;
                    
                if (leftValue >= MinValue)
                {
                    if (!ExcludedValues.Contains(leftValue)) return leftValue;
                    leftValue--;
                }

                if (rightValue <= MaxValue)
                {
                    if (!ExcludedValues.Contains(rightValue)) return rightValue;
                    rightValue++;
                }
            }

            return start;
        }

        protected override bool CheckRange(int min, int max, bool minInclusive, bool maxInclusive)
        {
            if (Math.Abs(min - max) == 0 && !(minInclusive && maxInclusive)) return false;
            if (Math.Abs(min - max) == 1 && !minInclusive && !maxInclusive) return false;
            return true;
        }
    }
    
    public class FloatCondition : RangeCondition<float>
    {
        public const float TOLERANCE = 0.001f;
        public FloatCondition(RangeCondition<float> condition) : base(condition) { }
        public FloatCondition(BaseTypes.ConditionType conditionType, float value) : base(conditionType, value) { }
        
        protected override void Initialize()
        {
            MinValue = float.MinValue;
            MaxValue = float.MaxValue;
        }
        
        public override void ApplyEffect(BaseTypes.EffectType type, object value)
        {
            base.ApplyEffect(type, value);
            
            if(!MinValue.Equals(float.MinValue)) MinValue = (int)Evaluate(MinValue, type, value);
            if(!MaxValue.Equals(float.MaxValue)) MaxValue = (int)Evaluate(MaxValue, type, value);
        }
        
        protected override int GetDistance(float value)
        {
            return (int)Math.Ceiling(GetCloserValue(value) - value);
        }

        protected override float GetCloserValue(float value)
        {
            if (RequiredValue != null) return (float)RequiredValue;

            if (value < MinValue) value = MinValue;
            if (value > MaxValue) value = MaxValue;

            return SearchCloser(value);
        }
        
        private float SearchCloser(float start)
        {
            if (!ExcludedValues.Contains(start))
            {
                if ((!start.Equals(MinValue) || MinInclusive) && (!start.Equals(MaxValue) || MaxInclusive))
                {
                    return start;
                }
            }
            
            float leftValue = start - TOLERANCE;
            float rightValue = start + TOLERANCE;
            while (leftValue >= MinValue || rightValue <= MaxValue)
            {
                if(!MinInclusive && leftValue.Equals(MinValue)) leftValue -= TOLERANCE;
                if(!MaxInclusive && rightValue.Equals(MaxValue)) rightValue += TOLERANCE;
                    
                if (leftValue >= MinValue)
                {
                    if (!ExcludedValues.Contains(leftValue)) return leftValue;
                    leftValue -= TOLERANCE;
                }
                    
                if (rightValue <= MaxValue)
                {
                    if (!ExcludedValues.Contains(rightValue)) return rightValue;
                    rightValue += TOLERANCE;
                }
            }

            return start;
        }

        protected override bool CheckRange(float min, float max, bool minInclusive, bool maxInclusive)
        {
            return Math.Abs(min - max) > TOLERANCE || minInclusive && maxInclusive;
        }
    }
    
    public abstract class RangeCondition<T> : Condition where T : IComparable<T>, IConvertible
    {
        public T MinValue;
        public T MaxValue;
        public bool MinInclusive = true;
        public bool MaxInclusive = true;
        
        protected RangeCondition(RangeCondition<T> condition) : base(condition)
        {
            MinValue = condition.MinValue;
            MaxValue = condition.MaxValue;
            MinInclusive = condition.MinInclusive;
            MaxInclusive = condition.MaxInclusive;
        }

        protected RangeCondition(BaseTypes.ConditionType conditionType, T value) : base(conditionType, value)
        {
            Initialize();
            switch (conditionType)
            {
                case BaseTypes.ConditionType.LessThan:
                    MaxValue = value;
                    MaxInclusive = false;
                    break;
                case BaseTypes.ConditionType.LessOrEqual:
                    MaxValue = value;
                    break;
                case BaseTypes.ConditionType.GreaterThan:
                    MinValue = value;
                    MinInclusive = false;
                    break;
                case BaseTypes.ConditionType.GreaterOrEqual:
                    MinValue = value;
                    break;
            }
        }

        protected abstract void Initialize();

        public override bool Check(object value)
        {
            if (!base.Check(value)) return false;
            if (value is not int iValue) return false;
            
            if (!MaxInclusive && MaxValue.Equals(iValue)) return false;
            if (!MinInclusive && MinValue.Equals(iValue)) return false;

            if (iValue.CompareTo(MinValue) < 0) return false;
            return iValue.CompareTo(MaxValue) <= 0;
        }

        public override int GetDistance(object value)
        {
            if (Check(value)) return 0;
            if (value is not T tValue) throw new ArgumentException();
            return GetDistance(tValue);
        }

        protected abstract int GetDistance(T value);
        
        public override object GetClosest(object value)
        {
            var result = base.GetClosest(value);
            if (result != null) return result;
            if (value is not T tValue) throw new ArgumentException();
            return GetCloserValue(tValue);
        }
        
        protected abstract T GetCloserValue(T value);
        
        public override bool CheckCombine(Condition condition)
        {
            if (!base.CheckCombine(condition)) return false;
            if(condition is not RangeCondition<T> rangeCondition) return false;
            
            //Check ranges
            T min, max;
            bool minInclusive, maxInclusive;
            
            var minCompare = MinValue.CompareTo(rangeCondition.MinValue);
            if (minCompare == -1)
            {
                min = rangeCondition.MinValue;
                minInclusive = rangeCondition.MinInclusive;
            }
            else if (minCompare == 0)
            {
                min = MinValue;
                minInclusive = MinInclusive && rangeCondition.MinInclusive;
            }
            else
            {
                min = MinValue;
                minInclusive = MinInclusive;
            }
                
            var maxCompare = MaxValue.CompareTo(rangeCondition.MaxValue);
            if (maxCompare == 1)
            {
                max = rangeCondition.MaxValue;
                maxInclusive = rangeCondition.MaxInclusive;
            }
            else if (maxCompare == 0)
            {
                max = MaxValue;
                maxInclusive = MaxInclusive && rangeCondition.MaxInclusive;
            }
            else
            {
                max = MaxValue;
                maxInclusive = MaxInclusive;
            }

            return min.CompareTo(max) <= 0 && CheckRange(min, max, minInclusive, maxInclusive);
        }

        protected abstract bool CheckRange(T min, T max, bool minInclusive, bool maxInclusive);

        public override void Combine(Condition condition)
        {
            base.Combine(condition);
            
            if (condition is not RangeCondition<T> floatCondition) 
                throw new ArgumentException();

            if (RequiredValue != null)
            {
                MinValue = default;
                MaxValue = default;
                MinInclusive = false;
                MaxInclusive = false;
            }
            else
            {
                var minCompare = MinValue.CompareTo(floatCondition.MinValue);
                if (minCompare == -1)
                {
                    MinValue = floatCondition.MinValue;
                    MinInclusive = floatCondition.MinInclusive;
                }
                else if (minCompare == 0)
                {
                    MinInclusive = MinInclusive && floatCondition.MinInclusive;
                }
                
                var maxCompare = MaxValue.CompareTo(floatCondition.MaxValue);
                if (maxCompare == 1)
                {
                    MinValue = floatCondition.MaxValue;
                    MinInclusive = floatCondition.MaxInclusive;
                }
                else if (maxCompare == 0)
                {
                    MaxInclusive = MaxInclusive && floatCondition.MaxInclusive;
                }
            }
        }
        
        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            RangeCondition<T> other = (RangeCondition<T>)obj;
            return Equals(RequiredValue, other.RequiredValue)
                   && ExcludedValues.SetEquals(other.ExcludedValues)
                   && MinValue.Equals(other.MinValue)
                   && MaxValue.Equals(other.MaxValue)
                   && MinInclusive.Equals(other.MinInclusive)
                   && MaxInclusive.Equals(other.MaxInclusive);
        }

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(base.GetHashCode(), MinValue, MaxValue);
            hash <<= 2;
            hash += MaxInclusive ? 2 : 0;
            hash += MinInclusive ? 1 : 0;
            return hash;
        }
    }

    public class Condition
    {
        public object RequiredValue;
        public readonly HashSet<object> ExcludedValues = new();
        
        public Condition(Condition condition)
        {
            RequiredValue = condition.RequiredValue;
            ExcludedValues = new HashSet<object>(condition.ExcludedValues);
        }
        public Condition(BaseTypes.ConditionType conditionType, object value)
        {
            switch (conditionType)
            {
                case BaseTypes.ConditionType.Equal:
                    RequiredValue = value;
                    break;
                case BaseTypes.ConditionType.NotEqual:
                    ExcludedValues.Add(value);
                    break;
            }
        }
        
        public virtual bool Check(object value)
        {
            if (RequiredValue == null)
            {
                return !ExcludedValues.Contains(value);
            }
            return RequiredValue.Equals(value);
        }
        
        public virtual int GetDistance(object value)
        {
            if (Check(value)) return 0;
            if (RequiredValue != null)
            {
                return value switch
                {
                    bool bValue => bValue ? -1 : 1,
                    string sValue => string.Compare((string)RequiredValue, sValue, StringComparison.InvariantCultureIgnoreCase),
                    _ => 1
                };
            }

            return 1;
        }

        public virtual object GetClosest(object value)
        {
            if (Check(value)) return value;
            return RequiredValue;
        }
        
        public virtual bool CheckCombine(Condition condition)
        {
            //Check if required values are the same.
            if (RequiredValue != null && condition.RequiredValue != null)
                return RequiredValue.Equals(condition.RequiredValue);

            //Check if required value is not excluded value.
            if (RequiredValue != null)
            {
                foreach (var excludedValue in condition.ExcludedValues)
                {
                    if(RequiredValue.Equals(excludedValue)) 
                        return false;
                }
            }

            if (condition.RequiredValue != null)
            {
                foreach (var excludedValue in ExcludedValues)
                {
                    if(condition.RequiredValue.Equals(excludedValue)) 
                        return false;
                }
            }

            return true;
        }

        public virtual void Combine(Condition condition)
        {
            if (GetType() != condition.GetType()) return;
            RequiredValue ??= condition.RequiredValue;
            if(RequiredValue != null) return;

            ExcludedValues.UnionWith(condition.ExcludedValues);
        }

        public virtual void ApplyEffect(BaseTypes.EffectType type, object value)
        {
            if (RequiredValue != null)
            {
                RequiredValue = Evaluate(RequiredValue, type, value);
            }

            object[] excludedValues = ExcludedValues.ToArray();
            ExcludedValues.Clear();
            foreach (var excludedValue in excludedValues)
            {
                ExcludedValues.Add(Evaluate(excludedValue, type, value));
            }
        }
        
        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            Condition other = (Condition)obj;
            return Equals(RequiredValue, other.RequiredValue)
                   && ExcludedValues.SetEquals(other.ExcludedValues);
        }

        public override int GetHashCode()
        {
            int excludedHash = 0;
            foreach (var excludedValue in ExcludedValues)
            {
                excludedHash ^= excludedValue.GetHashCode();
            }

            var hash = HashCode.Combine(RequiredValue, excludedHash);
            return hash;
        }
    }
}
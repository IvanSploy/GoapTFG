using System;
using System.Collections.Generic;
using System.Globalization;
using static QGoap.Base.BaseTypes;

namespace QGoap.Base
{
    public static class ConditionFactory
    {
        public static Condition Create(BaseTypes.ConditionType conditionType, object value)
        {
            Condition result;
            switch (value)
            {
                case int iValue:
                    result = new IntCondition(conditionType, iValue);
                    break;
                case float fValue:
                    result = new FloatCondition(conditionType, fValue);
                    break;
                default:
                    result = new Condition(conditionType, value);
                    break;
            }

            return result;
        }
        
        public static Condition Create(Condition condition)
        {
            Condition result;
            switch (condition)
            {
                case IntCondition intConditionValue:
                    result = new IntCondition(intConditionValue);
                    break;
                case FloatCondition floatConditionValue:
                    result = new FloatCondition(floatConditionValue);
                    break;
                default:
                    result = new Condition(condition);
                    break;
            }

            return result;
        }
    }
}
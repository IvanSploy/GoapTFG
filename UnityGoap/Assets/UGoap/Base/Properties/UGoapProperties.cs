using System;
using System.Collections.Generic;

namespace UGoap.Base
{
    public static partial class UGoapPropertyManager
    {
        [Serializable]
        public enum PropertyKey 
        {
            Target,
Seeds,
Fish,
Money,
Happiness,
Fatigue,
MentalState,

        }
        
        private static readonly Dictionary<PropertyKey, PropertyType> PropertyTypes = new()
        {
            { PropertyKey.Target, PropertyType.String },
{ PropertyKey.Seeds, PropertyType.Integer },
{ PropertyKey.Fish, PropertyType.Integer },
{ PropertyKey.Money, PropertyType.Float },
{ PropertyKey.Happiness, PropertyType.Float },
{ PropertyKey.Fatigue, PropertyType.Float },
{ PropertyKey.MentalState, PropertyType.Enum },
       
        };
    }
}

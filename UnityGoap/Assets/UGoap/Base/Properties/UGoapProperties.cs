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
Coins,
DoorState,
HasKey,

        }
        
        private static readonly Dictionary<PropertyKey, PropertyType> PropertyTypes = new()
        {
            { PropertyKey.Target, PropertyType.String },
{ PropertyKey.Coins, PropertyType.Integer },
{ PropertyKey.DoorState, PropertyType.Enum },
{ PropertyKey.HasKey, PropertyType.Boolean },
       
        };
    }
}

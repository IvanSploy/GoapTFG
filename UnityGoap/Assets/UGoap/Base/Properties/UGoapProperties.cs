using System;
using System.Collections.Generic;

namespace UGoap.Base
{
    public static partial class UGoapPropertyManager
    {
        [Serializable]
        public enum PropertyKey 
        {
            None,
            Target,
Coins,
DoorState,
HasKey,
Indicator,
IsIt,
PlayerNear,
DestinationX,
DestinationZ,
MoveState,

        }
        
        private static readonly Dictionary<PropertyKey, PropertyType> PropertyTypes = new()
        {
            { PropertyKey.Target, PropertyType.String },
{ PropertyKey.Coins, PropertyType.Integer },
{ PropertyKey.DoorState, PropertyType.Enum },
{ PropertyKey.HasKey, PropertyType.Boolean },
{ PropertyKey.Indicator, PropertyType.Enum },
{ PropertyKey.IsIt, PropertyType.Boolean },
{ PropertyKey.PlayerNear, PropertyType.Boolean },
{ PropertyKey.DestinationX, PropertyType.Float },
{ PropertyKey.DestinationZ, PropertyType.Float },
{ PropertyKey.MoveState, PropertyType.Enum },
       
        };
    }
}

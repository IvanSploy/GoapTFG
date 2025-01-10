using System;
using System.Collections.Generic;

namespace LUGoap.Base
{
    public static partial class PropertyManager
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
TargetX,
TargetZ,
GoalReached,
Hp,

        }
        
        private static readonly Dictionary<PropertyKey, PropertyType> PropertyTypes = new()
        {
            { PropertyKey.Target, PropertyType.String },
{ PropertyKey.Coins, PropertyType.Integer },
{ PropertyKey.DoorState, PropertyType.Enum },
{ PropertyKey.HasKey, PropertyType.Boolean },
{ PropertyKey.Indicator, PropertyType.Enum },
{ PropertyKey.IsIt, PropertyType.Boolean },
{ PropertyKey.PlayerNear, PropertyType.Enum },
{ PropertyKey.DestinationX, PropertyType.Float },
{ PropertyKey.DestinationZ, PropertyType.Float },
{ PropertyKey.MoveState, PropertyType.Enum },
{ PropertyKey.TargetX, PropertyType.Float },
{ PropertyKey.TargetZ, PropertyType.Float },
{ PropertyKey.GoalReached, PropertyType.Boolean },
{ PropertyKey.Hp, PropertyType.Float },
       
        };
    }
}

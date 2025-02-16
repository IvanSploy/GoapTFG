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
Hp,
Ammo,
HasEnemy,
EnemyVisible,
EnemyHp,
HasMoved,

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
{ PropertyKey.Hp, PropertyType.Float },
{ PropertyKey.Ammo, PropertyType.Integer },
{ PropertyKey.HasEnemy, PropertyType.Boolean },
{ PropertyKey.EnemyVisible, PropertyType.Boolean },
{ PropertyKey.EnemyHp, PropertyType.Integer },
{ PropertyKey.HasMoved, PropertyType.Boolean },
       
        };
    }
}

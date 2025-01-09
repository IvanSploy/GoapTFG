using System.Collections.Generic;

namespace LUGoap.Base
{
    public static partial class PropertyManager
    {
        public static readonly Dictionary<PropertyKey, string[]> EnumNames = new()
        {
            { PropertyKey.DoorState, new [] { "Opened", "Closed", "Locked", }},
{ PropertyKey.Indicator, new [] { "Blue", "Red", }},
{ PropertyKey.MoveState, new [] { "Ready", "Set", "Required", }},
{ PropertyKey.PlayerNear, new [] { "Close", "Near", "Far", }},

        };
    }
}

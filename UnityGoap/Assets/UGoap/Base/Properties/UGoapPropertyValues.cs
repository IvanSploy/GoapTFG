using System.Collections.Generic;

namespace UGoap.Base
{
    public static partial class UGoapPropertyManager
    {
        public static readonly Dictionary<PropertyKey, string[]> EnumNames = new()
        {
            { PropertyKey.DoorState, new [] { "Opened", "Closed", "Locked", }},
{ PropertyKey.Indicator, new [] { "Blue", "Red", }},
{ PropertyKey.MoveState, new [] { "Ready", "Set", "Required", }},

        };
    }
}

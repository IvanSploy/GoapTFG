using System.Collections.Generic;

namespace UGoap.Base
{
    public static partial class UGoapPropertyManager
    {
        public static Dictionary<PropertyKey, string[]> EnumNames = new()
        {
            { PropertyKey.MentalState, new [] { "Good", "Regular", "Bad", }},

        };
    }
}

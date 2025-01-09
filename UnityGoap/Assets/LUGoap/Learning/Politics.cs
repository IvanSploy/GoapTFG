using System.Collections.Generic;
using System.Linq;

namespace LUGoap.Learning
{
    public static class Politics
    {
        public static string GetMax(Dictionary<string, float> values)
        {
            return values.Aggregate((maxPair, pair) => pair.Value > maxPair.Value ? pair : maxPair).Key;
        }
    }
}
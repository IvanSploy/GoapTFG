using System.Collections.Generic;
using LUGoap.Base;

namespace LUGoap.Learning
{
    public struct QLearningData
    {
        public float Alpha;
        public float Gamma;
        public float ExploreChance;
        public List<PropertyManager.PropertyKey> FilterKeys;
        public List<PropertyManager.PropertyKey> AdditionalKeys;
        public int ValueRange;

        public static QLearningData GetDefault()
        {
            return new QLearningData()
            {
                Alpha = 0.25f,
                Gamma = 0.9f,
                ValueRange = 5,
            };
        }
    }
}
﻿using System.Collections.Generic;
using QGoap.Base;

namespace QGoap.Learning
{
    public struct QLearningConfig
    {
        public float Alpha;
        public float Gamma;
        public float Epsilon;
        public List<PropertyManager.PropertyKey> FilterKeys;
        public List<PropertyManager.PropertyKey> AdditionalKeys;
        public int RangeSize;

        public static QLearningConfig GetDefault()
        {
            return new QLearningConfig()
            {
                Alpha = 0.25f,
                Gamma = 0.9f,
                RangeSize = 5,
            };
        }
    }
}
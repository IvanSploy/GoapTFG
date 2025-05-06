using System;
using System.Collections.Generic;
using QGoap.Base;
using QGoap.Learning;
using UnityEngine;
using UnityEngine.Serialization;

namespace QGoap.Unity
{
    [Serializable]
    public struct QLearningTemplate
    {
        [Header("Main")]
        [Range(0f,1f)] public float Alpha;
        [Range(0f,1f)] public float Epsilon;

        [Header("Filtering")]
        public List<PropertyManager.PropertyKey> FilterKeys;
        public List<PropertyManager.PropertyKey> AdditionalKeys;
        [FormerlySerializedAs("ValueRange")] 
        public int RangeSize;

        public static QLearningTemplate GetDefault()
        {
            return new QLearningTemplate()
            {
                Alpha = 0.25f,
                RangeSize = 5,
            };
        }

        public QLearningConfig DeSerialize()
        {
            return new QLearningConfig
            {
                Alpha = Alpha,
                Gamma = 0,
                Epsilon = Epsilon,
                FilterKeys = FilterKeys,
                AdditionalKeys = AdditionalKeys,
                RangeSize = RangeSize
            };
        }
    }
}
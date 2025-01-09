using System;
using System.Collections.Generic;
using LUGoap.Base;
using LUGoap.Learning;
using UnityEngine;

namespace LUGoap.Unity
{
    [Serializable]
    public struct QLearningTemplate
    {
        [Header("Main")]
        [Range(0f,1f)] public float Alpha;
        [Range(0f,1f)] public float Gamma;
        
        [Header("Exploration")]
        [Range(0f,1f)] public float ExploreChance;

        [Header("Filtering")]
        public List<PropertyManager.PropertyKey> FilterKeys;
        public List<PropertyManager.PropertyKey> AdditionalKeys;
        public int ValueRange;

        public static QLearningTemplate GetDefault()
        {
            return new QLearningTemplate()
            {
                Alpha = 0.25f,
                Gamma = 0.9f,
                ValueRange = 5,
            };
        }

        public QLearningData DeSerialize()
        {
            return new QLearningData
            {
                Alpha = Alpha,
                Gamma = Gamma,
                ExploreChance = ExploreChance,
                FilterKeys = FilterKeys,
                AdditionalKeys = AdditionalKeys,
                ValueRange = ValueRange
            };
        }
    }
}
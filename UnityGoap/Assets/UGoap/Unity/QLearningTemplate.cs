using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Learning;
using UnityEngine;

namespace UGoap.Unity
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
        public List<PropertyManager.PropertyKey> LearningKeys;
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
                LearningKeys = LearningKeys,
                ValueRange = ValueRange
            };
        }
    }
}
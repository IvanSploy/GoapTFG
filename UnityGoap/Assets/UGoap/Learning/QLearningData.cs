using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;

namespace UGoap.Learning
{
    [Serializable]
    public struct QLearningData
    {
        [Header("Main")]
        [Range(0f,1f)] public float Alpha;
        [Range(0f,1f)] public float Gamma;
        
        [Header("Exploration")]
        [Range(0f,1f)] public float ExploreChance;
        public Vector2 ExploreRange;

        [Header("Filtering")]
        public List<PropertyManager.PropertyKey> LearningKeys;
        public int ValueRange;

        public static QLearningData GetDefault()
        {
            return new QLearningData()
            {
                Alpha = 0.25f,
                Gamma = 0.9f,
                ExploreRange = new Vector2(-1000, 1000),
                ValueRange = 5,
            };
        }
    }
}
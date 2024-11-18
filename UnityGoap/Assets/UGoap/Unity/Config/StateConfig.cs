using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "Goap Items/State", order = 1)]
    public class StateConfig : ScriptableObject
    {
        [HideInInspector] public List<Property> properties;

        public GoapState Create()
        {
            GoapState goapState = new();
            goapState.ApplyProperties(properties);
            return goapState;
        }
    }
}

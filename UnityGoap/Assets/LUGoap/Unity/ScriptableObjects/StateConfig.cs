using System.Collections.Generic;
using LUGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "LUGoap/State", order = -1)]
    public class StateConfig : ScriptableObject
    {
        [FormerlySerializedAs("properties")] 
        [HideInInspector] public List<Property> Properties;

        public State Create()
        {
            State state = new();
            state.ApplyProperties(Properties);
            return state;
        }
    }
}

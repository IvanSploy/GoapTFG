using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.PropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "UGoap/State", order = 1)]
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

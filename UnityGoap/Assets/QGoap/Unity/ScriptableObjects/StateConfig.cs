using System.Collections.Generic;
using QGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "QGoap/State", order = -1)]
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

using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "UGoap/State", order = 1)]
    public class StateConfig : ScriptableObject
    {
        [FormerlySerializedAs("properties")] 
        [HideInInspector] public List<Property> Properties;

        public GoapState Create()
        {
            GoapState goapState = new();
            goapState.ApplyProperties(Properties);
            return goapState;
        }
    }
}

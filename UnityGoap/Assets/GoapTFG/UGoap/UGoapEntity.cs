using GoapTFG.Base;
using GoapTFG.UGoap.ScriptableObjects;
using UnityEngine;

namespace GoapTFG.UGoap
{
    public class UGoapEntity : MonoBehaviour, IGoapEntity<UGoapPropertyManager.PropertyList, object>
    {
        public string nameEntity;
        public UGoapState uGoapState;

        public string Name => nameEntity;

        public PropertyGroup<UGoapPropertyManager.PropertyList, object> CurrentState { get; set; }

        private void Awake()
        {
            if (uGoapState != null)
            {
                PropertyGroup<UGoapPropertyManager.PropertyList, object> state = new ();
                UGoapPropertyManager.AddIntoPropertyGroup(uGoapState.stateProperties, in state);
                CurrentState = state;
            }
            else
            {
                CurrentState = new PropertyGroup<UGoapPropertyManager.PropertyList, object>();
            }
            UGoapWMM.Add(Name, this);
        }

        private void OnValidate()
        {
            nameEntity ??= gameObject.name;
        }
    }
}
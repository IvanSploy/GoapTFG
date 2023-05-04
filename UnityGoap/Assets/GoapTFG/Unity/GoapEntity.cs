using GoapTFG.Base;
using GoapTFG.Unity.ScriptableObjects;
using UnityEngine;

namespace GoapTFG.Unity
{
    public class GoapEntity : MonoBehaviour, IEntity<PropertyManager.PropertyList, object>
    {
        public string nameEntity;
        public StateScriptableObject StateScriptableObject;

        public string Name => nameEntity;

        public PropertyGroup<PropertyManager.PropertyList, object> CurrentState { get; set; }

        private void Start()
        {
            if (StateScriptableObject != null)
            {
                PropertyGroup<PropertyManager.PropertyList, object> state = new ();
                PropertyManager.AddIntoPropertyGroup(StateScriptableObject.stateProperties, in state);
                CurrentState = state;
            }
            else
            {
                CurrentState = new PropertyGroup<PropertyManager.PropertyList, object>();
            }
            WorkingMemoryManager.Add(Name, this);
        }

        private void OnValidate()
        {
            nameEntity ??= gameObject.name;
        }
    }
}
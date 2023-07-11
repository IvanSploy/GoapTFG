using System;
using GoapTFG.Base;
using GoapTFG.UGoap.ScriptableObjects;
using UnityEngine;

namespace GoapTFG.UGoap
{
    [RequireComponent(typeof(Collider))]
    public class UGoapEntity : MonoBehaviour, IGoapEntity<UGoapPropertyManager.PropertyKey, object>
    {
        [SerializeField] private string nameEntity;
        [SerializeField] private UGoapState initialState;
        [SerializeField] private Collider _collider;

        public string Name => nameEntity;
        public Collider Collider => _collider;

        public PropertyGroup<UGoapPropertyManager.PropertyKey, object> CurrentState { get; set; }

        private void Awake()
        {
            _collider ??= GetComponent<Collider>();
            gameObject.layer = LayerMask.NameToLayer("Entity");
            
            AddToWorkingMemoryManager();
        }

        private void OnValidate()
        {
            nameEntity ??= gameObject.name;
        }

        private void AddToWorkingMemoryManager()
        {
            if (initialState != null)
            {
                PropertyGroup<UGoapPropertyManager.PropertyKey, object> state = new ();
                UGoapPropertyManager.AddIntoPropertyGroup(this.initialState.properties, in state);
                CurrentState = state;
            }
            else
            {
                CurrentState = new PropertyGroup<UGoapPropertyManager.PropertyKey, object>();
            }
            UGoapWMM.Add(Name, this);
        }
    }
}
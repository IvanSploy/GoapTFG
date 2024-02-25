using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity
{
    [RequireComponent(typeof(Collider))]
    public class UGoapEntity : MonoBehaviour, IGoapEntity<PropertyKey, object>
    {
        [SerializeField] private string nameEntity;
        [SerializeField] private Collider _collider;
        [SerializeField] private List<Property> _initialState;

        public string Name => nameEntity;
        public Collider Collider => _collider;

        public StateGroup<PropertyKey, object> CurrentState { get; set; }

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
            StateGroup<PropertyKey, object> state = new ();
            AddIntoPropertyGroup(_initialState, in state);
            CurrentState = state;
            UGoapWMM.Add(Name, this);
        }
    }
}
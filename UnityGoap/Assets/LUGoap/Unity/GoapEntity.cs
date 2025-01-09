using System.Collections.Generic;
using LUGoap.Base;
using UnityEngine;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Unity
{
    [RequireComponent(typeof(Collider))]
    public class GoapEntity : MonoBehaviour, IEntity
    {
        [SerializeField] private string _nameEntity;
        [SerializeField] private Collider _collider;
        [SerializeField] private List<Property> _initialState;

        public string Name => _nameEntity;
        public Collider Collider => _collider;

        public State CurrentState { get; set; }

        private void Awake()
        {
            _collider ??= GetComponent<Collider>();
            gameObject.layer = LayerMask.NameToLayer("Entity");
            
            AddToWorkingMemoryManager();
        }

        private void OnValidate()
        {
            _nameEntity ??= gameObject.name;
        }

        private void AddToWorkingMemoryManager()
        {
            State state = new();
            state.ApplyProperties(_initialState);
            CurrentState = state;
            WorkingMemoryManager.Add(Name, this);
        }
    }
}
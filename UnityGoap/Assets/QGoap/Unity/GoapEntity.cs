using System.Collections.Generic;
using QGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity
{
    public class GoapEntity : MonoBehaviour, IEntity
    {
        [FormerlySerializedAs("_nameEntity")] 
        [SerializeField] private string _name;
        [SerializeField] private Collider _collider;
        [SerializeField] private List<Property> _initialState;

        public string Name => _name;
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
            _name ??= gameObject.name;
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
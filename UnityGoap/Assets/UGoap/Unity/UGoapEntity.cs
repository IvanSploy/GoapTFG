using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity
{
    [RequireComponent(typeof(Collider))]
    public class UGoapEntity : MonoBehaviour, IGoapEntity
    {
        [FormerlySerializedAs("nameEntity")] 
        [SerializeField] private string _nameEntity;
        [SerializeField] private Collider _collider;
        [SerializeField] private List<Property> _initialState;

        public string Name => _nameEntity;
        public Collider Collider => _collider;

        public GoapState CurrentState { get; set; }

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
            GoapState goapState = new();
            goapState.ApplyProperties(_initialState);
            CurrentState = goapState;
            UGoapWMM.Add(Name, this);
        }
    }
}
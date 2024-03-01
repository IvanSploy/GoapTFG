using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity
{
    [RequireComponent(typeof(Collider))]
    public class UGoapEntity : MonoBehaviour, IGoapEntity
    {
        [SerializeField] private string nameEntity;
        [SerializeField] private Collider _collider;
        [SerializeField] private List<Property> _initialState;

        public string Name => nameEntity;
        public Collider Collider => _collider;

        public GoapState CurrentGoapState { get; set; }

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
            GoapState goapState = new ();
            AddIntoPropertyGroup(_initialState, in goapState);
            CurrentGoapState = goapState;
            UGoapWMM.Add(Name, this);
        }
    }
}
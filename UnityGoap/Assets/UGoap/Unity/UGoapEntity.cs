using UGoap.Base;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity
{
    [RequireComponent(typeof(Collider))]
    public class UGoapEntity : MonoBehaviour, IGoapEntity<PropertyKey, object>
    {
        [SerializeField] private string nameEntity;
        [SerializeField] private UGoapState initialState;
        [SerializeField] private Collider _collider;

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
            if (initialState != null)
            {
                StateGroup<PropertyKey, object> state = new ();
                AddIntoPropertyGroup(initialState.properties, in state);
                CurrentState = state;
            }
            else
            {
                CurrentState = new StateGroup<PropertyKey, object>();
            }
            UGoapWMM.Add(Name, this);
        }
    }
}
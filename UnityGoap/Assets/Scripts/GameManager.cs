using UGoap.Base;
using UGoap.Unity;
using UnityEngine;
using static UGoap.Base.PropertyManager;

public class GameManager : MonoBehaviour
{
    [Range(0f,1f)]
    [SerializeField] private float _lockProbability;
    
    // Start is called before the first frame update
    private void Start()
    {
        //Define starting simulation conditions.
        var agent = FindObjectOfType<UGoapAgent>();
        
        //EXAMPLE 1: DOOR
        var random = Random.Range(0f, 1f);
        bool isLocked = random <= _lockProbability;
        
        State doorState = new State();
        doorState[PropertyKey.DoorState] = isLocked ? "Locked" : "Closed";
        WorkingMemoryManager.Get("Door").Object.CurrentState = doorState;
        
        WorkingMemoryManager.Get("Indicator").Object.GetComponent<MaterialSelector>().SetMaterial(isLocked ? 1 : 0);
        
        State agentState = new State();
        agentState[PropertyKey.DoorState] = "Closed";
        agentState[PropertyKey.Indicator] = isLocked ? "Red" : "Blue";
        
        //EXAMPLE 2: ???
        
        agent.Initialize(agentState);
    }
}

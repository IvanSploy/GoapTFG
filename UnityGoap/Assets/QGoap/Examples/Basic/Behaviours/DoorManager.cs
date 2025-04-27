using QGoap.Base;
using QGoap.Unity;
using UnityEngine;
using static QGoap.Base.PropertyManager;
using Random = QGoap.Base.Random;

public class DoorManager : MonoBehaviour
{
    [Range(0f,1f)]
    [SerializeField] private float _lockProbability;
    
    // Start is called before the first frame update
    private void Start()
    {
        var random = Random.Next();
        bool isLocked = random <= _lockProbability;
        
        State doorState = new State();
        doorState[PropertyKey.DoorState] = isLocked ? "Locked" : "Closed";
        WorkingMemoryManager.Get("Door").Object.CurrentState = doorState;
        
        WorkingMemoryManager.Get("Indicator").Object.GetComponent<MaterialSelector>().SetMaterial(isLocked ? 1 : 0);
        
        State agentState = new State();
        agentState[PropertyKey.DoorState] = "Closed";
        agentState[PropertyKey.Indicator] = isLocked ? "Red" : "Blue";
        
        var agents = FindObjectsByType<GoapAgent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            agent.Initialize(agentState);
        }
    }
}

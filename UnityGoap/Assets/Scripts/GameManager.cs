using UGoap.Base;
using UGoap.Unity;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager.PropertyKey;

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
        
        GoapState doorState = new GoapState();
        doorState[DoorState] = isLocked ? "Locked" : "Closed";
        UGoapWMM.Get("Door").Object.CurrentGoapState = doorState;
        
        UGoapWMM.Get("Indicator").Object.GetComponent<MaterialSelector>().SetMaterial(isLocked ? 1 : 0);
        
        GoapState agentState = new GoapState();
        agentState[DoorState] = "Closed";
        agentState[Indicator] = isLocked ? "Red" : "Blue";
        
        //EXAMPLE 2: ???
        
        agent.Initialize(agentState);
    }
}

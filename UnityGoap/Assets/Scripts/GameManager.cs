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
        doorState[DoorState] = isLocked ? 2 : 1;
        UGoapWMM.Get("Door").Object.CurrentGoapState = doorState;
        
        UGoapWMM.Get("Indicator").Object.GetComponent<MaterialSelector>().SetMaterial(isLocked ? 1 : 0);
        
        GoapState agentState = new GoapState();
        agentState[DoorState] = 1;
        agentState[Indicator] = isLocked ? 1 : 0;
        
        //EXAMPLE 2: ???
        
        agent.Initialize(agentState);
    }
}

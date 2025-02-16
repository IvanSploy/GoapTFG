using UnityEngine;
using Panda.Examples.Shooter;

public class GoapGoalSeeker : MonoBehaviour
{
    [SerializeField] private CheckpointManager _checkpointManager;
    private Unit _self;
    
    protected void Start()
    {
        _self = GetComponent<Unit>();
    }
    
    public bool SetDestination_CheckPoint()
    {
        if (_checkpointManager.Count <= 0) return false;
        
        var dst = _checkpointManager.GetCurrent();
        
        UnityEngine.AI.NavMeshPath selfPath = new UnityEngine.AI.NavMeshPath();
        if (_self.navMeshAgent.CalculatePath(dst, selfPath) && selfPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            _self.SetDestination(dst);
            return true;
        }

        return false;
    }
}

using UnityEngine;
using System.Collections.Generic;
using Panda;
using Panda.Examples.Shooter;

public class GoalSeeker : MonoBehaviour
{
    public List<Transform> CheckPoints;

    protected Unit _self;
    protected AI _ai;

    protected readonly Queue<Vector3> _remainingCheckPoints = new();
    private bool _loaded;
    
    // Use this for initialization
    protected void Start()
    {
        _self = GetComponent<Unit>();
        _ai = GetComponent<AI>();
    }

    [Task]
    bool IsLoaded => _loaded;

    [Task]
    public void LoadCheckPoints()
    {
        _remainingCheckPoints.Clear();
        foreach (var checkPointTransform in CheckPoints)
        {
            _remainingCheckPoints.Enqueue(checkPointTransform.position);
        }

        _loaded = true;
        ThisTask.Succeed();
    }
    
    [Task]
    public bool SetDestination_CheckPoint()
    {
        if (_remainingCheckPoints.Count <= 0)
            return false;
        
        var dst = _remainingCheckPoints.Peek();
        
        var attacker = _self.shotBy ? _self.shotBy : _ai.enemy;
        if (attacker)
        {
            var src = attacker.transform.position;
            var ignoreList = new List<GameObject>() { this.gameObject, attacker.gameObject };

            if (HasLoS(src, dst, ignoreList))
                return false;
        }
        
        UnityEngine.AI.NavMeshPath selfPath = new UnityEngine.AI.NavMeshPath();
        if (_self.navMeshAgent.CalculatePath(dst, selfPath) && selfPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            _self.SetDestination(dst);
            return true;
        }

        return false;
    }
    
    [Task]
    public void RemoveCheckPoint()
    {
        _remainingCheckPoints.Dequeue();
        ThisTask.Succeed();
    }

    protected bool HasLoS( Vector3 source, Vector3 destination, List<GameObject>  ignoreList )
    {
        bool hasLos = true;
        var delta = (destination - source);
        var ray = new Ray(source, delta.normalized);
        var hits = Physics.RaycastAll(ray, delta.magnitude );
        foreach( var hit in hits)
        {
            var type = hit.collider.GetComponent<TriggerType>();
            if (!type || !type.collidesWithBullet)
                continue;
                
            var go = hit.collider.attachedRigidbody ? hit.collider.attachedRigidbody.gameObject: hit.collider.gameObject;
            if(! ignoreList.Contains( go ) && Vector3.Distance( hit.point, destination ) > 2.0f)
            {
                hasLos = false;
                break;
            }
        }
        return hasLos;
    }
}

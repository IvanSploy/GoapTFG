using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Panda;
using Panda.Examples.Shooter;

public class GoalSeek : MonoBehaviour
{
    public List<Transform> CheckPoints;

    Unit self;
    AI ai;
    
    private readonly Queue<Vector3> _remainingCheckPoints = new();
    private bool _loaded;

    [Task]
    bool IsLoaded => _loaded;

    [Task]
    void LoadCheckPoints()
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
    bool SetDestination_CheckPoint()
    {
        if (_remainingCheckPoints.Count <= 0)
            return false;
        
        var dst = _remainingCheckPoints.Peek();
        
        var attacker = self.shotBy != null ? self.shotBy : ai.enemy;
        if (attacker != null)
        {
            var src = attacker.transform.position;
            var ignoreList = new List<GameObject>() { this.gameObject, attacker.gameObject };

            if (HasLoS(src, dst, ignoreList))
                return false;
        }
        
        UnityEngine.AI.NavMeshPath selfPath = new UnityEngine.AI.NavMeshPath();
        if (self.navMeshAgent.CalculatePath(dst, selfPath) && selfPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            self.SetDestination(dst);
            return true;
        }

        return false;
    }
    
    [Task]
    void RemoveCheckPoint()
    {
        _remainingCheckPoints.Dequeue();
        ThisTask.Succeed();
    }

    bool HasLoS( Vector3 source, Vector3 destination, List<GameObject>  ignoreList )
    {
        bool hasLos = true;
        var delta = (destination - source);
        var ray = new Ray(source, delta.normalized);
        var hits = Physics.RaycastAll(ray, delta.magnitude );
        foreach( var hit in hits)
        {
            var type = hit.collider.GetComponent<TriggerType>();
            if (type == null || !type.collidesWithBullet)
                continue;
                
            var go = hit.collider.attachedRigidbody != null ? hit.collider.attachedRigidbody.gameObject: hit.collider.gameObject;
            if(! ignoreList.Contains( go ) && Vector3.Distance( hit.point, destination ) > 2.0f)
            {
                hasLos = false;
                break;
            }
        }
        return hasLos;
    }

    // Use this for initialization
    void Start()
    {
        self = this.GetComponent<Unit>();
        ai = GetComponent<AI>();
    }

}

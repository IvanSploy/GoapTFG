using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointManager : MonoBehaviour
{
    private List<Transform> _waypoints = new();

    private void Awake()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            _waypoints.Add(transform.GetChild(i));
        }
    }

    public (Vector3 previous, Vector3 next) GetWaypoints(NavMeshAgent navMeshAgent)
    {
        float d1 = float.PositiveInfinity;
        float d2 = float.PositiveInfinity;
        int index1 = -1;
        int index2 = -1;
        var path = new NavMeshPath();
        for (var i = 0; i < _waypoints.Count; i++)
        {
            var waypoint = _waypoints[i];
            if (navMeshAgent.CalculatePath(waypoint.position, path))
            {
                var distance = PathLength(path);
                if (distance < d1)
                {
                    index2 = index1;
                    index1 = i;
                    d2 = d1;
                    d1 = distance;
                }
                else if (distance < d2)
                {
                    index2 = i;
                    d2 = distance;
                }
            }
        }
        
        if(index1 > index2) (index1, index2) = (index2, index1);

        return (_waypoints[index1].position,  _waypoints[index2].position);
    }
    
    private float PathLength(NavMeshPath path)
    {
        float d = float.PositiveInfinity;

        if (path != null && path.corners.Length > 1)
        {
            d = 0.0f;
            for (int i = 0; i < path.corners.Length-1; i++)
            {
                var p0 = path.corners[i + 0];
                var p1 = path.corners[i + 1];
                d += Vector3.Distance(p1, p0);
            }
        }
        return d;
    }
}

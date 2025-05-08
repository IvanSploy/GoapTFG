using System.Collections.Generic;
using Panda.Examples.Shooter;
using UnityEngine;

public class GoapAI : MonoBehaviour
{
    public Unit Enemy { get; private set; }
    private GoapUnit _self;
    private AIVision _vision;

    private Vector3 _enemyLastSeenPosition;
    private float _lastSeenTime = float.NegativeInfinity;

    void Start()
    {
        _vision = GetComponentInChildren<AIVision>();
        _self = GetComponent<GoapUnit>();
    }
    
    public bool SetTarget_Enemy()
    {
        if (!Enemy) return false;
        _self.SetTarget(Enemy.transform.position);
        return true;
    }

    public bool SetTarget_EnemyLastSeenPosition()
    {
        if (!Enemy) return false;
        _self.SetTarget(_enemyLastSeenPosition);
        return true;
    }

    public void SetTarget_Angle(float angle)
    {
        var p = transform.position + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        _self.SetTarget(p);
    }

    public void SetGlobalTarget_Angle(float angle)
    {
        var p = transform.position + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        _self.SetTarget(p);
    }

    public bool Acquire_Enemy()
    {
        Enemy = null;

        if (!Enemy && _self.shotBy && _self.shotBy.team != _self.team && (Time.time - _self.lastShotTime) < 1.0f)
            Enemy = _self.shotBy;

        if (!Enemy && _vision.visibles != null)
        {
            foreach (var v in _vision.visibles)
            {
                if (!v) continue;

                var shooter = v.GetComponent<Unit>();

                if (!shooter)
                {
                    var bullet = v.GetComponent<Bullet>();
                    shooter = bullet && bullet.shooter ? bullet.shooter.GetComponent<Unit>() : null;

                    if (shooter && _self.team == shooter.team)
                        shooter = null;
                }

                if (shooter && shooter.team != _self.team)
                {
                    Enemy = shooter;
                    if (!IsEnemyInSight())
                    {
                        Enemy = null;
                        continue;
                    }
                    break;
                }
            }
        }

        return HasEnemy();
    }

    public bool HasAmmo_Enemy()
    {
        return Enemy && Enemy.ammo > 0;
    }

    public void Clear_Enemy()
    {
        Enemy = _self.shotBy = null;
    }

    public bool IsVisible_Enemy()
    {
        if (Enemy && Enemy.gameObject)
        {
            foreach (var v in _vision.visibles)
            {
                if (v == Enemy.gameObject)
                {
                    _lastSeenTime = Time.time;
                    _enemyLastSeenPosition = Enemy.transform.position;
                    break;
                }
            }
        }

        return (Time.time - _lastSeenTime) < 0.5f;
    }

    public bool SetDestination_Enemy()
    {
        bool succeeded = false;

        if (Enemy)
        {
            _self.SetDestination(Enemy.transform.position);
            succeeded = true;
        }

        return succeeded;
    }

    public bool SetDestination_Random(float minRadius, float maxRadius, float offset)
    {
        var iterations = -1;
        do {
            iterations++;
            var randomUnits = Random.insideUnitSphere;
            var dst = transform.position + (randomUnits * (maxRadius - minRadius) + new Vector3(
                minRadius * Mathf.Sign(randomUnits.x),
                minRadius * Mathf.Sign(randomUnits.y),
                minRadius * Mathf.Sign(randomUnits.z)));
            dst.y = transform.position.y;
            _self.SetDestination(dst);
        } while (_self.navMeshAgent.remainingDistance > maxRadius + offset && iterations < 15);
        return iterations < 15;
    }

    public bool HasEnemy()
    {
        return Enemy;
    }

    public bool IsEnemyInSight()
    {
        bool hasLoS = false;
        if (Enemy)
        {
            var ignoreList = new List<GameObject>() { gameObject, Enemy.gameObject };
            var src = Enemy.transform.position;
            var dst = _self.destination;
            dst.y = transform.position.y;
            hasLoS = HasLoS(src, dst, ignoreList);
        }

        return hasLoS;
    }

    public bool LastBulletSeenTime_LessThan(float duration)
    {
        float t = Time.time - _vision.lastBulletSeenTime;
        return t < duration;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    bool HasLoS(Vector3 source, Vector3 destination, List<GameObject> ignoreList)
    {
        bool hasLos = true;
        var delta = (destination - source);
        var ray = new Ray(source, delta.normalized);
        var hits = Physics.RaycastAll(ray, delta.magnitude);
        
        Debug.DrawRay(ray.origin, ray.direction * delta.magnitude, Color.red, 0.1f);
        
        foreach (var hit in hits)
        {
            var type = hit.collider.GetComponent<TriggerType>();
            if (!type || !type.collidesWithBullet)
                continue;

            var go = hit.collider.attachedRigidbody
                ? hit.collider.attachedRigidbody.gameObject
                : hit.collider.gameObject;
            if (!ignoreList.Contains(go))
            {
                hasLos = false;
                break;
            }
        }

        return hasLos;
    }

    public void SetDestination(Vector3 destination)
    {
        _self.SetDestination(destination);
    }

    public bool SetDestination_Cover(float searchRadius = 3)
    {
        var possibleCovers = new List<Vector3>();

        int n = 20;
        int s = 10;
        
        bool isSet = false;

        var attacker = _self.shotBy ? _self.shotBy : Enemy;

        if (attacker)
        {
            // Sample random cover points on an increasing circle.
            var src = attacker.transform.position;
            var pos = transform.position;
            var ignoreList = new List<GameObject>() { gameObject, attacker.gameObject };
            while (possibleCovers.Count < n)
            {
                for (int i = 0; i < s; i++)
                {
                    float a = Random.value * Mathf.PI * 2.0f;
                    var dst = pos + new Vector3(Mathf.Cos(a), 0.0f, Mathf.Sin(a)) * searchRadius;

                    if (!HasLoS(src, dst, ignoreList))
                        possibleCovers.Add(dst);
                }

                searchRadius += 2.0f;
            }

            if (possibleCovers.Count == 0) return false;

            // Search the closest cover point
            UnityEngine.AI.NavMeshPath selfPath = new UnityEngine.AI.NavMeshPath();
            UnityEngine.AI.NavMeshPath attackerPath = new UnityEngine.AI.NavMeshPath();
            Vector3 closest = pos;
            float minD = float.PositiveInfinity;
            foreach (var p in possibleCovers)
            {
                if (_self.navMeshAgent.CalculatePath(p, selfPath) &&
                    selfPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                {
                    float attackerDistance = 0.0f;
                    if (attacker && attacker.navMeshAgent && attacker.navMeshAgent.CalculatePath(p, attackerPath))
                        attackerDistance = PathLength(attackerPath);

                    float d = PathLength(selfPath) - attackerDistance * 0.1f;
                    if (d < minD)
                    {
                        minD = d;
                        closest = p;
                    }
                }
            }

            _self.SetDestination(closest);
            isSet = true;
        }

        return isSet;
    }

    private static float PathLength(UnityEngine.AI.NavMeshPath path)
    {
        float d = float.PositiveInfinity;

        if (path != null && path.corners.Length > 1)
        {
            d = 0.0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                var p0 = path.corners[i + 0];
                var p1 = path.corners[i + 1];
                d += Vector3.Distance(p1, p0);
            }
        }

        return d;
    }
}
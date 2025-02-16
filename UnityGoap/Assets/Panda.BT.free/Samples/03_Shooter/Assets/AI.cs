using UnityEngine;
using System.Collections.Generic;

namespace Panda.Examples.Shooter
{
    public class AI : MonoBehaviour
    {
        public Unit enemy { get; protected set; }
        Unit self;
        AIVision vision;

        float random_destination_radius = 1.0f;

        Vector3 enemyLastSeenPosition;

        [Task]
        bool SetTarget_Enemy()
        {
            if (enemy != null)
            {
                self.SetTarget(enemy.transform.position);
                return true;
            }
            return false;
        }

        [Task]
        bool SetTarget_EnemyLastSeenPosition()
        {
            if (enemy != null)
            {
                self.SetTarget(enemyLastSeenPosition);
                return true;
            }
            return false;
        }

        [Task]
        public bool SetTarget_Angle( float angle )
        {
            var p = this.transform.position +  Quaternion.AngleAxis( angle, Vector3.up)*this.transform.forward;
            self.SetTarget(p);
            return true;
        }
        
        [Task]
        public bool SetGlobalTarget_Angle(float angle)
        {
            var p = this.transform.position +  Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            self.SetTarget(p);
            return true;
        }

        float lastEnemyAcquisitionTime = float.NegativeInfinity;
        [Task]
        public void Acquire_Enemy()
        {
            if (Time.time - lastEnemyAcquisitionTime > 0.5f)
            {
                enemy = null;

                if (!enemy && self.shotBy && self.shotBy.team != self.team && (Time.time - self.lastShotTime) < 1.0f)
                    enemy = self.shotBy;

                if (!enemy && vision.visibles != null)
                {
                    foreach (var v in vision.visibles)
                    {
                        if (!v) continue;

                        var shooter = v.GetComponent<Unit>();

                        if (!shooter)
                        {
                            var bullet = v.GetComponent<Bullet>();
                            shooter = bullet && bullet.shooter ? bullet.shooter.GetComponent<Unit>() : null;

                            if (shooter && self.team == shooter.team)
                                shooter = null;
                        }

                        if (shooter && shooter.team != self.team)
                        {
                            enemy = shooter;
                            break;
                        }
                    }
                }
                lastEnemyAcquisitionTime = Time.time;
            }

            ThisTask.Complete(enemy);

        }

        [Task]
        bool HasAmmo_Enemy()
        {
            bool has = false;
            if (enemy != null)
                has = enemy.ammo > 0;
            return has;
        }

        [Task]
        public bool Clear_Enemy()
        {
            enemy = self.shotBy = null;
            return true;
        }

        float lastSeenTime = float.NegativeInfinity;
        [Task]
        public bool IsVisible_Enemy()
        {
            if (enemy && enemy.gameObject)
            {
                foreach (var v in vision.visibles)
                {
                    if (v == enemy.gameObject)
                    {
                        lastSeenTime = Time.time;
                        enemyLastSeenPosition = enemy.transform.position;
                        break;
                    }
                }
            }

            return (Time.time - lastSeenTime) < 0.5f;
        }

        [Task]
        bool SetDestination_Enemy()
        {
            bool succeeded = false;

            if( enemy != null )
            {
                self.SetDestination(enemy.transform.position);
                succeeded = true;
            }
            return succeeded;
        }

        [Task]
        public bool SetDestination_Random(float radius)
        {
            random_destination_radius = radius;
            return SetDestination_Random();
        }

        [Task]
        public bool SetDestination_Random()
        {
            var dst = this.transform.position + (Random.insideUnitSphere * random_destination_radius);
            self.SetDestination(dst);
            return true;
        }

        [Task]
        public bool HasEnemy()
        {
            return enemy;
        }

        [Task]
        bool IsThereLineOfSight_Attacker_Destination()
        {
            bool hasLoS = false;
            var attacker = self.shotBy != null ? self.shotBy : enemy;
            if (attacker != null  )
            {
                var ignoreList = new List<GameObject>() { this.gameObject, attacker.gameObject };
                var src = attacker.transform.position;
                var dst =  self.destination ;
                hasLoS = HasLoS(src, dst, ignoreList);
            }
            return hasLoS;
        }

        [Task]
        bool LastBulletSeenTime_LessThan( float duration )
        {
            float t = Time.time - vision.lastBulletSeenTime;
            if( Task.isInspected )
                ThisTask.debugInfo = string.Format("t={0:0.00}", t);
            return t < duration;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        bool HasLoS( Vector3 source, Vector3 destination, List<GameObject>  ignoreList )
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

        [Task]
        public bool SetDestination_Cover()
        {
            // Search for a cover where the enemy has no line of sight.

            var possibleCovers = new List<Vector3>();
            float searchRadius = 3.0f;

            int n = 20; // Maxiumum number of candidate cover points.
            int s = 10; // Number of sample per circle

            bool isSet = false;

            var  attacker = self.shotBy ? self.shotBy : enemy;

            if (attacker)
            {
                // Sample random cover points on an increasing circle.
                var src = attacker.transform.position;
                var pos = this.transform.position;
                var ignoreList = new List<GameObject>() { this.gameObject, attacker.gameObject };
                while (possibleCovers.Count < n)
                {
                    for (int i = 0; i < s; i++)
                    {
                        float a = Random.value * Mathf.PI * 2.0f;
                        var dst = pos + new Vector3(Mathf.Cos(a), 0.0f ,Mathf.Sin(a)) * searchRadius;

                        if ( ! HasLoS(src, dst, ignoreList) )
                            possibleCovers.Add(dst);

                    }
                    searchRadius += 2.0f;
                }

                // Search the closest cover point
                UnityEngine.AI.NavMeshPath selfPath = new UnityEngine.AI.NavMeshPath();
                UnityEngine.AI.NavMeshPath attackerPath = new UnityEngine.AI.NavMeshPath();
                Vector3 closest = pos;
                float minD = float.PositiveInfinity;
                foreach ( var p in possibleCovers)
                {
                    if( self.navMeshAgent.CalculatePath(p, selfPath) && selfPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete )
                    {
                        float attackerDistance = 0.0f;
                        if (attacker && attacker.navMeshAgent && attacker.navMeshAgent.CalculatePath(p, attackerPath))
                            attackerDistance = PathLength(attackerPath);

                        float d = PathLength(selfPath) - attackerDistance*0.1f;
                        if( d < minD)
                        {
                            minD = d;
                            closest = p;
                        }
                    }
                }

                self.SetDestination(closest);
                isSet = true;
            }

            return isSet;

        }

        static float PathLength( UnityEngine.AI.NavMeshPath path )
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

        // Use this for initialization
        void Start()
        {
            self = this.GetComponent<Unit>();
            vision = this.GetComponentInChildren<AIVision>();
        }

    }
}

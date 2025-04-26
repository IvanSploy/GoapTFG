using System.Threading;
using Panda.Examples.Shooter;
using UnityEngine;
using UnityEngine.AI;
using Task = System.Threading.Tasks.Task;

public class GoapUnit : Unit
{
    private NavMeshAgent _navMeshAgent;
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public new void SetTarget(Vector3 target)
    {
        this.target = target;
        this.target.y = transform.position.y;
    }
    
    public new void SetDestination(Vector3 p)
    {
        destination = p;
        NavMeshPath selfPath = new NavMeshPath();
        if (_navMeshAgent.CalculatePath(p, selfPath) &&
            selfPath.status == NavMeshPathStatus.PathComplete)
        {
            _navMeshAgent.SetPath(selfPath);
        }
        else
        {
            _navMeshAgent.SetDestination(p);
        }
    }
    
    public async Task AimAt_Target(CancellationToken token)
    {
        var targetDelta = (target - transform.position);
        var targetDir = targetDelta.normalized;

        while (targetDelta.magnitude > 0.2f)
        {
            Vector3 axis = Vector3.up * Mathf.Sign(Vector3.Cross(transform.forward, targetDir).y);

            var rot = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, axis);
            transform.rotation = rot * transform.rotation;

            Vector3 newAxis = Vector3.up * Mathf.Sign(Vector3.Cross(transform.forward, targetDir).y);

            float dot = Vector3.Dot(axis, newAxis);

            if (dot < 0.01f) 
            {// We overshooted the target
                var snapRot = Quaternion.FromToRotation(transform.forward, targetDir);
                transform.rotation = snapRot * transform.rotation;
                return;
            }

            var straighUp = Quaternion.FromToRotation(transform.up, Vector3.up);
            transform.rotation = straighUp * transform.rotation;
            await Task.Yield();
            if (token.IsCancellationRequested) return;
        }
    }
    
    public new void Fire()
    {
        var bulletOb = ammo > 0? Instantiate(bulletPrefab): Instantiate(jammedEffectPrefab);

        bulletOb.transform.position = transform.position;
        bulletOb.transform.rotation = transform.rotation;
        if (ammo > 0)
        {
            var bullet = bulletOb.GetComponent<Bullet>();
            bullet.shooter = gameObject;

            ammo--;
            lastReloadTime = Time.time;
        }
        else
        {
            lastReloadTime = Time.time + (1.0f/reloadRate);
            bulletOb.transform.parent = transform;
        }
    }
    
    public void Move()
    {
        _navMeshAgent.isStopped = false;
    }
    
    public new void Stop()
    {
        navMeshAgent.isStopped = true;
    }
        
    public async Task WaitArrival(CancellationToken token)
    {
        while (_navMeshAgent.remainingDistance > 1e-2)
        {
            await Task.Yield();
            if (token.IsCancellationRequested)
            {
                Stop();
                return;
            }
        }
    }
    
    public new void Explode()
    {
        ShooterGameController.instance.OnUnitDestroy(this);

        if (explosionPrefab)
        {
            var explosion = Instantiate(explosionPrefab);
            explosion.transform.position = transform.position;
        }

        Destroy(gameObject);
    }
}

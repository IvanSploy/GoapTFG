using LUGoap.Base;
using LUGoap.Unity;
using Panda.Examples.Shooter;
using TMPro;
using UnityEngine;

public class GoapLinker : MonoBehaviour
{
    [SerializeField] private PropertyManager.PropertyKey _ammoKey;
    [SerializeField] private PropertyManager.PropertyKey _hasEnemyKey;
    [SerializeField] private PropertyManager.PropertyKey _visibleEnemyKey;
    [SerializeField] private PropertyManager.PropertyKey _enemyHpKey;

    private GoapAgent _agent;
    private GoapAI _ai;
    private GoapUnit _self;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<GoapAgent>();
        _ai = GetComponent<GoapAI>();
        _self  = GetComponent<GoapUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        _agent.CurrentState.Set(_ammoKey, _self.ammo);
        _agent.CurrentState.Set(_visibleEnemyKey, _ai.IsEnemyInSight());
        _agent.CurrentState.Set(_enemyHpKey, (int)(_ai.Enemy?.health ?? 0));

        if(!_ai.HasEnemy()) _ai.Acquire_Enemy();
        
        bool hadEnemy = _agent.CurrentState.TryGetOrDefault(_hasEnemyKey, false);
        _agent.CurrentState.Set(_hasEnemyKey, (bool)_ai.Enemy);
        if (hadEnemy != _ai.Enemy) _agent.Interrupt();
    }
}

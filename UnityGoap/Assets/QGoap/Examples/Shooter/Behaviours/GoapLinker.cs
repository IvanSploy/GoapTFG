using System;
using System.Linq;
using LUGoap.Base;
using LUGoap.Unity;
using UnityEngine;

public class GoapLinker : MonoBehaviour
{
    [SerializeField] private PropertyManager.PropertyKey _ammoKey;
    [SerializeField] private PropertyManager.PropertyKey _hasEnemyKey;
    [SerializeField] private PropertyManager.PropertyKey _enemyTypeKey;
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

        if(!_ai.HasEnemy()) _ai.Acquire_Enemy();
        
        bool hadEnemy = _agent.CurrentState.TryGetOrDefault(_hasEnemyKey, false);

        if (_ai.Enemy)
        {
            _agent.CurrentState.Set(_hasEnemyKey, true);
            _agent.CurrentState.Set(_enemyTypeKey, PropertyManager.EnumNames[_enemyTypeKey].First(
                value => _ai.Enemy.name.Contains(value, StringComparison.InvariantCultureIgnoreCase)));
            _agent.CurrentState.Set(_visibleEnemyKey, _ai.IsEnemyInSight());
            _agent.CurrentState.Set(_enemyHpKey, (int)_ai.Enemy.health);
        }
        else
        {
            _agent.CurrentState.Set(_hasEnemyKey, false);
            _agent.CurrentState.Remove(_enemyTypeKey);
            _agent.CurrentState.Remove(_visibleEnemyKey);
            _agent.CurrentState.Remove(_enemyHpKey);
        }
        
        if (hadEnemy != _ai.Enemy) _agent.Interrupt();
    }
}

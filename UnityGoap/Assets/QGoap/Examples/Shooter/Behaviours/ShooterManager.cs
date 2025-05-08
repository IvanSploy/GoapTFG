using System;
using System.Collections.Generic;
using QGoap.Unity;
using Panda.Examples.Shooter;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShooterManager : MonoBehaviour
{
    [SerializeField] private int _seed;
    
    [SerializeField, Range(0f,1f)] private float _spawnProbability = 0.5f;
    [SerializeField, Range(0f,1f)] private float _hunterProbability = 0.5f;
    [SerializeField] private GameObject _hunterPrefab;
    [SerializeField] private GameObject _sniperPrefab;
    [SerializeField] private List<Transform> _enemySpawners;
    [SerializeField] private Patroller[] _patrols;

    [SerializeField] private bool _useLearning;
    [SerializeField] private LearningGoapAgent _goapPlayer;

    private int _totalEnemies;

    private void Awake()
    {
        if (_enemySpawners == null || _enemySpawners.Count == 0)
        {
            _enemySpawners = new List<Transform>();
            for (var i = 0; i < transform.childCount; i++)
            {
                _enemySpawners.Add(transform.GetChild(i));
            }
        }
        
        if (_patrols == null || _patrols.Length == 0)
        {
            _patrols = FindObjectsByType<Patroller>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        }
        
        Random.InitState(_seed == 0 ? DateTime.Now.Ticks.GetHashCode() : _seed);
        
        SpawnEnemies();
        _goapPlayer.UseLearning = _useLearning;
        
        Debug.Log($"[SHOOTER] Total enemies: {_totalEnemies}");
    }

    public void SpawnEnemies()
    {
        foreach (var spawner in _enemySpawners)
        {
            if (Random.value > _spawnProbability) continue;
            Instantiate(Random.value < _hunterProbability ? _hunterPrefab : _sniperPrefab, spawner);
            _totalEnemies++;
        }

        foreach (var patroller in _patrols)
        {
            if (Random.value <= _spawnProbability)
            {
                _totalEnemies++;
                continue;
            }
            Destroy(patroller.gameObject);
        }
    }
}

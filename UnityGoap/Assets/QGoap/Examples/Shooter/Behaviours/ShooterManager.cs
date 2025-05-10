using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using QGoap.Unity;
using Panda.Examples.Shooter;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class ShooterManager : MonoBehaviour
{
    private static int _currentRound;
    
    [SerializeField] private int _rounds;
    private static int _seed;
    
    [SerializeField, Range(0f,1f)] private float _spawnProbability = 0.5f;
    [SerializeField, Range(0f,1f)] private float _hunterProbability = 0.5f;
    [SerializeField] private GameObject _hunterPrefab;
    [SerializeField] private GameObject _sniperPrefab;
    [SerializeField] private List<Transform> _enemySpawners;
    [SerializeField] private Patroller[] _patrols;

    [SerializeField] private LearningGoapAgent _goapPlayer;

    private int _totalEnemies;
    private static readonly List<int> MaxEnemies = new();
    private static readonly List<(int enemies, float time)> RoundsData = new();

    private int _currentEnemies;
    private float _currentTime;
    
    private static readonly string Path = Application.dataPath + "/../Results/" + "Shooter/";
    private readonly string _file = "Shooter";

    private Stopwatch _stopwatch = new();
    
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

        if (_currentRound % 2 == 0) _seed = DateTime.Now.Ticks.GetHashCode();
        Random.InitState(_seed);
        
        SpawnEnemies();
        _goapPlayer.UseLearning = _rounds <= 0 || _currentRound % 2 == 1;
        
        Debug.Log($"[SHOOTER] Total enemies: {GetCurrentEnemies()}");
        
        _stopwatch.Start();
        
        if (_rounds > 0 && _currentRound >= _rounds)
        {
            Save();
            Debug.Break();
            return;
        }
    }

    private void Update()
    {
        if (!_goapPlayer || GetCurrentEnemies() == 0)
        {
            _stopwatch.Stop();
            _currentTime = _stopwatch.ElapsedMilliseconds / 1000f;
            _currentEnemies = GetCurrentEnemies();
            RegisterRound();
            ReloadLevel();
        }
    }

    private int GetCurrentEnemies()
    {
        return ShooterGameController.instance.GetCurrentEnemies();
    }

    private void ReloadLevel()
    {
        Debug.Log($"[SHOOTER] Enemies left: {GetCurrentEnemies()}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void SpawnEnemies()
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

    private void RegisterRound()
    {
        if (_currentRound % 2 == 0)
        {
            MaxEnemies.Add(_totalEnemies);
        }
        
        RoundsData.Add((_currentEnemies, _currentTime));
        _currentRound++;
    }
    
    public void Save()
    {
        var fullPath = GetFullPath();
        CreateFile(fullPath);
        string log = "INDEX;TOTAL ENEMIES;GOAP ENEMIES;GOAP TIME;QGOAP ENEMIES;QGOAP TIME;\n";
        for (var i = 0; i < MaxEnemies.Count; i++)
        {
            var index = i * 2;
            if (index + 1 >= RoundsData.Count) break;
            
            log += $"{i};{MaxEnemies[i]};{RoundsData[index].enemies};{RoundsData[index].time};{RoundsData[index+1].enemies};{RoundsData[index+1].time};\n";
        }

        File.WriteAllText(fullPath, log);
    }
    
    private void CreateFile(string fullPath)
    {
        if (File.Exists(fullPath)) return;
        Directory.CreateDirectory(Path);
        File.Create(fullPath).Close();
    }

    private string GetFullPath()
    {
        return System.IO.Path.Combine(Path, $"{_file}_{GetCurrentDate()}.csv");
    }

    private static string GetCurrentDate()
    {
        var date = DateTime.Now;
        return $"{date.Year}_{date.Month}_{date.Day}__{date.Hour}_{date.Minute}";
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TagManager : MonoBehaviour
{
    public int Rounds = 100;
    public static TagManager Instance;
    
    [SerializeReference]
    private GameObject[] _tagObjects;
    public int TagIndex;
    public float TagCooldown;

    private ITaggable[] _taggables;
    private float _cooldown;

    private readonly List<float> _logTimes = new();
    private float _lastTime = -1;
    
    private static readonly string Path = Application.dataPath + "/../Results/" + "PlayTag/";
    private readonly string _file = "PlayTag";

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _taggables = new ITaggable[_tagObjects.Length];
        for (int i = 0; i < _tagObjects.Length; i++)
        {
            _taggables[i] = _tagObjects[i].GetComponent<ITaggable>();
        }
    }

    private void Start()
    {
        _taggables[TagIndex].Tag(TagCooldown);
        _cooldown = TagCooldown;
    }

    private void Update()
    {
        if(_cooldown > 0) _cooldown -= Time.deltaTime;
    }

    public void Tag()
    {
        if (_cooldown > 0) return;
        if (_lastTime >= 0)
        {
            var elapsedTime = Time.time - _lastTime;
            _logTimes.Add(elapsedTime);
        }
        _lastTime = Time.time;
        var previous = TagIndex;
        TagIndex = (int)Mathf.Repeat(TagIndex + 1, _taggables.Length);
        _taggables[previous].UnTag();

        if (Rounds > 0 && _logTimes.Count >= Rounds)
        {
            Debug.Break();
            return;
        }
        
        _taggables[TagIndex].Tag(TagCooldown);
        _cooldown = TagCooldown;
    }

    private void OnDestroy()
    {
        Debug.Log("Log ready");
        Save();
    }
    
    public void Save()
    {
        var fullPath = GetFullPath();
        CreateFile(fullPath);
        string log = "INDEX;BT;QGOAP;\n";
        var maxPars = _logTimes.Count % 2 == 0 ? _logTimes.Count : _logTimes.Count - 1;
        for (var i = 0; i < maxPars; i+=2)
        {
            var time1 = _logTimes[i];
            var time2 = _logTimes[i+1];
            log += $"{i/2};{time1};{time2};\n";
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

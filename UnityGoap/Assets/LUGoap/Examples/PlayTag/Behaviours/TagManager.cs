using UnityEngine;

public class TagManager : MonoBehaviour
{
    public static TagManager Instance;
    
    [SerializeReference]
    private GameObject[] _tagObjects;
    public int TagIndex;
    public float TagCooldown;

    private ITaggable[] _taggables;
    private float _cooldown;

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
        var previous = TagIndex;
        TagIndex = (int)Mathf.Repeat(TagIndex + 1, _taggables.Length);
        _taggables[previous].UnTag();
        _taggables[TagIndex].Tag(TagCooldown);
        _cooldown = TagCooldown;
    }
}

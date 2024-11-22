using UnityEngine;

public class OpenableBehaviour : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    public bool IsOpen { get; set; }

    private void Awake()
    {
        _animator ??= GetComponent<Animator>();
    }

    public void Open()
    {
        _animator.SetTrigger("Open");
    }
    
    public void Close()
    {
        _animator.SetTrigger("Close");
    }

    public void SetOpen()
    {
        IsOpen = true;
    }
    
    public void SetClosed()
    {
        IsOpen = false;
    }
}
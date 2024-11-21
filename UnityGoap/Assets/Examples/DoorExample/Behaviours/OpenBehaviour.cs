using System;
using UnityEngine;

public class OpenBehaviour : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        _animator ??= GetComponent<Animator>();
    }
}
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialSelector : MonoBehaviour
{
    [SerializeField] private Material[] _materials;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void SetMaterial(int index)
    {
        _renderer.material = _materials[index];
    }
}

using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camera;
    
    private void Awake()
    {
        LoadCamera();
    }

    private void LateUpdate()
    {
        if(!_camera) LoadCamera();
        if (!_camera) return;
        
        transform.rotation = _camera.rotation;
    }
    
    private void LoadCamera()
    {
        var cam = Camera.main;
        if (cam) _camera = cam.transform;
    }
}

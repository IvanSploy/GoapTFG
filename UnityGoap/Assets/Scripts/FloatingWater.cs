using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FloatingWater : MonoBehaviour
{
    public float density = 1;
    
    private void OnTriggerStay(Collider other)
    {
        var t = other.transform;
        var pos = t.position;
        pos.y += Time.fixedDeltaTime * density * 10;
        t.position = pos;
    }
}

using UnityEngine;

namespace Panda.Examples
{
    public class FollowCamera : MonoBehaviour
    {
        public GameObject target;

        Vector3 offset = Vector3.zero;

        // Use this for initialization
        void Start()
        {
            if (target)
            {
                offset = transform.position - target.transform.position;
                offset.x = offset.z = 0.0f;
            }
        }

        // SetParent is called once per frame
        void Update()
        {
            if( target)
                this.transform.position = target.transform.position + offset;
        }
    }
}

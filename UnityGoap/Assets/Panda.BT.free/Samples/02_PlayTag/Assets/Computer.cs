using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Panda.Examples.PlayTag
{
    public class Computer : MonoBehaviour, ITaggable
    {
        public GameObject target;
        public float targetExtent = 3f;
        public float nearRange = 4f;
        public float closeRange = 1f;
        public float speed = 1.0f; // per second.

        public Color it;
        public Color notIt;

        public Dialogue tagDialogue;

        Vector3 destination = Vector3.zero;

        private Renderer _renderer;
        private bool _stopped;

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _renderer.material.color = IsIt ? it : notIt;
        }
        
        #region Tasks

        [Task]
        bool IsIt = false; // Whether the agent is "It".

        [Task]
        void IsColliding_Player()
        {
            float distanceToPlayer = Vector3.Distance(target.transform.position, this.transform.position);
            ThisTask.Complete(  distanceToPlayer < closeRange );
        }

        [Task] 
        private bool IsStopped => _stopped;

        /*
         * Whether the player is near.
         */
        [Task]
        void IsPlayerNear()
        {
            float distanceToPlayer = Vector3.Distance(target.transform.position, this.transform.position);
            ThisTask.Complete(  distanceToPlayer < nearRange );
        }

        /*
        * Pop a text over the agent.
        */
        [Task]
        bool Say(string text)
        {
            tagDialogue.SetText(text);
            tagDialogue.speaker = this.gameObject;
            tagDialogue.ShowText();
            return true;
        }

        /*
         * Move to the destination at the current speed and succeeds when the destination has been reached.
         */
        [Task]
        void MoveToDestination()
        {
            var delta = destination - transform.position;

            if (transform.position != destination)
            {
                var velocity = delta.normalized * speed;
                transform.position = transform.position + velocity * Time.deltaTime;

                // Check for overshooting the destination.
                // Succeed when the destination is reached.
                var newDelta = destination - transform.position;
                if (Vector3.Dot(newDelta, delta) < 0.0f)
                {
                    transform.position = destination;
                }
            }

            if (transform.position == destination)
                ThisTask.Succeed();
        }

        /*
         * Used the current player position  as destination and succeeds.
         */
        [Task]
        bool SetDestination_Player()
        {
            destination = target.transform.position;
            return true;
        }

        /*
         * Used the a random position as destination and succeeds.
         */
        [Task]
        bool SetDestination_Random()
        {
            destination = Random.insideUnitSphere * targetExtent;
            destination.y = 0.0f;
            destination.x = Mathf.RoundToInt(destination.x);
            destination.z = Mathf.RoundToInt(destination.z);

            return true;
        }

        /*
         * Succeeds when the current destination direction is safe. 
         */
        [Task]
        bool IsDirectionSafe
        { 
            get
            {
                Vector3 playerDirection = (target.transform.position - this.transform.position).normalized;
                Vector3 destinationDirection = (destination - this.transform.position).normalized;
                bool isSafe = Vector3.Angle(destinationDirection, playerDirection) > 45.0f;
                return isSafe;
            }
        }

        /*
         * Set the current speed and succeeds. 
         */
        [Task]
        bool SetSpeed( float speed )
        {
            this.speed = speed;
            return true;
        }


         /*
         * Tag and apply the color accordingly.
         */
        [Task]
        bool Tag()
        {
            TagManager.Instance.Tag();
            return true;
        }
        
        public void Tag(float tagCooldown)
        {
            IsIt = true;
            _renderer.material.color = it;
            StartCoroutine(StopCoroutine(tagCooldown));
        }
        
        IEnumerator StopCoroutine(float tagCooldown)
        {
            _stopped = true;
            yield return new WaitForSeconds(tagCooldown);
            _stopped = false;
        }

        public void UnTag()
        {
            IsIt = false;
            _renderer.material.color = notIt;
        }

        #endregion
    }
}

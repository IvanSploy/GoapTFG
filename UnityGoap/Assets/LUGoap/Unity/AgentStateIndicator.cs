using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LUGoap.Unity
{
    [RequireComponent(typeof(GoapAgent))]
    [DisallowMultipleComponent]
    public class AgentStateIndicator : MonoBehaviour
    {
        public enum AgentState
        {
            None,
            Planning,
            Achieved,
            Failed
        }
        
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _planning;
        [SerializeField] private Sprite _achieved;
        [SerializeField] private Sprite _failed;
        [SerializeField] private float _showTime = 0.5f;

        private GoapAgent _goapAgent;

        private AgentState _currentState;
        private Coroutine _removeCoroutine;
        private float _startTime;

        private void Awake()
        {
            _image.enabled = false;

            _goapAgent = GetComponent<GoapAgent>();
            
            _goapAgent.PlanningStarted += () => Set(AgentState.Planning);
            _goapAgent.PlanningEnded += () =>
            {
                if (_currentState == AgentState.Planning)
                {
                    var elapsedTime = _startTime - Time.time;
                    if (elapsedTime >= _showTime)
                    {
                        Clear();
                    }
                    else
                    {
                        RemoveAfterSeconds(_showTime - elapsedTime);
                    }
                    
                }
            };
            _goapAgent.PlanAchieved += () => Set(AgentState.Achieved, 1);
            _goapAgent.PlanFailed += () => Set(AgentState.Failed, 1);
        }

        public void Set(AgentState state, int seconds = 0)
        {
            while (_currentState != AgentState.None) return;
            _currentState = state;
            _startTime = Time.time;
            
            var sprite = state switch
            {
                AgentState.Planning => _planning,
                AgentState.Achieved => _achieved,
                AgentState.Failed => _failed,
                _ => null
            };
            
            if (sprite)
            {
                _image.enabled = true;
                _image.sprite = sprite;
            }
            else
            {
                Clear();
                return;
            }

            if (seconds > 0)
            {
                RemoveAfterSeconds(seconds);
            }
        }

        public void Clear()
        {
            _image.enabled = false;
            _currentState = AgentState.None;
            ClearCoroutine();
        }

        private void RemoveAfterSeconds(float seconds)
        {
            ClearCoroutine();
            _removeCoroutine = StartCoroutine(RemoveAfterSecondsCoroutine(seconds));
        }

        private void ClearCoroutine()
        {
            if(_removeCoroutine == null) return;
            StopCoroutine(_removeCoroutine);
            _removeCoroutine = null;
        }
        
        IEnumerator RemoveAfterSecondsCoroutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Clear();
        }
    }
}

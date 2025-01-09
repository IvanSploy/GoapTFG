using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace LUGoap.Unity
{
    [RequireComponent(typeof(Agent))]
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

        private Agent _agent;

        private void Awake()
        {
            _image.enabled = false;

            _agent = GetComponent<Agent>();
            
            _agent.PlanningStarted += () => Set(AgentState.Planning);
            _agent.PlanningEnded += Clear;
            _agent.PlanAchieved += () => Set(AgentState.Achieved, 1);
            _agent.PlanFailed += () => Set(AgentState.Failed, 1);
        }

        public async void Set(AgentState state, int seconds = 0)
        {
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
                _image.enabled = false;
                return;
            }

            if (seconds > 0)
            {
                await Task.Delay(seconds * 1000);
                Clear();
            }
        }

        public void Clear()
        {
            _image.enabled = false;
        }
    }
}

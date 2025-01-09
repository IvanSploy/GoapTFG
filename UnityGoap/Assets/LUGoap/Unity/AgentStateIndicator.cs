using System.Threading.Tasks;
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

        private GoapAgent _goapAgent;

        private void Awake()
        {
            _image.enabled = false;

            _goapAgent = GetComponent<GoapAgent>();
            
            _goapAgent.PlanningStarted += () => Set(AgentState.Planning);
            _goapAgent.PlanningEnded += Clear;
            _goapAgent.PlanAchieved += () => Set(AgentState.Achieved, 1);
            _goapAgent.PlanFailed += () => Set(AgentState.Failed, 1);
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

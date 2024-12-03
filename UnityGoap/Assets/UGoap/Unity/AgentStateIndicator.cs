using System.Collections.Generic;
using System.Threading.Tasks;
using UGoap.Unity.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UGoap.Unity
{
    [RequireComponent(typeof(UGoapAgent))]
    [DisallowMultipleComponent]
    public class AgentStateIndicator : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private SerializableDictionary<string, Sprite> _indicators;

        private UGoapAgent _agent;
        
        private Dictionary<string, Sprite> _dictionary;

        private void Awake()
        {
            _image.enabled = false;
            _dictionary = _indicators.ToDictionary();

            _agent = GetComponent<UGoapAgent>();
            
            _agent.PlanningStarted += () => Set("Think");
            _agent.PlanningEnded += Clear;
            _agent.PlanAchieved += () => Set("Victory", 1);
            _agent.PlanFailed += () => Set("Fail", 1);
        }

        public async void Set(string key, int seconds = 0)
        {
            _dictionary.TryGetValue(key, out var sprite);
            if (sprite)
            {
                _image.enabled = true;
                _image.sprite = sprite;
            }
            else
            {
                _image.enabled = false;
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

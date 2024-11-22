using System.Collections.Generic;
using UGoap.Unity.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UGoap.Unity
{
    [DisallowMultipleComponent]
    public class UGoapAgentView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private SerializableDictionary<string, Sprite> _indicators;

        private Dictionary<string, Sprite> _dictionary;

        private void Awake()
        {
            _image.enabled = false;
            _dictionary = _indicators.ToDictionary();
        }

        public void Set(string key)
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
        }

        public void Clear()
        {
            _image.enabled = false;
        }
    }
}

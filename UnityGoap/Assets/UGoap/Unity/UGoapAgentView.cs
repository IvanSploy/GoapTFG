using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGoap.Unity
{
    public class UGoapAgentView : MonoBehaviour
    {
        [SerializeField] private Image _icon;

        private void Awake()
        {
            _icon ??= GetComponentInChildren<Image>();
            Hide();
        }

        public void Show()
        {
            var color = _icon.color;
            color.a = 1f;
            _icon.color = color;
        }

        public void Hide()
        {
            var color = _icon.color;
            color.a = 0f;
            _icon.color = color;
        }
    }
}

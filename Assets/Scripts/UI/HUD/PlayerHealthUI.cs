using Strategies.Health;
using UnityEngine;

namespace UI.HUD
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;

        [SerializeField] private RectTransform _healthBar;
        [SerializeField] private RectTransform _healthBarShadow;

        private float _totalWidth;
        private float _totalShadowWidth;

        private void Start()
        {
            _totalWidth = _healthBar.rect.width;
            _totalShadowWidth = _healthBarShadow.rect.width;
        }

        private void Update()
        {
            float fill = (float)_playerHealth.Health / _playerHealth.MaxHealth;

            _healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _totalWidth * fill);
            _healthBarShadow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _totalShadowWidth * fill);
        }
    }
}
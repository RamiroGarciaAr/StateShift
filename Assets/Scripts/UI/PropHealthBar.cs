using Core.Strategies.Health;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PropHealthBar : MonoBehaviour
    {
        [SerializeField] private PropHealth _propHealth;
        [SerializeField] private Slider _healthBar;

        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            _healthBar.value = (float)_propHealth.Health / _propHealth.MaxHealth;

            transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
        }
    }
}
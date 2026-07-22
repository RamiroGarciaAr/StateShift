using Health;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        if (_playerHealth == null)
            _playerHealth = GetComponent<PlayerHealth>();
        if (_playerHealth == null)
        {
            Debug.LogError($"[PlayerDeathHandler] No PlayerHealth on {name}", this);
            enabled = false;
            return;
        }
        _playerHealth.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (EventsManager.Instance != null)
            EventsManager.Instance.ActionGameOver();
        else
            Debug.LogError(
                "[PlayerDeathHandler] No EventsManager in scene — game over won't fire",
                this
            );
    }
}

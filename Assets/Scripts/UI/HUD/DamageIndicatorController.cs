using Health;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DamageIndicatorController : MonoBehaviour
{
    private const float DirectionThresholdSqrMagnitude = 0.0001f;

    [Header("References")]
    [Tooltip("The player's health component that publishes applied-damage feedback.")]
    [SerializeField] private PlayerHealth _playerHealth;

    [Tooltip("The gameplay camera used to convert world-space hit direction into HUD space.")]
    [SerializeField] private Transform _cameraTransform;

    [Tooltip("The dedicated pivot used for fixed camera damage impulses.")]
    [SerializeField] private DamageCameraShake _damageCameraShake;

    [Header("Presentation")]
    [Tooltip("Distance in canvas units from the center at which valid directional indicators are shown.")]
    [SerializeField, Min(0f)] private float _safeRadius = 280f;

    [Tooltip("Seconds required for the indicator to fade from full opacity to invisible.")]
    [SerializeField, Min(0.01f)] private float _fadeDuration = 0.45f;

    private Transform _playerRootTransform;
    private CanvasGroup _canvasGroup;
    private RectTransform _indicatorRectTransform;
    private float _remainingFadeTime;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _indicatorRectTransform = transform as RectTransform;

        if (_playerHealth == null)
            _playerHealth = GetComponentInParent<PlayerHealth>();

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;

        _playerRootTransform = _playerHealth != null ? _playerHealth.transform.root : null;

        if (_playerHealth == null || _canvasGroup == null || _indicatorRectTransform == null || _playerRootTransform == null || _cameraTransform == null)
        {
            Debug.LogError("[DamageIndicatorController] Missing PlayerHealth, CanvasGroup, RectTransform, player root, or camera transform. Disabling component.", this);
            enabled = false;
            return;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDamageAppliedToHealth += PlayDamageReaction;
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDamageAppliedToHealth -= PlayDamageReaction;
    }

    private void Update()
    {
        if (_remainingFadeTime <= 0f)
            return;

        _remainingFadeTime = Mathf.Max(0f, _remainingFadeTime - Time.deltaTime);
        _canvasGroup.alpha = _remainingFadeTime / _fadeDuration;
    }

    /// <summary>
    /// Displays a directional incoming-damage indicator for the supplied applied damage.
    /// </summary>
    public void PlayDamageReaction(DamageInfo damageInfo)
    {
        if (!enabled || damageInfo == null)
            return;

        if (TryGetSourceDirection(damageInfo, out Vector3 sourceDirection))
        {
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(sourceDirection, _cameraTransform.up);
            if (horizontalDirection.sqrMagnitude > DirectionThresholdSqrMagnitude)
            {
                Vector3 cameraLocalDirection = _cameraTransform.InverseTransformDirection(horizontalDirection.normalized);
                float angle = -Mathf.Atan2(cameraLocalDirection.x, cameraLocalDirection.z) * Mathf.Rad2Deg;
                _indicatorRectTransform.anchoredPosition = new Vector2(
                    Mathf.Sin(-angle * Mathf.Deg2Rad) * _safeRadius,
                    Mathf.Cos(-angle * Mathf.Deg2Rad) * _safeRadius
                );
                _indicatorRectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
            else
            {
                SetFallbackPose();
            }
        }
        else
        {
            SetFallbackPose();
        }

        _remainingFadeTime = _fadeDuration;
        _canvasGroup.alpha = 1f;
        _damageCameraShake?.PlayShake();
    }

    private bool TryGetSourceDirection(DamageInfo damageInfo, out Vector3 sourceDirection)
    {
        if (damageInfo.HitDirection.sqrMagnitude > DirectionThresholdSqrMagnitude)
        {
            sourceDirection = -damageInfo.HitDirection.normalized;
            return true;
        }

        sourceDirection = Vector3.zero;
        return false;
    }

    private void SetFallbackPose()
    {
        _indicatorRectTransform.anchoredPosition = new Vector2(0f, _safeRadius);
        _indicatorRectTransform.localRotation = Quaternion.identity;
    }
}

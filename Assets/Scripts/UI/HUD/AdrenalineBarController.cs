using Health;
using UnityEngine;
using UnityEngine.UI;

public sealed class AdrenalineBarController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player adrenaline component that supplies the meter value.")]
    [SerializeField] private PlayerAdrenaline _playerAdrenaline;

    [Tooltip("Ordered adrenaline tier images, from the lowest tier to the highest tier.")]
    [SerializeField] private Image[] _tierBars;

    [Header("Animation")]
    [Tooltip("The normalized fill speed applied to each adrenaline tier.")]
    [SerializeField, Min(0f)] private float _fillSpeed = 2f;

    private float _targetNormalizedValue;

    private void Awake()
    {
        if (_playerAdrenaline == null)
        {
            Debug.LogWarning("[AdrenalineBarController] Player adrenaline reference is missing.", this);
            enabled = false;
            return;
        }

        if (_tierBars == null || _tierBars.Length == 0)
        {
            Debug.LogWarning("[AdrenalineBarController] No adrenaline tier bars are assigned.", this);
            enabled = false;
            return;
        }

        _targetNormalizedValue = _playerAdrenaline.AdrenalineNormalized;
        ApplyFill(_targetNormalizedValue);
    }

    private void Update()
    {
        _targetNormalizedValue = _playerAdrenaline.AdrenalineNormalized;
        ApplyFill(_targetNormalizedValue);
    }

    private void ApplyFill(float normalizedValue)
    {
        float clampedValue = Mathf.Clamp01(normalizedValue);
        float tierSize = 1f / _tierBars.Length;

        for (int tierIndex = 0; tierIndex < _tierBars.Length; tierIndex++)
        {
            Image tierBar = _tierBars[tierIndex];
            if (tierBar == null)
            {
                continue;
            }

            float tierStart = tierIndex * tierSize;
            float targetFill = Mathf.Clamp01((clampedValue - tierStart) / tierSize);
            tierBar.fillAmount = Mathf.MoveTowards(tierBar.fillAmount, targetFill, _fillSpeed * Time.deltaTime);
        }
    }
}

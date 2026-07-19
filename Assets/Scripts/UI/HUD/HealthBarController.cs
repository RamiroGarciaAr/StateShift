using Health;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth health;

    [SerializeField]
    private Image mainHealthBar;

    [SerializeField]
    private Image[] segmentsBar;

    void Awake()
    {
        if (health == null)
            Debug.LogWarning("[HealthBarController] No health set");
        else
        {
            health.OnHealthChanged += OnUpdateHealthUI;
        }
    }

    void OnDestroy()
    {
        health.OnHealthChanged -= OnUpdateHealthUI;
    }

    private void OnUpdateHealthUI(HealthChangeEventArgs args)
    {
        mainHealthBar.fillAmount = health.MainChunkHealthNormalized;
        for (int i = 0; i < health.TotalSideChunks; i++)
        {
            segmentsBar[i].fillAmount = health.GetSideChunkHealthNormalized(i);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Health;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth health;

    [SerializeField]
    private List<Image> healthContainerUI;

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

    private void OnUpdateHealthUI(HealthChangeEventArgs args) { }
}

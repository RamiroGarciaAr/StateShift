using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIAnimationManager : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    
    [Header("Button Mappings")]
    [SerializeField] private List<TriggerMapping> triggerMappings;

    [Header("Back (Esc) Configuration")]
    [SerializeField] private string escTriggerName = "ShowMenu";
    [SerializeField] private bool isEscAllowed = true;

    [Header("Button Reset")]
    [SerializeField] private List<ButtonTextColorChanger> buttonsToReset;

    private void OnEnable()
    {
        if (animator == null)
            Debug.LogError("UI Animation Manager does not have an animator in the component");

        foreach (TriggerMapping mapping in triggerMappings)
        {
            if (mapping.button != null)
            {
                string trigger = mapping.triggerName;
                mapping.button.onClick.AddListener(() => SetTrigger(trigger));
            }
        }
    }

    private void OnDisable()
    {
        // Limpiar listeners
        foreach (TriggerMapping mapping in triggerMappings)
        {
            if (mapping.button != null)
            {
                string trigger = mapping.triggerName;
                mapping.button.onClick.RemoveListener(() => SetTrigger(trigger));
            }
        }
    }

    private void Update()
    {
        if (isEscAllowed && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetTrigger(escTriggerName);
        }
    }

    public void ActivateTrigger(string triggerName)
    {
        SetTrigger(triggerName);
    }

    private void SetTrigger(string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
            Debug.Log($"Trigger activado: {triggerName}");
            
            // Resetear todos los botones a color normal
            ResetAllButtons();
        }
    }

    private void ResetAllButtons()
    {
        foreach (var button in buttonsToReset)
        {
            if (button != null)
            {
                button.ResetToNormal();
            }
        }
    }
}
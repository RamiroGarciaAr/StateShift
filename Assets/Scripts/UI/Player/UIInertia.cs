using System.Collections;
using Core.Events;
using UnityEngine;

public class UIInertia : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform reactivePanel;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Settings Landing")]
    [SerializeField] private float landingImpactAmount = 15f;
    [SerializeField] private float landingReturnSpeed = 10f;
    [SerializeField] private float minVelocityThreshold = 0.1f;

    [Header("Settings Jump")]
    [SerializeField] private float jumpImpactAmount = 20f;
    [SerializeField] private float jumpReturnSpeed = 8f;

    private Vector2 _originalPanelPos;
    private Coroutine _currentCoroutine;
    private const float DISTANCE_PROXIMITY = 0.01f;

    void Start()
    {
        ValidateReferences();
        
        if (reactivePanel != null)
        {
            _originalPanelPos = reactivePanel.anchoredPosition;
        }
        
        if (playerMovement != null)
        {
            playerMovement.OnPlayerLanded += OnPlayerLanded;
            playerMovement.OnPlayerJumped += OnPlayerJumped;
        }
    }

    void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnPlayerLanded -= OnPlayerLanded;
        }
    }
    private void OnPlayerJumped(PlayerJumpingEventArgs args)
    {
        PlayInertiaImpactAnimation(InertiaJumpDirection.Down);
    }

    private void OnPlayerLanded(PlayerLandingEventArgs args)
    {
        if (args.FallVelocity < minVelocityThreshold) return;

        PlayInertiaImpactAnimation(InertiaJumpDirection.Up);
    }

    private void PlayInertiaImpactAnimation(InertiaJumpDirection direction)
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        if (direction == InertiaJumpDirection.Up) _currentCoroutine = StartCoroutine(DoInertiaAnimation(jumpImpactAmount, jumpReturnSpeed, true));
        else _currentCoroutine = StartCoroutine(DoInertiaAnimation(landingImpactAmount, landingReturnSpeed, false));
    }

    private IEnumerator DoInertiaAnimation(float impactAmount, float returnSpeed, bool movingUp)
    {
        Debug.Log($"[UI Inertia]: Starting animation. Dir= {(movingUp ? "Up" : "Down")} | Amount: {impactAmount} | ReturnSpeed : {returnSpeed}");

        Vector2 impactOffset = new Vector2(0, movingUp ? impactAmount : -impactAmount);
        Vector2 targetPos = _originalPanelPos + impactOffset;
        reactivePanel.anchoredPosition = targetPos;
        float elapsed = 0f;
        float maxDuration = 2f;

        while (Vector2.Distance(reactivePanel.anchoredPosition, _originalPanelPos) > DISTANCE_PROXIMITY && elapsed < maxDuration)
        {
            reactivePanel.anchoredPosition = Vector2.Lerp(reactivePanel.anchoredPosition, _originalPanelPos, Time.deltaTime * returnSpeed);

            elapsed += Time.deltaTime;
            yield return null;
        }

        reactivePanel.anchoredPosition = _originalPanelPos;
    }
    private enum InertiaJumpDirection
    {
        Up,
        Down
    }

    private void ValidateReferences()
    {
        if (reactivePanel == null)
            Debug.LogError("[UI Inertia]: Missing Reactive Panel Component", this);
            
        if (playerMovement == null)
            Debug.LogError("[UI Inertia]: Missing Player Movement reference", this);
    }
}
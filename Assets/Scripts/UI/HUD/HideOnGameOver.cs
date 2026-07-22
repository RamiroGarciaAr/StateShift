using UnityEngine;

/// <summary>
/// Hides the gameplay HUD when the game-over event fires by driving a <see cref="CanvasGroup"/>,
/// keeping the GameObject active so any nested managers (e.g. the EventSystem) keep running while
/// the death screen is shown. Follows the same event-driven pattern as <see cref="GameOverMenu"/>.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class HideOnGameOver : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        EventsManager.Instance.OnGameOver += GameOverListener;
    }

    private void OnDestroy()
    {
        if (EventsManager.Instance != null)
        {
            EventsManager.Instance.OnGameOver -= GameOverListener;
        }
    }

    private void GameOverListener()
    {
        EventsManager.Instance.OnGameOver -= GameOverListener;

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}

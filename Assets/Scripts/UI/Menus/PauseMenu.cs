using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _exitButton;

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
    }

    private void Start()
    {
        _resumeButton.onClick.AddListener(OnResume);
        _exitButton.onClick.AddListener(OnExit);

        EventsManager.Instance.OnGamePause += GamePauseListener;
        EventsManager.Instance.OnGameOver += GameOverListener;
    }

    private void OnResume()
    {
        EventsManager.Instance.ActionGamePause(false);
    }

    private void OnExit()
    {
        EventsManager.Instance.ActionGameExit();
    }

    private void GamePauseListener(bool isPaused)
    {
        _canvas.enabled = isPaused;
    }

    private void GameOverListener()
    {
        EventsManager.Instance.OnGamePause -= GamePauseListener;
        EventsManager.Instance.OnGameOver -= GameOverListener;
    }
}
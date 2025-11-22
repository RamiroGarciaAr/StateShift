using Managers;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
    }

    private void Start()
    {
        _restartButton.onClick.AddListener(OnRestart);
        _exitButton.onClick.AddListener(OnExit);

        EventsManager.Instance.OnGameOver += GameOverListener;
    }

    private void OnRestart()
    {
        EventsManager.Instance.ActionGameRestart();
    }

    private void OnExit()
    {
        EventsManager.Instance.ActionGameExit();
    }

    private void GameOverListener()
    {
        EventsManager.Instance.OnGameOver -= GameOverListener;

        _canvas.enabled = true;
    }
}
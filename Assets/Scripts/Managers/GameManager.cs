using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPlaying { get => _isPlaying; }

    private bool _isPlaying = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    private void Start()
    {
        EventsManager.Instance.OnGamePause += GamePauseListener;
        EventsManager.Instance.OnGameOver += GameOverListener;
        EventsManager.Instance.OnGameExit += GameExitListener;
        EventsManager.Instance.OnGameRestart += GameRestartListener;

        Time.timeScale = 1;
    }

    private void GamePauseListener(bool isPaused)
    {
        _isPlaying = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }

    private void GameOverListener()
    {
        StopGame();
    }

    public void GameExitListener()
    {
        StopGame();

        SceneLoader.SwitchSceneAsync("MainMenuScene");
    }

    public void GameRestartListener()
    {
        StopGame();

        SceneLoader.ReloadSceneAsync();
    }

    private void StopGame()
    {
        EventsManager.Instance.OnGamePause -= GamePauseListener;
        EventsManager.Instance.OnGameOver -= GameOverListener;

        _isPlaying = false;
        Time.timeScale = 0;
    }
}

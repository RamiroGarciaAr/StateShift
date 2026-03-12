using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
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
        Time.timeScale = isPaused ? 0 : 1;
    }

    private void GameOverListener()
    {
        EventsManager.Instance.OnGamePause -= GamePauseListener;
        EventsManager.Instance.OnGameOver -= GameOverListener;

        Time.timeScale = 0;
    }

    public void GameExitListener()
    {
        SceneLoader.OpenLoadingScene("MainMenuScene");
    }

    public void GameRestartListener()
    {
        SceneLoader.OpenLoadingScene("SampleScene");
    }
}

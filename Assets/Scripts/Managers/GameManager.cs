using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private const string MainMenuSceneName = "MainMenuScene";

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

        // Unlock and reveal the cursor so the Game Over UI buttons are clickable.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Restores normal time flow and returns to the main menu via the loading scene.
    /// </summary>
    public void GameExitListener()
    {
        Time.timeScale = 1;
        SceneLoader.OpenLoadingScene(MainMenuSceneName);
    }

    /// <summary>
    /// Restores normal time flow and reloads the currently active scene so the player
    /// retries the level they died in.
    /// </summary>
    public void GameRestartListener()
    {
        Time.timeScale = 1;
        string currentScene = SceneManager.GetActiveScene().name;
        SceneLoader.OpenLoadingScene(currentScene);
    }
}

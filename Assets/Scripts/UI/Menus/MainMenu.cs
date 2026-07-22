using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires the main menu buttons to their actions. Both Continue and New Game
/// load the gameplay scene through the loading screen, while Quit exits the app.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [Tooltip("Resumes play. Loads the gameplay scene through the loading screen.")]
    [SerializeField] private Button _continueButton;

    [Tooltip("Starts a fresh run. Loads the gameplay scene through the loading screen.")]
    [SerializeField] private Button _newGameButton;

    [Tooltip("Quits the application.")]
    [SerializeField] private Button _quitButton;

    [Header("Scene")]
    [Tooltip("Name of the gameplay scene to load. Must be added to Build Settings.")]
    [SerializeField] private string _gameScene = "ShootingRange";

    private void Awake()
    {
        _continueButton.onClick.AddListener(OnPlay);
        _newGameButton.onClick.AddListener(OnPlay);
        _quitButton.onClick.AddListener(OnQuit);
    }

    private void OnDestroy()
    {
        _continueButton.onClick.RemoveListener(OnPlay);
        _newGameButton.onClick.RemoveListener(OnPlay);
        _quitButton.onClick.RemoveListener(OnQuit);
    }

    /// <summary>
    /// Loads the configured gameplay scene through the loading screen.
    /// </summary>
    public void OnPlay()
    {
        SceneLoader.OpenLoadingScene(_gameScene);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void OnQuit()
    {
        Application.Quit();
    }
}

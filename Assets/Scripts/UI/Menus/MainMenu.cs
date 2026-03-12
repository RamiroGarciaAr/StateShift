using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _testButton;

    [SerializeField] private string _gameScene;

    private void Start()
    {
        _playButton.onClick.AddListener(OnPlay);
        _quitButton.onClick.AddListener(OnQuit);
        _testButton.onClick.AddListener(() => SceneLoader.OpenLoadingScene("PlayerTesting"));
    }

    public void OnPlay()
    {
        SceneLoader.OpenLoadingScene(_gameScene);
    }

    public void OnQuit()
    {
        Debug.Log("OnQuit");
        Application.Quit();
    }
}

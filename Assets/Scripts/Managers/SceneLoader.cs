using UnityEngine;
using UnityEngine.SceneManagement;

static class SceneLoader
{
    private static string _nextScene;

    public static void OpenLoadingScene(string nextSceneName)
    {
        _nextScene = nextSceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    public static AsyncOperation LoadNextAsync()
    {
        return SceneManager.LoadSceneAsync(_nextScene);
    }

    public static AsyncOperation CloseLoadingScene()
    {
        return SceneManager.UnloadSceneAsync("LoadingScene");
    }
}
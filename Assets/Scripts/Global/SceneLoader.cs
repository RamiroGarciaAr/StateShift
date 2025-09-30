using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global loading scene management.
/// </summary>
static class SceneLoader
{
    private static string _nextScene;

    /// <summary>
    /// Asynchronously loads LoadingScene with the next scene to load.
    /// </summary>
    /// <param name="sceneName">Next scene to load.</param>
    /// <returns>LoadingScene async load operation.</returns>
    public static AsyncOperation SwitchSceneAsync(string sceneName)
    {
        _nextScene = sceneName;
        return SceneManager.LoadSceneAsync("LoadingScene");
    }

    /// <summary>
    /// Asynchronously loads LoadingScene with the current scene to load.
    /// </summary>
    /// <returns>LoadingScene async load operation.</returns>
    public static AsyncOperation ReloadSceneAsync()
    {
        return SwitchSceneAsync(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Asynchronously loads the next scene.
    /// </summary>
    /// <remarks>DO NOT CALL OUTSIDE OF LoadingScene.</remarks>
    /// <returns>Next scene async load operation.</returns>
    public static AsyncOperation LoadNextAsync()
    {
        return SceneManager.LoadSceneAsync(_nextScene);
    }
}
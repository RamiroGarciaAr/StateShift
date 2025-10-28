using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;

    private AsyncOperation _loadingOperation;

    private void Start()
    {
        _loadingOperation = SceneLoader.LoadNextAsync();
    }

    private void Update()
    {
        float progress = _loadingOperation.progress;

        _progressBar.value = progress;
        _progressText.text = $"{progress}%";
    }
}

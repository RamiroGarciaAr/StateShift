using UnityEngine;
using TMPro; // o UnityEngine.UI si usás Text

public class InteractionPromptUI : MonoBehaviour {
    [SerializeField] private PlayerInteractor interactor; 
    [SerializeField] private TMP_Text promptText;

    void Awake() {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
    void OnEnable() {
        if (interactor == null) return;
        interactor.OnShowPrompt += HandleShow;
        interactor.OnHidePrompt += HandleHide;
    }
    void OnDisable() {
        if (interactor == null) return;
        interactor.OnShowPrompt -= HandleShow;
        interactor.OnHidePrompt -= HandleHide;
    }
    void HandleShow(string msg) {
        if (promptText == null) return;
        promptText.text = msg;
        promptText.gameObject.SetActive(true);
    }
    void HandleHide() {
        if (promptText == null) return;
        promptText.gameObject.SetActive(false);
    }
}

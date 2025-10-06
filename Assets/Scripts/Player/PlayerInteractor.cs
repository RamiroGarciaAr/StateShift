using UnityEngine;
public class PlayerInteractor : MonoBehaviour {
    [Header("Raycast")]
    [SerializeField] private Camera playerCam;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    // UI opcional (conectá eventos a tu HUD)
    public System.Action<string> OnShowPrompt;
    public System.Action OnHidePrompt;

    private IInteractable _current;

    void Update() {
        Detect();
    }

    void Detect() {
        _current = null;
        if (!playerCam) {
            Debug.LogWarning("PlayerInteractor: No camera assigned!");
            return;
        }

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        
        // DEBUG: Visualizar el ray en Scene view
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 0.1f);
        
        if (Physics.Raycast(ray, out var hit, maxDistance, interactMask, QueryTriggerInteraction.Ignore)) {
            
            if (hit.collider.TryGetComponent<IInteractable>(out var it)) {
                _current = it;
                OnShowPrompt?.Invoke(it.Prompt);
                return;
            }
        } 
        OnHidePrompt?.Invoke();
    }
    public void TryInteract(GameObject instigator) {
        _current?.Interact(instigator);
    }
}
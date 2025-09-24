using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteractable : MonoBehaviour, IInteractable {
    [SerializeField] private InteractionAction action; // tu SO de acción única
    [SerializeField] private string prompt = "E: Usar";

    private AudioManager audioManager; 
     private const string SOUND_NAME = "Button_SFX";

    public string Prompt => prompt;

    private void Start() {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void Interact(GameObject instigator)
    {
        action?.Execute(instigator);
        audioManager?.Play(SOUND_NAME);

    }
}

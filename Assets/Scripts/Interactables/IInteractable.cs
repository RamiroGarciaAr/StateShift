using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    string Prompt { get; } // texto para el HUD (E: Abrir / E: Usar)
    void Interact(GameObject instigator = null);
}

using UnityEngine;
public abstract class InteractionAction : ScriptableObject {
    public abstract void Execute(GameObject instigator);
}
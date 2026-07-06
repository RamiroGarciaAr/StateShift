using System;
using UnityEngine;

public abstract class ChannelSO<T> : ScriptableObject
{
    // 'event' keyword: outsiders can only += / -=, they cannot invoke it
    // or overwrite it with =. Only THIS class can Raise it. That's the
    // encapsulation the raw blog examples skip.
    public event Action<T> OnRaised;

    // Called by the raiser (EnemyHealth). The ?. means "if nobody is
    // subscribed, do nothing" — a raise with zero listeners is a safe no-op.
    public void Raise(in T evt)
    {
        OnRaised?.Invoke(evt);
    }

    // If a listener didn't unsubscribe cleanly, its dead reference
    // could linger here into the next session. Clearing on OnDisable gives a
    // clean slate each time the SO unloads (which happens on domain reload /
    // play-mode exit). This is the idiomatic self-clean.
    private void OnDisable()
    {
        OnRaised = null;
    }
}

using UnityEngine;

public interface IPickupEffect
{
    bool CanApply(GameObject collector);
    void Apply(GameObject collector);
    void SetAudioPool(AudioPool pool);
}

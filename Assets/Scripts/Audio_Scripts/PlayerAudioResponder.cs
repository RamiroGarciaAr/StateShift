using Health;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerAudioResponder : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private AudioPool _audioPool;

    [Header("Sounds")]
    [SerializeField]
    private Sound characterScreamSound;

    [SerializeField]
    private Sound impactSound;

    [SerializeField]
    private Sound chunkBrokenSound;
    private PlayerHealth _health;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
        if (_audioPool == null)
        {
            Debug.LogError("[PlayerAudioResponder] Could not find AudioPool");
            enabled = false;
            return;
        }
        if (characterScreamSound == null || impactSound == null || chunkBrokenSound == null)
        {
            Debug.LogError("[PlayerAudioResponder] Could not find Sounds");
            enabled = false;
            return;
        }

        _health.OnHealthChanged += HandleHealthChanged;
        _health.OnChunkDepleted += HandleBrokenChunk;
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= HandleHealthChanged;
            _health.OnChunkDepleted -= HandleBrokenChunk;
        }
    }

    private void HandleHealthChanged(HealthChangeEventArgs args)
    {
        //recieve dmg
        if (args.PreviousHealth > args.CurrentHealth)
        {
            _audioPool.PlayAt(characterScreamSound, pitchRandomization: false, is3D: false);
            _audioPool.PlayAt(impactSound, is3D: false);
        }
    }

    private void HandleBrokenChunk(int idx)
    {
        _audioPool.PlayAt(chunkBrokenSound, is3D: false);
    }
}

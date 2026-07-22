using System.Collections.Generic;
using Gaskellgames;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class AudioPool : MonoBehaviour
{
    [SerializeField, Tooltip("AudioSource prefab: playOnAwake OFF, spatialBlend handled per-play")]
    private AudioSource _prefab;

    [SerializeField]
    private int _defaultCapacity = 10;

    [SerializeField]
    private int _maxCapacity = 30;

    private ObjectPool<AudioSource> _pool;

    // Active sources awaiting release, with the time they finish playing.
    private readonly List<ActiveSource> _active = new();

    private struct ActiveSource
    {
        public AudioSource Source;
        public float FreeAt;
    }

    private void Awake()
    {
        if (_prefab == null)
        {
            Debug.LogError($"[AudioPool] No AudioSource prefab assigned on {name}", this);
            enabled = false;
            return;
        }

        _pool = new ObjectPool<AudioSource>(
            createFunc: () => Instantiate(_prefab, transform),
            actionOnGet: src => src.gameObject.SetActive(true),
            actionOnRelease: src => src.gameObject.SetActive(false),
            actionOnDestroy: src => Destroy(src.gameObject),
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxCapacity
        );
    }

    /// <summary>
    /// Plays a positioned one-shot. Null sound or null clip is silent (not an error).
    /// </summary>
    public void PlayAt(
        Sound s,
        Vector3? pos = null,
        bool pitchRandomization = true,
        bool is3D = true
    )
    {
        if (s == null)
            return;
        AudioClip clip = s.GetClip(); // get Clip from sounds
        if (clip == null)
            return;

        AudioSource src = _pool.Get();

        if (is3D == true && pos != null)
            src.transform.position = pos.Value;
        src.clip = clip;
        src.volume = s.volume;
        src.loop = false; // one-shots never loop, regardless of the Sound's authored flag
        float rand_pitch = pitchRandomization ? Random.Range(-0.05f, 0.05f) : 0f;
        src.pitch = s.pitch + rand_pitch; // per-shot pitch variation
        src.spatialBlend = is3D ? 1f : 0f;
        src.Play();

        // Release when the clip finishes. Divide by pitch: a higher pitch plays shorter.
        float duration = clip.length / Mathf.Max(0.01f, src.pitch);
        _active.Add(new ActiveSource { Source = src, FreeAt = Time.time + duration });
    }

    private void Update()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            if (Time.time >= _active[i].FreeAt)
            {
                _pool.Release(_active[i].Source);
                _active.RemoveAt(i);
            }
        }
    }
}

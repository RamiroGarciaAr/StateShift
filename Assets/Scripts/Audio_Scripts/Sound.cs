using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string Name;
    public AudioClip[] clips;
    public bool loop = false;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;
    internal AudioSource source;

    public AudioClip GetClip()
    {
        if (clips == null || clips.Length == 0)
            return null;
        if (clips.Length == 1)
            return clips[0];
        return clips[Random.Range(0, clips.Length)];
    }
}

using UnityEngine;

public readonly struct PerceptionResult
{
    public readonly bool IsVisible;
    public readonly Vector3 Position;
    public static readonly PerceptionResult Miss = new(false, default);

    public PerceptionResult(bool visible, Vector3 position)
    {
        IsVisible = visible;
        Position = position;
    }
}

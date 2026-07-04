using UnityEngine;

[System.Serializable] // So we can see the class in the inspector
public class SecondOrderDynamics
{
    private Vector3 _xp;
    private Vector3 _y,
        _yd; // y is the output, yd is the derivative of y

    private float k1,
        k2,
        k3; // Constants for the second-order dynamics

    public Vector3 Value => _y;

    public SecondOrderDynamics() { }

    public SecondOrderDynamics(float f, float z, float r, Vector3 x0)
    {
        Initialize(x0);
        ComputeConstants(f, z, r);
    }

    public void Initialize(Vector3 x0)
    {
        _xp = x0;
        _y = x0;
        _yd = Vector3.zero;
    }

    public void ComputeConstants(float f, float z, float r)
    {
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);
    }

    public Vector3 Update(float deltaTime, Vector3 x, Vector3? xd = null)
    {
        if (deltaTime <= 0f)
            return _y;
        if (xd == null) // we estimate velocity if it's not provided
        {
            xd = (x - _xp) / deltaTime;
        }
        _xp = x;
        float k2Stable = Mathf.Max(
            k2,
            (deltaTime * deltaTime) / 4 + (deltaTime * k1) / 2,
            deltaTime * k1
        );
        _y += _yd * deltaTime;
        _yd += deltaTime * (x + k3 * (Vector3)xd - _y - k1 * _yd) / k2Stable;

        return _y;
    }
}

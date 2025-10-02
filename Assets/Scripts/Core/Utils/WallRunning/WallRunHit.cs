using UnityEngine;

/// <summary>
/// Estructura que contiene información sobre la detección de una pared para wall running
/// </summary>
public struct WallRunHit
{
    public bool hit;           // ¿Se detectó una pared?
    public Vector3 normal;     // Normal de la pared
    public Vector3 point;      // Punto de contacto
    public bool isRight;       // ¿La pared está a la derecha?
    public float distance;     // Distancia a la pared

    public WallRunHit(bool hit, Vector3 normal, Vector3 point, bool isRight, float distance)
    {
        this.hit = hit;
        this.normal = normal;
        this.point = point;
        this.isRight = isRight;
        this.distance = distance;
    }

    public static WallRunHit NoHit()
    {
        return new WallRunHit(false, Vector3.zero, Vector3.zero, false, 0f);
    }
}
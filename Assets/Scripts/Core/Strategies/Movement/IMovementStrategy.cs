using UnityEngine;

public interface IMovementStrategy
{
    void ApplyMovement(Rigidbody rb, Vector3 inputDirection, float maxSpeed, bool isGrounded);
}

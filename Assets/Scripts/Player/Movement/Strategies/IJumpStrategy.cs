using UnityEngine;

public interface IJumpStrategy
{
    void ApplyJump(Rigidbody rb, float jumpHeight);
    void SetJumpHeld(bool held);
    void UpdateJump(Rigidbody rb);
    bool CanJump(bool isGrounded);
}
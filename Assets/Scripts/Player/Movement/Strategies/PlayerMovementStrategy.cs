using UnityEngine;

[System.Serializable]
public class PlayerMovementStrategy : IMovementStrategy
{
    [Header("Velocity Movement Settings")]
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float airAcceleration = 24f;
    [SerializeField] private float airControlFactor = 0.4f;
    
    public void ApplyMovement(Rigidbody rb, Vector3 inputDirection, float maxSpeed, bool isGrounded)
    {
        Vector3 targetVelocity = inputDirection * maxSpeed;
        Vector3 currentVelocity = new(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 velocityDifference = targetVelocity - currentVelocity;
    
        float acceleration = isGrounded ? groundAcceleration : airAcceleration;
        
        if (!isGrounded)
        {
            velocityDifference *= airControlFactor;
        }
        
        float maxVelocityChange = acceleration * Time.fixedDeltaTime;
        Vector3 velocityChange = Vector3.ClampMagnitude(velocityDifference, maxVelocityChange);
        
        Vector3 newVelocity = currentVelocity + velocityChange;
        rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
    }
}
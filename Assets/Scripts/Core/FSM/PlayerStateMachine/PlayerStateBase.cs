using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerStateBase : BaseState<PlayerController>
{
    protected Rigidbody rb => Context.Rb;
    protected Transform orientation => Context.Orientation;
    
    // Configuración común
    protected float WalkSpeed => Context.WalkSpeed;
    protected float SprintSpeed => Context.SprintSpeed;
    protected float GroundAcceleration => Context.GroundAcceleration;
    protected float AirAcceleration => Context.AirAcceleration;
    protected float AirControlFactor => Context.AirControlFactor;
    
    // Estado del player
    protected bool IsSprinting => Context.IsSprinting;
    protected Vector2 MoveInput => Context.MoveInput;
    protected bool IsGrounded => Context.IsGrounded;

    protected PlayerStateBase(PlayerController context) : base(context) { }

    protected Vector3 CalculateMovementDirection(Vector2 input)
    {
        Vector3 forward = Vector3.ProjectOnPlane(orientation.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(orientation.right, Vector3.up).normalized;
        Vector3 direction = right * input.x + forward * input.y;
        
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();
            
        return direction;
    }
    protected void ApplyMovement(Vector3 inputDirection, float maxSpeed, float acceleration)
    {
        Vector3 targetVelocity = inputDirection * maxSpeed;
        Vector3 currentVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 velocityDifference = targetVelocity - currentVelocity;
        
        float maxVelocityChange = acceleration * Time.fixedDeltaTime;
        Vector3 velocityChange = Vector3.ClampMagnitude(velocityDifference, maxVelocityChange);
        
        Vector3 newVelocity = currentVelocity + velocityChange;
        rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
    }

    public virtual void CheckTransitions()
    {
        // Los estados hijos implementan sus transiciones específicas
    }
}

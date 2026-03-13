using System.Collections;
using System.Collections.Generic;
using Strategies;
using UnityEngine;

namespace Core
{
    public enum MovementState
    {
        // Outer states
        Grounded,
        WallRunning,
        Dashing,
        Grappling,
        InAir,
        // Grounded sub-states
        Walking,
        Sprinting,
        Crouching,
        Sliding
    }
    public class PlayerMovementContext
    {
        // Referencias a componentes
        public IControllable Controllable { get; set; }
        public PlayerMovement PlayerMovement { get; set; }
        public PlayerCrouch PlayerCrouch { get; set; }
        public PlayerSlide PlayerSlide { get; set; }
        public PlayerDash PlayerDash { get; set; }
        public PlayerWallRun PlayerWallRun { get; set; }
        public Rigidbody Rigidbody { get; set; }
        public PlayerGrapple PlayerGrapple { get; set; }

        // State Machines
        public StateMachine<MovementState> StateMachine { get; set; }
        public StateMachine<MovementState> GroundedStateMachine { get; set; }

        // Input States
        public Vector2 MovementInput { get; set; }
        public Vector2 DashInputDirection { get; set; } // Lo guardo para que no cambie una vez que lo ejecuto

        public bool WantsToSprint { get; set; }
        public bool WantsToCrouch { get; set; }
        public bool WantsToJump { get; set; }
        public bool WantsToGrapple { get; set; }
        public bool WantsToDash { get; set; }

    }
}
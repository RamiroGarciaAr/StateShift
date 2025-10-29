using System.Collections;
using System.Collections.Generic;
using Strategies;
using UnityEngine;

namespace Core
{
    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching,
        Sliding,
        WallRunning,
        Dashing
    }
    public class PlayerMovementContext
    {
        // Referencias a componentes
        public IControllable Controllable { get; set; }
        public PlayerMovement PlayerMovement { get; set; }
        public PlayerCrouch PlayerCrouch { get; set; }
        public PlayerSlide PlayerSlide { get; set; }
        public PlayerWallRun PlayerWallRun { get; set; }
        public Rigidbody Rigidbody { get; set; }

        // State Machine
        public StateMachine<MovementState> StateMachine { get; set; }

        // Input States
        public Vector2 MovementInput { get; set; }
        public bool WantsToSprint { get; set; }
        public bool WantsToCrouch { get; set; }
        public bool WantsToJump { get; set; }
        public bool WantsToGrapple { get; set; }
        public bool WantsToDash { get; set; }
    }
}
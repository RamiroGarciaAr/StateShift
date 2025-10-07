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
        Sliding
    }
    public class PlayerMovementContext
    {
        public IControllable Controllable;
        public PlayerCrouch PlayerCrouch;
        public PlayerSlide PlayerSlide;
        public Rigidbody Rigidbody;
        public StateMachine<MovementState> StateMachine;

        // Inputs
        public bool WantsToCrouch;
        public bool WantsToSprint;
        public bool WantsToJump;
        public Vector2 MovementInput;
    }
    
}
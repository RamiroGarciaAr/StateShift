using UnityEngine;

namespace Core.Events
{
    //Data that is called when the PlayerLands -> this is for the future
    public class PlayerLandingEventArgs
    {
        public float FallVelocity { get; } // @TODO: fall dmg ??
        // @TODO: Particles
        public Vector3 LandingPoing { get; }
        public Vector3 LandingNormal { get; }

        public PlayerLandingEventArgs(float fallVelocity, Vector3 landingPoint, Vector3 landingNormal)
        {
            FallVelocity = fallVelocity;
            LandingPoing = landingPoint;
            LandingNormal = landingNormal;
        }
    }
    public delegate void PlayerLandingEvent(PlayerLandingEventArgs args); //OKAY WTF

}

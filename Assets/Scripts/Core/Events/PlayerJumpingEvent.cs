namespace Core.Events
{

    public class PlayerJumpingEventArgs
    {
        public float JumpForce { get; }

        public PlayerJumpingEventArgs(float jumpForce)
            {
                JumpForce = jumpForce;
            }
        }



    public delegate void PlayerJumpingEvent(PlayerJumpingEventArgs args);

}

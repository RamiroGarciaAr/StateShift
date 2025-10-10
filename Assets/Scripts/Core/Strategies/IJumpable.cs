
namespace Strategies
{
    public interface IJumpable
    {
        public float JumpForce { get; set; }
        public void Jump();
        public void SetHoldingJump(bool holding);
    }

}

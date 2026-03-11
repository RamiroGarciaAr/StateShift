public interface ITickable 
{
    // <summary>
    /// Interface for objects that need to be updated every frame by a TickManager.
    /// </summary>
    void OnTick(float deltaTime);


    bool IsTickable {get;}
}

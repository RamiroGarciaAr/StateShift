
public interface IState
{
    //Se llama en el momento en que se entra al estado
    void OnEnter();
    //Se llama en cada frame
    void OnUpdate();
    //Se llama en cada frame fijo
    void OnFixedUpdate();
    //Se llama en el momento en que se sale del estado
    void OnExit();
}

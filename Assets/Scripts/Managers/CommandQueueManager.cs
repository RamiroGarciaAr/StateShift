using System.Collections.Generic;
using UnityEngine;

public sealed class CommandQueueManager : MonoBehaviour
{
    public static CommandQueueManager Instance { get; private set; }

    private Queue<ICommand> _commandQueue = new Queue<ICommand>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    private void Update()
    {
        ExecuteCommands(_commandQueue);

        _commandQueue.Clear();
    }

    private void ExecuteCommands(IEnumerable<ICommand> commands)
    {
        foreach (ICommand command in commands)
        {
            command.Execute();
        }
    }

    public void EnqueueCommand(ICommand command)
    {
        _commandQueue.Enqueue(command);
    }
}
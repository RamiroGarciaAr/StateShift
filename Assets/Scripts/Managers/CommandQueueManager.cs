using System.Collections.Generic;
using UnityEngine;

public sealed class CommandQueueManager : MonoBehaviour
{
    public static CommandQueueManager Instance { get; private set; }

    private readonly Queue<ICommand> _commandQueue = new();

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
        while (_commandQueue.Count > 0)
        {
            _commandQueue.Dequeue().Execute();
        }
    }

    public void EnqueueCommand(ICommand command)
    {
        _commandQueue.Enqueue(command);
    }
}
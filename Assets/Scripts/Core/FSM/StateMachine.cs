using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<TState> where TState : Enum
{
    private readonly Dictionary<TState, IState> states = new();
    private IState _currentState;
    private TState _currentStateType;

    public IState CurrentState => _currentState;
    public TState CurrentStateType => _currentStateType;
    public void Initialize(TState initialState)
    {
        if (!states.ContainsKey(initialState))
        {
            Debug.LogError($"Estado inicial {initialState} no registrado");
            return;
        }

        _currentStateType = initialState;
        _currentState = states[initialState];
        _currentState?.OnEnter();
    }
    public void Update()
    {
        _currentState?.OnUpdate();
    }
    public void FixedUpdate()
    {
        _currentState?.OnFixedUpdate();
    }

    public void RegisterState(TState stateType, IState state)
    {
        if (states.ContainsKey(stateType))
        {
            Debug.LogWarning($"Estado {stateType} ya registrado. Sobrescribiendo...");
        }
        states[stateType] = state;
    }

    public void ChangeState(TState newStateType)
    {
        if (!states.ContainsKey(newStateType))
        {
            Debug.LogError($"Estado {newStateType} no registrado en la StateMachine");
            return;
        }

        // Exit del estado actual
        _currentState?.OnExit();

        // Cambiar al nuevo estado
        _currentStateType = newStateType;
        _currentState = states[newStateType];

        // Enter del nuevo estado
        _currentState?.OnEnter();
    }

    public bool IsInState(TState stateType)
    {
        return EqualityComparer<TState>.Default.Equals(_currentStateType, stateType);
    }


    public IState GetState(TState stateType)
    {
        return states.TryGetValue(stateType, out var state) ? state : null;
    }

    public void Clear()
    {
        _currentState?.OnExit();
        states.Clear();
        _currentState = null;
    }

}

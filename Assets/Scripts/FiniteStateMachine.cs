using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine
{
    public class State
    {
        public string name;

        public System.Action onFrame; //This will run in all frames in which the state is active
        public System.Action onEnter;
        public System.Action onExit;

        public override string ToString()
        {
            return name;
        }

    }

    Dictionary<string, State> states = new Dictionary<string, State>();

    public State CurrentState { get; private set; }

    public State initialState;

    //creates new state, subscribes it, returns it
    public State CreateState(string name)
    {
        var state = new State();
        state.name = name;
        if (states.Count == 0)
        {
            initialState = state;
        }
        states[name] = state;
        return state;
    }

    public void Update()
    {
        // no states => log error
        if (states.Count == 0)
        {
            Debug.Log("State Machine with no states!");
        }
        // if no current state, transition to initial state
        if (CurrentState == null)
        {
            TransitionTo(initialState);
        }

        // onFrame? Run it: skip it
        if (CurrentState.onFrame != null)
        {
            CurrentState.onFrame();
        }
    }

    public void TransitionTo(State newState)
    {
        // newState null => log error and return
        if (newState == null)
        {
            Debug.Log("NewState is Null!");
            return;
        }

        // if currentState != null and has onExit, run it; else skip it
        if (CurrentState != null && CurrentState.onExit != null)
        {
            CurrentState.onExit();
        }

        // [Log Transition from currentState to newSTate]


        CurrentState = newState;

        // if currentState  has onEnter, run it; else skip it
        if (CurrentState.onEnter != null)
        {
            CurrentState.onEnter();
        }
    }

    public void TransitionTo(string stateName)
    {
        // if states has no stateName key; LogError and return
        var newState = states[stateName];
        TransitionTo(newState);

    }
}


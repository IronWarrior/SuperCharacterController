// With a little help from UnityGems

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SuperStateMachine : MonoBehaviour {

    protected float timeEnteredState;

    public class State
    {
        public Action DoSuperUpdate = DoNothing;
        public Action enterState = DoNothing;
        public Action exitState = DoNothing;

        public Enum currentState;
    }

    public State state = new State();

    public Enum currentState
    {
        get
        {
            return state.currentState;
        }
        set
        {
            if (state.currentState == value)
                return;

            ChangingState();
            state.currentState = value;
            ConfigureCurrentState();
        }
    }

    [HideInInspector]
    public Enum lastState;

    void ChangingState()
    {
        lastState = state.currentState;
        timeEnteredState = Time.time;
    }

    void ConfigureCurrentState()
    {
        if (state.exitState != null)
        {
            state.exitState();
        }

        //Now we need to configure all of the methods
        state.DoSuperUpdate = ConfigureDelegate<Action>("SuperUpdate", DoNothing);
        state.enterState = ConfigureDelegate<Action>("EnterState", DoNothing);
        state.exitState = ConfigureDelegate<Action>("ExitState", DoNothing);

        if (state.enterState != null)
        {
            state.enterState();
        }
    }

    Dictionary<Enum, Dictionary<string, Delegate>> _cache = new Dictionary<Enum, Dictionary<string, Delegate>>();

    T ConfigureDelegate<T>(string methodRoot, T Default) where T : class
    {

        Dictionary<string, Delegate> lookup;
        if (!_cache.TryGetValue(state.currentState, out lookup))
        {
            _cache[state.currentState] = lookup = new Dictionary<string, Delegate>();
        }
        Delegate returnValue;
        if (!lookup.TryGetValue(methodRoot, out returnValue))
        {
            var mtd = GetType().GetMethod(state.currentState.ToString() + "_" + methodRoot, System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod);

            if (mtd != null)
            {
                returnValue = Delegate.CreateDelegate(typeof(T), this, mtd);
            }
            else
            {
                returnValue = Default as Delegate;
            }
            lookup[methodRoot] = returnValue;
        }
        return returnValue as T;

    }

    void SuperUpdate()
    {
        EarlyGlobalSuperUpdate();

        state.DoSuperUpdate();

        LateGlobalSuperUpdate();
    }

    protected virtual void EarlyGlobalSuperUpdate() { }

    protected virtual void LateGlobalSuperUpdate() { }

    static void DoNothing() { }
}

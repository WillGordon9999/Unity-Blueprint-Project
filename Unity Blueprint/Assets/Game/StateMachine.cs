using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State<T>
{
    public abstract void EnterState(T owner);
    public abstract void UpdateState(T owner);
    public abstract void ExitState(T owner);
    public abstract void OnCollisionEnter(T owner, Collision collision);
    public abstract void OnCollisionStay(T owner, Collision collision);
    public abstract void OnCollisionExit(T owner, Collision collision);
    
    public abstract void OnTriggerEnter(T owner, Collider collider);
    public abstract void OnTriggerStay(T owner, Collider collider);
    public abstract void OnTriggerExit(T owner, Collider collider);
}

public class StateMachine<T>
{
    T owner;
    Dictionary<System.Type, State<T>> states;
    State<T> currentState;

    public StateMachine(T theOwner)
    {
        owner = theOwner;
        states = new Dictionary<System.Type, State<T>>();
        currentState = null;
    }

    public void Update()
    {
        if (currentState != null)
            currentState.UpdateState(owner);
    }

    public void OnCollisionEnter(T owner, Collision collision)
    {
        if (currentState != null)
            currentState.OnCollisionEnter(owner, collision);
    }

    public void OnCollisionStay(T owner, Collision collision)
    {
        if (currentState != null)
            currentState.OnCollisionStay(owner, collision);
    }
    public void OnCollisionExit(T owner, Collision collision)
    {
        if (currentState != null)
            currentState.OnCollisionExit(owner, collision);
    }

    public void OnTriggerEnter(T owner, Collider collider)
    {
        if (currentState != null)
            currentState.OnTriggerEnter(owner, collider);
    }

    public void OnTriggerStay(T owner, Collider collider)
    {
        if (currentState != null)
            currentState.OnTriggerStay(owner, collider);
    }

    public void OnTriggerExit(T owner, Collider collider)
    {
        if (currentState != null)
            currentState.OnTriggerExit(owner, collider);
    }

    public bool CheckState<T1>() where T1 : State<T>
    {
        return currentState.GetType() == typeof(T1);
    }

    public void ChangeState<T1>() where T1 : State<T>, new()
    {
        if (currentState != null)
        {
            currentState.ExitState(owner);
        }

        State<T> state;

        if (states.TryGetValue(typeof(T1), out state))
        {
            currentState = state;
            currentState.EnterState(owner);
        }

        else
        {
            currentState = new T1();
            states[typeof(T1)] = currentState;
            currentState.EnterState(owner);
        }
    }

}

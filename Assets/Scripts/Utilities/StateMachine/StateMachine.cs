using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State state;

    /// <summary>
    /// Call it to change state
    /// </summary>
    public void SetState(State stateToSet)
    {
        State previousState = state;

        //set new one
        state = stateToSet;

        //exit from previous
        if (previousState != null)
            previousState.Exit();

        //enter in new one
        if (state != null)
        {
            state.AwakeState(this);
            state.Enter();
        }
    }

    protected virtual void Update()
    {
        if(state != null)
            state.Update();
    }
        
    protected virtual void FixedUpdate()
    {
        if(state != null)
            state.FixedUpdate();
    }
}

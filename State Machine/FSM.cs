using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Istate {
    void OnEnter(BlackBoard blackBoard);
    void OnExit();
    void OnFixedUpdate();
    void OnUpdate();

}

public abstract class  BlackBoard
{
    
}

public class FSM {
    private IDictionary<PlayerState, Istate> _states;
    private BlackBoard blackBoard;
    public Istate currentState;

    public FSM(BlackBoard blackBoard) {
        this.blackBoard = blackBoard;
        _states = new Dictionary<PlayerState, Istate>();
    }

    public void AddState(PlayerState stateType,Istate state) {
        if (_states.ContainsKey(stateType)) {
            return;
        }
        Debug.Log($"{stateType.ToString()} State Added");
        _states[stateType] = state;
    }

    public void SwitchState(PlayerState TargertState) {
        if(!_states.ContainsKey(TargertState))
            return;
        if(currentState != null)
            currentState.OnExit();
        currentState = _states[TargertState];
        currentState.OnEnter(blackBoard);
    }

    public void Update() {
        currentState.OnUpdate();
    }

    public void FixedUpdate() {
        currentState.OnFixedUpdate();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class PlayerBlackBoard : BlackBoard {

    public Rigidbody2D rb;
    public PlayerInput input;
    public PlayerStateCheck stateCheck;

    //方向与移动相关
    public int faceDirection = 1;
    public int moveDirectionX;
    public int moveDirectionY;
    public float speedX;
    public float speedY;
    public float jumpRiseMul = 1;

    //计时器相关
    //跳跃计时器
    public bool isJumping;
    public bool isSuperDashing;
    public bool isWallJumping;
    public float jumpTime = 0.25f;
    public float wallJumpTime = 0.12f;
    public float jumpTimer;
    //跳跃预输入计时器
    public bool isTryingJumping;
    public float tryJumpTime = 1 / 12f;
    public float tryJumpTimer;
    //头顶碰撞计时器
    public bool isHeadColliding;
    public float headCollideGraceTime = 0.05f;
    public float headCollideGraceTimer;
    //土狼时间计时器
    public bool isJumpGrace;
    public float jumpGraceTime = 1 / 12f;
    public float jumpGraceTimer;
    //冲刺预输入计时器
    public bool isTryingDashing;
    public float tryDashTime = 1 / 12f;
    public float tryDashTimer;
    //冲刺冷却计时器
    public bool isDashingCoolingDown;
    public float dashCoolDownTime = 0.2f;
    public float dashCoolDownTimer;
    //冲刺充能计时器
    public bool isDashRefilling;
    public float dashRefillTime = 0.1f;
    public float dashRefillTimer;

}
public enum PlayerState {
    Normal,
    Climb,
    Dash
}

public class PlayerControl : MonoBehaviour {
    public FSM fsm;
    public PlayerBlackBoard blackBoard;
    public Animator anim;
    private void Awake() {
        anim = GetComponent<Animator>();

        //初始化blackboard
        blackBoard = new PlayerBlackBoard {
            rb = GetComponent<Rigidbody2D>(),
            input = GetComponent<PlayerInput>(),
            stateCheck = GetComponent<PlayerStateCheck>()
        };

        //初始化fsm
        fsm = new FSM(blackBoard);
        fsm.AddState(PlayerState.Normal, new StNormal(fsm,blackBoard));
        fsm.AddState(PlayerState.Climb, new StClimb());
        fsm.AddState(PlayerState.Dash, new StDash(fsm, blackBoard));
        fsm.SwitchState(PlayerState.Normal);
    }

    private void Update() {
        UpdateMovementStates();
        fsm.Update();
        anim.SetBool("dashRefilled", blackBoard.stateCheck.isDashRefilled);
    }

    private void FixedUpdate() {
        fsm.FixedUpdate();
    }

    private void UpdateMovementStates() {
        blackBoard.faceDirection = blackBoard.input.inputDirX == 0 ? blackBoard.faceDirection : blackBoard.input.inputDirX;

        blackBoard.speedX = Mathf.Abs(blackBoard.rb.velocity.x);
        blackBoard.speedY = Mathf.Abs(blackBoard.rb.velocity.y);

        blackBoard.moveDirectionX = blackBoard.rb.velocity.x != 0 ? (int)Mathf.Sign(blackBoard.rb.velocity.x) : 0;
        blackBoard.moveDirectionY = blackBoard.rb.velocity.y != 0 ? (int)Mathf.Sign(blackBoard.rb.velocity.y) : 0;
    }
}

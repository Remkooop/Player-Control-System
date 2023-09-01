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

    //�������ƶ����
    public int faceDirection = 1;
    public int moveDirectionX;
    public int moveDirectionY;
    public float speedX;
    public float speedY;

    //��ʱ�����
    //��Ծ��ʱ��
    public bool isJumping;
    public bool isWallJumping;
    public float jumpTime = 0.25f;
    public float wallJumpTime = 0.12f;
    public float jumpTimer;
    //��ԾԤ�����ʱ��
    public bool isTryingJumping;
    public float tryJumpTime = 1 / 12f;
    public float tryJumpTimer;
    //ͷ����ײ��ʱ��
    public bool isHeadColliding;
    public float headCollideGraceTime = 0.05f;
    public float headCollideGraceTimer;
    //����ʱ���ʱ��
    public bool isJumpGrace;
    public float jumpGraceTime = 1 / 12f;
    public float jumpGraceTimer;

}
public enum PlayerState {
    Normal,
    Climb,
    Dash
}

public class PlayerControl : MonoBehaviour {
    public FSM fsm;
    public PlayerBlackBoard blackBoard;
    private void Awake() {
        //��ʼ��blackboard
        blackBoard = new PlayerBlackBoard {
            rb = GetComponent<Rigidbody2D>(),
            input = GetComponent<PlayerInput>(),
            stateCheck = GetComponent<PlayerStateCheck>()
        };

        //��ʼ��fsm
        fsm = new FSM(blackBoard);
        fsm.AddState(PlayerState.Normal, new StNormal());
        fsm.AddState(PlayerState.Climb, new StClimb());
        fsm.AddState(PlayerState.Dash, new StDash());
        fsm.SwitchState(PlayerState.Normal);
    }

    private void Update() {
        UpdateMovementStates();
        fsm.Update();
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

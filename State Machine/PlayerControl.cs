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
    public float jumpRiseMul = 1;

    //�������
    public float stamina;

    //��ʱ�����
    //��Ծ��ʱ��
    public bool isJumping;
    public bool isSuperDashing;
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
    //���Ԥ�����ʱ��
    public bool isTryingDashing;
    public float tryDashTime = 1 / 12f;
    public float tryDashTimer;
    //�����ȴ��ʱ��
    public bool isDashingCoolingDown;
    public float dashCoolDownTime = 0.2f;
    public float dashCoolDownTimer;
    //��̳��ܼ�ʱ��
    public bool isDashRefilling;
    public float dashRefillTime = 0.1f;
    public float dashRefillTimer;
    //ץ�Ǽ��ټ�ʱ��
    public bool isCornerBoostGrace;
    public float cornerBoostGraceTime = 0.2f;
    public float cornerBoostGraceTimer;
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

        //��ʼ��blackboard
        blackBoard = new PlayerBlackBoard {
            rb = GetComponent<Rigidbody2D>(),
            input = GetComponent<PlayerInput>(),
            stateCheck = GetComponent<PlayerStateCheck>(),
            stamina = 110
        };

        //��ʼ��fsm
        fsm = new FSM(blackBoard);
        fsm.AddState(PlayerState.Normal, new StNormal(fsm,blackBoard));
        fsm.AddState(PlayerState.Climb, new StClimb(fsm, blackBoard));
        fsm.AddState(PlayerState.Dash, new StDash(fsm, blackBoard));
        fsm.SwitchState(PlayerState.Normal);
    }

    private void Update() {
        UpdateMovementStates();
        fsm.Update();
        anim.SetBool("dashRefilled", blackBoard.stateCheck.isDashRefilled);
        anim.SetBool("isTired", blackBoard.stamina < 20);
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

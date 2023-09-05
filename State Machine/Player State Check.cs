using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCheck : MonoBehaviour {

    public PlayerBlackBoard blackBoard;
    public FSM fsm;

    //物理相关
    public bool isOnGround;
    public bool isFaceGround;
    public bool isBackGround;

    //操作相关
    public bool canJump;
    public bool isDucking;
    public bool isDashRefilled = true;
    public bool canWallBounce;

    [Header("检测参数")]
    public LayerMask groundCheckLayer;

    private void Awake() {
        blackBoard = GetComponent<PlayerControl>().blackBoard;
        fsm = GetComponent<PlayerControl>().fsm;
    }

    private void Update() {
        GroundCheck();
        DashRefill();
        HeadCollideCheck();
        sideCheck();
        JumpCheck();
        WallBounceCheck();
        DuckCheck();
    }

    private void WallBounceCheck() {
        if (!(fsm.currentState is StDash)) {
            //非冲刺则不行
            canWallBounce = false;
            return;
        }
        if (isFaceGround || isBackGround) {
            //已经贴墙则可以
            canWallBounce = true;
            return;
        }
        //蹭墙跳修正
        bool check1;
        bool check2;
        for(int i = 1; i <= 4; i++) {
            check1 = Physics2D.BoxCast(new Vector3(transform.position.x + blackBoard.faceDirection * (0.75f+i*0.1f), transform.position.y - 0.16f), new Vector3(0.1f, 2f), 0, new Vector3(blackBoard.faceDirection, 0), 0.1f, groundCheckLayer);
            check2 =Physics2D.BoxCast(new Vector3(transform.position.x - blackBoard.faceDirection * (0.75f + i * 0.1f), transform.position.y - 0.16f), new Vector3(0.1f, 2f), 0, new Vector3(-blackBoard.faceDirection, 0), 0.1f, groundCheckLayer);
            if (check1) {
                isFaceGround = true;
                canWallBounce = true;
                return;
            }
            if(check2) {
                isFaceGround = true;
                canWallBounce = true;
                return;
            }
        }

    }

    private void DuckCheck() {
        isDucking = blackBoard.input.inputDirY == -1;
    }

    private void JumpCheck() {
        //土狼时间
        if (isOnGround) {
            blackBoard.isJumpGrace = true;
            blackBoard.jumpGraceTimer = blackBoard.jumpGraceTime;
        } else {
            if (blackBoard.isJumpGrace) {
                //土狼时间内
                blackBoard.jumpGraceTimer -= Time.deltaTime;
                if (blackBoard.jumpGraceTimer <= 0) {
                    //土狼时间结束
                    blackBoard.isJumpGrace = false;
                    blackBoard.jumpGraceTimer = 0;
                }
            }
        }
        canJump = blackBoard.isJumpGrace || (isFaceGround || isBackGround);
    }

    private void GroundCheck() {
        bool wasOnGround = isOnGround;
        isOnGround = Physics2D.BoxCast(transform.position + new Vector3(0, -1.25f), new Vector3(1.25f, 0.05f), 0, new Vector2(0, -1), 0.05f, groundCheckLayer);

        //正常状态撞地
        if (fsm.currentState is StNormal && !wasOnGround && isOnGround) {
            blackBoard.speedX *= 1.2f;
            blackBoard.rb.velocity = new Vector2(blackBoard.speedX*blackBoard.moveDirectionX,blackBoard.speedY*blackBoard.moveDirectionY);
        }

        if(isOnGround&&fsm.currentState is StDash) {
            //冲刺状态且在地面上
            //如果可以修正则不进入碰撞状态
            if (GroundCollideYCorrect()) {
                isOnGround = false;
            }
        }

        if (isOnGround) blackBoard.stamina = 110;
    }

    private void sideCheck() {
        isFaceGround = Physics2D.BoxCast(new Vector3(transform.position.x + blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), new Vector3(0.1f, 2f), 0, new Vector3(blackBoard.faceDirection, 0), 0.1f, groundCheckLayer);
        isBackGround = Physics2D.BoxCast(new Vector3(transform.position.x - blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), new Vector3(0.1f, 2f), 0, new Vector3(-blackBoard.faceDirection, 0), 0.1f, groundCheckLayer);
    
        if(isFaceGround&&fsm.currentState is StDash) {
            //冲刺状态且面前撞墙
            //如果可以修正则不进入碰撞状态
            if(GroundCollideXCorrect()) {
                isFaceGround=false;
            }
        }
    }


    private bool HeadCollideCheck() {
        bool isCollide = Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer);
        if (isCollide && blackBoard.speedY > 0 && blackBoard.moveDirectionY == 1 && !blackBoard.isHeadColliding) {
            //头顶有东西且竖直方向向上运动且先前不在碰撞状态
            //如果可以修正则不进入碰撞状态
            //如果是冲刺状态则修正距离为5
            if (!HeadCollideCorrect(fsm.currentState is StDash? 5:4)) {
                blackBoard.isHeadColliding = true;
                blackBoard.headCollideGraceTimer = blackBoard.headCollideGraceTime;
            }
        }
        return isCollide;
    }


    private void OnDrawGizmosSelected() {
        //地面检测
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, -1.25f), 0.05f);

        //头顶检测
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.8585f), 0.05f);

        //侧身检测
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), 0.05f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), 0.05f);

    }

    private bool GroundCollideXCorrect() {
        if (!(fsm.currentState is StDash)) return false;
        //水平或斜下冲刺时，如果碰到墙壁会进行水平修正
        var state = fsm.currentState as StDash;

        //若水平方向不移动
        if (state.dashDir.x == 0) return false;
        for(int i = 1; i <= 4; i++) {
            if(!Physics2D.BoxCast(new Vector3(transform.position.x + blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f + i*0.1f), new Vector3(0.1f, 2f), 0, new Vector3(blackBoard.faceDirection, 0), 0.1f, groundCheckLayer)) {
                transform.position = new Vector3(transform.position.x, transform.position.y + i * 0.1f);
                return true;
            }
        }
        return false;
    }


    private bool GroundCollideYCorrect() {
        if (!(fsm.currentState is StDash)) return false;
        //向下或斜下冲刺时，如果碰到地面会进行垂直修正
        //不会修正到与冲刺运动方向水平相反的墙角
        bool flag1 = false;
        bool flag2 = false;
        var state = fsm.currentState as StDash;

        //若冲刺竖直方向不向下
        if (state.dashDir.y != -1) return false;

        if (state.dashDir.x == 0) {
            //冲刺竖直
            flag1 = true;
            flag2 = true;
        } else {
            flag1 = state.dashDir.x > 0;
            flag2 = state.dashDir.x < 0;
        }

        if (flag1) {
            //向右修正
            for (int i = 1; i <= 4; i++) {
                if (!Physics2D.BoxCast(transform.position + new Vector3(0, -1.25f) + new Vector3(i*0.1f,0), new Vector3(1.25f, 0.05f), 0, new Vector2(0, -1), 0.05f, groundCheckLayer)) {
                    transform.position = transform.position + new Vector3(i * 0.1f, 0);
                    return true;
                }
            }
        }
        if (flag2) {
            //向左修正
            for(int i = 1;i<= 4; i++) {
                if (!Physics2D.BoxCast(transform.position + new Vector3(0, -1.25f) + new Vector3(-i * 0.1f, 0), new Vector3(1.25f, 0.05f), 0, new Vector2(0, -1), 0.05f, groundCheckLayer)) {
                    transform.position = transform.position + new Vector3(-i * 0.1f, 0);
                    return true;
                }
            }
        }
        return false;
    }

    public bool HeadCollideCorrect(int distance) {
        //竖直碰撞头顶修正,如果可以修正则返回true
        //优先在面朝方向进行修正
        for (int i = 1; i <= distance; i++) {
            if (!Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f) + new Vector3(i * 0.1f * blackBoard.faceDirection, 0), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer)) {
                transform.position = transform.position + new Vector3(i * 0.1f * blackBoard.faceDirection, 0);
                return true;
            }
        }

        //再进行背面修正
        for (int i = 1; i <= distance; i++) {
            if (!Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f) + new Vector3(i * 0.1f * -blackBoard.faceDirection, 0), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer)) {
                transform.position = transform.position + new Vector3(i * 0.1f * -blackBoard.faceDirection, 0);
                return true;
            }
        }
        return false;
    }

    private void DashRefill() {
        //冲刺已经充能
        if (isDashRefilled) return;

        //若冲刺充能正在冷却
        if (blackBoard.isDashRefilling) {
            blackBoard.dashRefillTimer -= Time.deltaTime;
            if (blackBoard.dashRefillTimer <= 0) {
                blackBoard.isDashRefilling = false;
                blackBoard.dashRefillTimer = 0;
            }
            return;
        }

        //冲刺可以充能
        if(isOnGround) isDashRefilled = true;
    }
}

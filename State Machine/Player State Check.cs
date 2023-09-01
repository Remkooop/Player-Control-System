using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCheck : MonoBehaviour
{

    public PlayerBlackBoard blackBoard;

    //物理相关
    public bool isOnGround;
    public bool isFaceGround;
    public bool isBackGround;

    //操作相关
    public bool canJump;

    [Header("检测参数")]
    public LayerMask groundCheckLayer;

    private void Awake() {
        blackBoard = GetComponent<PlayerControl>().blackBoard;
    }

    private void Update() {
        isOnGround = GroundCheck();
        HeadCollideCheck();
        sideCheck();

        //土狼时间
        if(isOnGround) {
            blackBoard.isJumpGrace = true;
            blackBoard.jumpGraceTimer = blackBoard.jumpGraceTime;
        } else {
            if(blackBoard.isJumpGrace) {
                //土狼时间内
                blackBoard.jumpGraceTimer -= Time.deltaTime;
                if(blackBoard.jumpGraceTimer <= 0) {
                    //土狼时间结束
                    blackBoard.isJumpGrace = false;
                    blackBoard.jumpGraceTimer = 0;
                }
            }
        }
        canJump = blackBoard.isJumpGrace || (isFaceGround || isBackGround);
    }

    private bool GroundCheck() {
        return Physics2D.BoxCast(transform.position + new Vector3(0,-1.25f), new Vector3(1.25f,0.05f), 0, new Vector2(0, -1), 0.05f, groundCheckLayer);
    }

    private bool HeadCollideCheck() {
        bool isCollide =  Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer);
        if(isCollide&&blackBoard.speedY>0&&blackBoard.moveDirectionY == 1&&!blackBoard.isHeadColliding) {
            //头顶有东西且竖直方向向上运动且先前不在碰撞状态
            //如果可以修正则不进入碰撞状态
            if(!HeadCollideCorrect(4)) {
                blackBoard.isHeadColliding = true;
                blackBoard.headCollideGraceTimer = blackBoard.headCollideGraceTime;
            }
        }
        return isCollide;
    }

    private void sideCheck() {
        isFaceGround =  Physics2D.BoxCast(new Vector3(transform.position.x + blackBoard.faceDirection * 0.75f, transform.position.y -0.16f), new Vector3(0.1f, 2f), 0,new Vector3( blackBoard.faceDirection,0), 0.1f, groundCheckLayer);
        isBackGround = Physics2D.BoxCast(new Vector3(transform.position.x - blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), new Vector3(0.1f, 2f), 0, new Vector3(-blackBoard.faceDirection, 0), 0.1f, groundCheckLayer);
    }

    private void OnDrawGizmosSelected() {
        //地面检测
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, -1.25f), 0.05f);

        //头顶检测
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.8585f), 0.05f);

        //侧身检测
        Gizmos.DrawWireSphere(new Vector3( transform.position.x + blackBoard.faceDirection * 0.75f, transform.position.y -0.16f), 0.05f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), 0.05f);

    }

    //竖直碰撞头顶修正,如果可以修正则返回true
    bool HeadCollideCorrect(int distance) {
        //优先在面朝方向进行修正
        for(int i = 1;i<=distance;i++) {
            if(!Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f) + new Vector3(i*0.1f*blackBoard.faceDirection,0), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer)) {
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
}

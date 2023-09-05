using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCheck : MonoBehaviour {

    public PlayerBlackBoard blackBoard;
    public FSM fsm;

    //�������
    public bool isOnGround;
    public bool isFaceGround;
    public bool isBackGround;

    //�������
    public bool canJump;
    public bool isDucking;
    public bool isDashRefilled = true;
    public bool canWallBounce;

    [Header("������")]
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
            //�ǳ������
            canWallBounce = false;
            return;
        }
        if (isFaceGround || isBackGround) {
            //�Ѿ���ǽ�����
            canWallBounce = true;
            return;
        }
        //��ǽ������
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
        //����ʱ��
        if (isOnGround) {
            blackBoard.isJumpGrace = true;
            blackBoard.jumpGraceTimer = blackBoard.jumpGraceTime;
        } else {
            if (blackBoard.isJumpGrace) {
                //����ʱ����
                blackBoard.jumpGraceTimer -= Time.deltaTime;
                if (blackBoard.jumpGraceTimer <= 0) {
                    //����ʱ�����
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

        //����״̬ײ��
        if (fsm.currentState is StNormal && !wasOnGround && isOnGround) {
            blackBoard.speedX *= 1.2f;
            blackBoard.rb.velocity = new Vector2(blackBoard.speedX*blackBoard.moveDirectionX,blackBoard.speedY*blackBoard.moveDirectionY);
        }

        if(isOnGround&&fsm.currentState is StDash) {
            //���״̬���ڵ�����
            //������������򲻽�����ײ״̬
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
            //���״̬����ǰײǽ
            //������������򲻽�����ײ״̬
            if(GroundCollideXCorrect()) {
                isFaceGround=false;
            }
        }
    }


    private bool HeadCollideCheck() {
        bool isCollide = Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer);
        if (isCollide && blackBoard.speedY > 0 && blackBoard.moveDirectionY == 1 && !blackBoard.isHeadColliding) {
            //ͷ���ж�������ֱ���������˶�����ǰ������ײ״̬
            //������������򲻽�����ײ״̬
            //����ǳ��״̬����������Ϊ5
            if (!HeadCollideCorrect(fsm.currentState is StDash? 5:4)) {
                blackBoard.isHeadColliding = true;
                blackBoard.headCollideGraceTimer = blackBoard.headCollideGraceTime;
            }
        }
        return isCollide;
    }


    private void OnDrawGizmosSelected() {
        //������
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, -1.25f), 0.05f);

        //ͷ�����
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.8585f), 0.05f);

        //������
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), 0.05f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - blackBoard.faceDirection * 0.75f, transform.position.y - 0.16f), 0.05f);

    }

    private bool GroundCollideXCorrect() {
        if (!(fsm.currentState is StDash)) return false;
        //ˮƽ��б�³��ʱ���������ǽ�ڻ����ˮƽ����
        var state = fsm.currentState as StDash;

        //��ˮƽ�����ƶ�
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
        //���»�б�³��ʱ����������������д�ֱ����
        //���������������˶�����ˮƽ�෴��ǽ��
        bool flag1 = false;
        bool flag2 = false;
        var state = fsm.currentState as StDash;

        //�������ֱ��������
        if (state.dashDir.y != -1) return false;

        if (state.dashDir.x == 0) {
            //�����ֱ
            flag1 = true;
            flag2 = true;
        } else {
            flag1 = state.dashDir.x > 0;
            flag2 = state.dashDir.x < 0;
        }

        if (flag1) {
            //��������
            for (int i = 1; i <= 4; i++) {
                if (!Physics2D.BoxCast(transform.position + new Vector3(0, -1.25f) + new Vector3(i*0.1f,0), new Vector3(1.25f, 0.05f), 0, new Vector2(0, -1), 0.05f, groundCheckLayer)) {
                    transform.position = transform.position + new Vector3(i * 0.1f, 0);
                    return true;
                }
            }
        }
        if (flag2) {
            //��������
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
        //��ֱ��ײͷ������,������������򷵻�true
        //�������泯�����������
        for (int i = 1; i <= distance; i++) {
            if (!Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f) + new Vector3(i * 0.1f * blackBoard.faceDirection, 0), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer)) {
                transform.position = transform.position + new Vector3(i * 0.1f * blackBoard.faceDirection, 0);
                return true;
            }
        }

        //�ٽ��б�������
        for (int i = 1; i <= distance; i++) {
            if (!Physics2D.BoxCast(transform.position + new Vector3(0, 0.8585f) + new Vector3(i * 0.1f * -blackBoard.faceDirection, 0), new Vector3(1.5f, 0.05f), 0, new Vector2(0, 1), 0.05f, groundCheckLayer)) {
                transform.position = transform.position + new Vector3(i * 0.1f * -blackBoard.faceDirection, 0);
                return true;
            }
        }
        return false;
    }

    private void DashRefill() {
        //����Ѿ�����
        if (isDashRefilled) return;

        //����̳���������ȴ
        if (blackBoard.isDashRefilling) {
            blackBoard.dashRefillTimer -= Time.deltaTime;
            if (blackBoard.dashRefillTimer <= 0) {
                blackBoard.isDashRefilling = false;
                blackBoard.dashRefillTimer = 0;
            }
            return;
        }

        //��̿��Գ���
        if(isOnGround) isDashRefilled = true;
    }
}

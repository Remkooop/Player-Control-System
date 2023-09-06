using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StDash : Istate
{
    public Vector2 dashDir;
    public float pastSpeedX;
    public int pastMoveDirX;

    FSM fsm;
    PlayerBlackBoard blackBoard;

    public StDash(FSM fsm, PlayerBlackBoard blackboard)
    {
        this.fsm = fsm;
        this.blackBoard = blackboard;
    }

    public void OnEnter() {
        Debug.Log("Entered Dash State");
        pastSpeedX = blackBoard.speedX;
        pastMoveDirX = blackBoard.moveDirectionX;
        //�ٶ�����
        blackBoard.rb.velocity = Vector2.zero;
        //ȷ����̷���
        DetermineDashDir();
    }

    public void OnExit() {
        Debug.Log("Exited Dash State");
    }

    public void OnFixedUpdate() {
    }

    public void OnUpdate() {
        //�����ٶ�,����������ǰ�ϴ��ˮƽ�ٶ�
        blackBoard.rb.velocity = DetermineDashSpeed();
        DashTimerReduce();
        TryJump();
        TryClimb();
    }

    private Vector2 DetermineDashSpeed() {
        //��ԭ�˶��������̷���ˮƽ������ͬ��ԭˮƽ�ٶȸ�������ԭ��
        if(dashDir.y == 1||dashDir.y == -1) return new Vector2(0, 24f*dashDir.y);
        if(pastMoveDirX == 0) return  24f * dashDir;
        if (pastMoveDirX == 1 && dashDir.x > 0) return new Vector2(Mathf.Max(pastSpeedX, 24f*dashDir.x), 24f * dashDir.y);
        if (pastMoveDirX == -1 && dashDir.x < 0) return new Vector2(-Mathf.Max(pastSpeedX, 24f*-dashDir.x), 24f * dashDir.y);
        return 24f * dashDir;
    }

    private void DetermineDashDir() {
        if(blackBoard.input.inputDir == Vector2.zero) {
            //���뷽��Ϊ��
            dashDir = new Vector2(blackBoard.faceDirection, 0);
        } else {
            //���뷽��Ϊ��
            dashDir = blackBoard.input.inputDir;
        }
    }

    private void DashTimerReduce() {
        blackBoard.dashCoolDownTimer -= Time.deltaTime;
        if(blackBoard.dashCoolDownTimer <= 0) {
            //���ʱ�����
            blackBoard.isDashingCoolingDown = false;
            blackBoard.dashCoolDownTimer = 0;
            //�����ٶ�
            float newSpeedY, newSpeedX;

            if(dashDir.y == 1||dashDir.y == -1) {
                //��ֱ��������
                newSpeedY = blackBoard.speedY * blackBoard.moveDirectionY;
                newSpeedX = 0;
            }else if(dashDir.y == 0 || dashDir.y > 0||blackBoard.stateCheck.isOnGround) {
                //ˮƽ���һ���б�ϻ��߳嵽������
                newSpeedY = dashDir.y == 0? 0: blackBoard.speedY;
                newSpeedX = 16f * (dashDir.x>0? 1:-1);
            } else {
                //б�±����ٶ�
                newSpeedY = -blackBoard.speedY;
                newSpeedX = blackBoard.speedX * (dashDir.x > 0 ? 1 : -1);
            }

            blackBoard.rb.velocity = new Vector2(newSpeedX, newSpeedY);

            fsm.SwitchState(PlayerState.Normal);
        }
    }

    private void TryJump() {
        //��ԾԤ����״̬����
        if (!blackBoard.isTryingJumping) return;

        //��ԾԤ�����ʱ������
        blackBoard.tryJumpTimer -= Time.deltaTime;

        //����ԾԤ�����ʱ��С��0
        if (blackBoard.tryJumpTimer <= 0) {
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
        }

        //��������Ծ
        if (!blackBoard.stateCheck.canJump) return;

        //���Ե�����Ծ
        if (blackBoard.isJumpGrace) {
            //����������Ծ
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
            blackBoard.isJumping = true;
            blackBoard.jumpTimer = blackBoard.jumpTime;
            //ȡ������ʱ��
            blackBoard.isJumpGrace = false;
            blackBoard.jumpGraceTimer = 0;

            if (!blackBoard.stateCheck.isDucking) {
                //Super
                blackBoard.speedX = 26f;
                blackBoard.moveDirectionX = blackBoard.input.inputDirX == 0 ? blackBoard.faceDirection : blackBoard.input.inputDirX;
                blackBoard.rb.velocity = new Vector2(26f* blackBoard.moveDirectionX, 10.5f);
            } else {
                //Hyper
                blackBoard.speedX = 32.5f;
                blackBoard.moveDirectionX = blackBoard.input.inputDirX == 0 ? blackBoard.faceDirection : blackBoard.input.inputDirX;
                blackBoard.rb.velocity = new Vector2(32.5f* blackBoard.moveDirectionX, 10.5f);
                blackBoard.jumpRiseMul = 0.5f;
            }
            blackBoard.isSuperDashing = true;
            fsm.SwitchState(PlayerState.Normal);
            return;
        } 
        if(blackBoard.stateCheck.canWallBounce) {
            //��ǽ��
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
            blackBoard.isJumping = true;
            blackBoard.jumpTimer = blackBoard.wallJumpTime;
            blackBoard.isWallJumping = true;

            blackBoard.speedX = 19f;
            if (blackBoard.stateCheck.isFaceGround) {
                //���ȴ��泯��������
                blackBoard.moveDirectionX = -blackBoard.faceDirection;
            } else {
                //�ӱ�������
                blackBoard.moveDirectionX = blackBoard.faceDirection;
            }

            blackBoard.rb.velocity = new Vector2(19f * blackBoard.moveDirectionX, 10.5f);

            fsm.SwitchState(PlayerState.Normal);
        }
    }

    private void TryClimb() {
        //û�а�סץ
        if (!blackBoard.input.isPressingGrab) return;

        //���泯ǽ
        if (!blackBoard.stateCheck.isFaceGround) return;

        //����С�ڵ���0
        if (blackBoard.stamina <= 20) return;

        //��������
        fsm.SwitchState(PlayerState.Climb);
    }
}

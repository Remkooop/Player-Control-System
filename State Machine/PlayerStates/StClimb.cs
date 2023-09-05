using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StClimb : Istate {
    //������س���
    private const float climbUpStaminaCost = 100 / 2.2f;
    private const float climbStillStaminaCost = 100 / 10f;
    private const float climbJumpStaminaCost = 110 / 4f;
    private const float climbUpSpeed = 4.5f;
    private const float climbDownSpeed = 8f;
    private const float climbAccel = 90f;

    private bool isCornerBoosted;

    //����ʱ�ٶ�
    private float EnterSpeed;

    FSM fsm;
    PlayerBlackBoard blackBoard;

    public StClimb(FSM fsm, PlayerBlackBoard blackboard) {
        this.fsm = fsm;
        this.blackBoard = blackboard;
    }

    public void OnEnter() {
        Debug.Log("Entered climb state");
        //��¼�����ٶ�
        EnterSpeed = blackBoard.speedX;
        //�����ٶ�
        blackBoard.rb.velocity = Vector2.zero;
        blackBoard.speedX = 0;
        blackBoard.speedY = 0;
        blackBoard.moveDirectionY = 0;
        //����һϵ�м�ʱ�����ж�
        blackBoard.isJumping = false;
        blackBoard.jumpTimer = 0;
        blackBoard.isSuperDashing = false;
        //����ץ�Ǽ���
        isCornerBoosted = false;
        blackBoard.isCornerBoostGrace = true;
        blackBoard.cornerBoostGraceTimer = blackBoard.cornerBoostGraceTime;
    }

    public void OnExit() {
        if (isCornerBoosted) {
            blackBoard.rb.velocity = new Vector2((EnterSpeed + 4f) * blackBoard.input.inputDirX == 0 ? 0 : blackBoard.input.inputDirX, 10.5f);
            blackBoard.speedX = EnterSpeed + 4f;
            blackBoard.speedY = 10.5f;
            blackBoard.moveDirectionX = blackBoard.input.inputDirX == 0 ? blackBoard.faceDirection : blackBoard.input.inputDirX;
        }
        Debug.Log("Exited climb state");

    }

    public void OnFixedUpdate() {
    }

    public void OnUpdate() {
        //ץ�Ǽ��ټ�ʱ������
        if(blackBoard.isCornerBoostGrace) {
            blackBoard.cornerBoostGraceTimer -= Time.deltaTime;
            if(blackBoard.cornerBoostGraceTimer <= 0) {
                blackBoard.cornerBoostGraceTimer = 0;
                blackBoard.isCornerBoostGrace = false;
            }
        }


        ClimbStateContinueCheck();
        TryDash();
        TryJump();
        Move();
    }

    private void ClimbStateContinueCheck() {
        if (!blackBoard.input.isPressingGrab) {
            //û�а�ץ
            fsm.SwitchState(PlayerState.Normal);
        }

        if (!blackBoard.stateCheck.isFaceGround) {
            //��������ǽ
            fsm.SwitchState(PlayerState.Normal);
        }

        //��������ʱ�����
        blackBoard.stamina -= Time.deltaTime * ((blackBoard.speedY > 0 && blackBoard.moveDirectionY > 0) ? climbUpStaminaCost : climbStillStaminaCost);
        if (blackBoard.stamina <= 0) {
            //��������
            fsm.SwitchState(PlayerState.Normal);
        }
    }

    private void Move() {
        blackBoard.rb.velocity = new Vector2(0f, CalcSpeedY() * blackBoard.moveDirectionY); ;
    }

    private float CalcSpeedY() {
        if (blackBoard.isJumping) {
            //������ץ��
            blackBoard.jumpTimer -= Time.deltaTime;
            if(blackBoard.jumpTimer <= 0) {
                blackBoard.jumpTimer = 0;
                blackBoard.isJumping = false;
                isCornerBoosted = false;
            }
            blackBoard.moveDirectionY = 1;
            return 10.5f;
        }

        if(blackBoard.input.inputDirY == 0) {
            //��ֱ����Ϊ��
            return Mathf.Max(0, blackBoard.speedX - climbAccel * Time.deltaTime);
        }else if (blackBoard.input.inputDirY > 0) {
            //��ֱ��������
            if(blackBoard.moveDirectionY >= 0) {
                //�����˶���ֹ
                blackBoard.moveDirectionY = 1;
                if (blackBoard.speedY > climbUpSpeed) {
                    //���ٶȴ�������ٶ�
                    return Mathf.Max(climbUpSpeed, blackBoard.speedY - climbAccel * Time.deltaTime);
                } else {
                    //�ٶ�С�ڵ�������ٶ�
                    return Mathf.Min(climbUpSpeed, blackBoard.speedY + climbAccel * Time.deltaTime);
                }
            } else {
                //�����˶�
                var tmp = blackBoard.speedY - climbAccel * Time.deltaTime;
                if (tmp <= 0) blackBoard.moveDirectionY = 1;
                return Mathf.Abs(Mathf.Min(climbUpSpeed, tmp));
            }
        } else {
            //��ֱ��������
            if (blackBoard.moveDirectionY <= 0) {
                //�����˶���ֹ
                blackBoard.moveDirectionY = -1;
                if (blackBoard.speedY > climbDownSpeed) {
                    //���ٶȴ�������ٶ�
                    return Mathf.Max(climbDownSpeed, blackBoard.speedY - climbAccel * Time.deltaTime);
                } else {
                    //�ٶ�С�ڵ�������ٶ�
                    return Mathf.Min(climbDownSpeed, blackBoard.speedY + climbAccel * Time.deltaTime);
                }
            } else {
                //�����˶�
                var tmp = blackBoard.speedY - climbAccel * Time.deltaTime;
                if (tmp < 0) blackBoard.moveDirectionY = -1;
                return Mathf.Abs(Mathf.Max(climbDownSpeed, tmp));
            }
        }
    }

    private void TryJump() {
        //������ԾԤ�����ʱ����
        if (!blackBoard.isTryingJumping) return;

        //��ԾԤ�����ʱ������
        blackBoard.tryJumpTimer -= Time.deltaTime;
        if(blackBoard.tryJumpTimer <=0) {
            blackBoard.tryJumpTimer = 0;
            blackBoard.isTryingJumping = false;
        }

        //ץ�Ǽ���
        isCornerBoosted = blackBoard.isCornerBoostGrace;
        blackBoard.isCornerBoostGrace = false;
        blackBoard.cornerBoostGraceTimer = 0;

        blackBoard.isTryingJumping = false;
        blackBoard.tryJumpTimer = 0;
        blackBoard.isJumping = true;
        blackBoard.jumpTimer = blackBoard.jumpTime;
        blackBoard.stamina -= climbJumpStaminaCost;

        //�����ٶ�
        blackBoard.rb.velocity = new Vector2(0, 10.5f);
    }

    private void TryDash() {
        //���Ԥ����ʱ�����
        if (!blackBoard.isTryingDashing) return;

        //����޳���
        if (!blackBoard.stateCheck.isDashRefilled) return;

        //���Ԥ�����ʱ������
        blackBoard.tryDashTimer -= Time.deltaTime;

        //�����ȴ
        if (blackBoard.isDashingCoolingDown) {
            blackBoard.dashCoolDownTimer -= Time.deltaTime;
            if (blackBoard.dashCoolDownTimer <= 0) {
                blackBoard.isDashingCoolingDown = false;
                blackBoard.dashRefillTimer = 0;
            }
        }

        //����ԾԤ�����ʱ��С����
        if (blackBoard.tryDashTimer <= 0) {
            blackBoard.isTryingDashing = false;
            blackBoard.tryDashTimer = 0;
        }

        //�����ڳ����ȴʱ��
        if (blackBoard.isDashingCoolingDown) return;

        //�������Գ��
        //�ǳ����ؼ�ʱ������
        blackBoard.isJumpGrace = false;
        blackBoard.jumpGraceTimer = 0;
        blackBoard.isJumping = false;
        blackBoard.jumpTimer = 0;
        blackBoard.isWallJumping = false;
        blackBoard.isTryingJumping = false;
        blackBoard.tryJumpTimer = 0;
        //ֹͣԤ����
        blackBoard.isTryingDashing = false;
        blackBoard.tryDashTimer = 0;
        //��̽�����ȴ
        blackBoard.isDashingCoolingDown = true;
        blackBoard.dashCoolDownTimer = blackBoard.dashCoolDownTime;
        //���ܽ�����ȴ
        blackBoard.stateCheck.isDashRefilled = false;
        blackBoard.isDashRefilling = true;
        blackBoard.dashRefillTimer = blackBoard.dashRefillTime;
        //�л�״̬
        fsm.SwitchState(PlayerState.Dash);
    }
}

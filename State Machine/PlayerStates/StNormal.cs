using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class StNormal : Istate {

    #region consts

    //����״̬�����ٶ����
    public const float maxRunSpeed = 9f;
    public const float runAccelerate = 100f;
    public const float runDecelerate = 40f;
    public const float airFriction = 65f;
    public const float airDecelerate = 26f;

    //����״̬�����ٶ����
    public const float gravityAccalerate = 90f;
    public const float normalMaxFallSpeed = 16;
    public const float fastFallMaxFallSpeed = 24;
    public const float jumpRaisingSpeed = 10.5f;

    #endregion

    private PlayerBlackBoard blackBoard;
    public void OnEnter(BlackBoard blackBoard) {
        Debug.Log("Entered Normal State");
        this.blackBoard = blackBoard as PlayerBlackBoard;
    }

    public void OnExit() {
        Debug.Log("Exited Normal State");
    }

    public void OnFixedUpdate() {
    }

    public void OnUpdate() {
        TryJump();
        blackBoard.rb.velocity = Move();
    }

    private Vector2 Move() {
        return new Vector2(CalcSpeedX() * (blackBoard.moveDirectionX == 0 ? blackBoard.input.inputDirX : blackBoard.moveDirectionX), CalcSpeedY() * blackBoard.moveDirectionY); ;
    }

    private float CalcSpeedX() {
        if(blackBoard.isWallJumping) {
            //����ǽ��ʱ����
            return blackBoard.speedX;
        }


        if(!blackBoard.stateCheck.isOnGround) {
            //���ڵ���ʱ
            if (blackBoard.input.inputDirX == 0) {
                //�����뷽��Ϊ��
                return Mathf.Max(0, blackBoard.speedX - airFriction * Time.deltaTime);
            } else if (blackBoard.input.inputDirX == blackBoard.moveDirectionX || blackBoard.moveDirectionX == 0) {
                //�����뷽�����ƶ�������ͬʱ
                if (blackBoard.speedX > maxRunSpeed) {
                    return Mathf.Max(maxRunSpeed, blackBoard.speedX - airDecelerate * Time.deltaTime);
                } else {
                    //�ٶ�С����������ٶ�
                    return Mathf.Min(maxRunSpeed, blackBoard.speedX + runAccelerate * Time.deltaTime);
                }
            } else {
                //�����뷽�����ƶ������෴
                var tmp = blackBoard.speedX - (runAccelerate + runAccelerate) * Time.deltaTime;
                if (tmp < 0) blackBoard.moveDirectionX = -blackBoard.moveDirectionX;
                return Mathf.Abs(Mathf.Max(-maxRunSpeed, tmp));
            }
        }
        
        
        //�ڵ�������ʱ
        if(blackBoard.input.inputDirX == 0) {
            //�����뷽��Ϊ��
            return Mathf.Max(0, blackBoard.speedX - runAccelerate * Time.deltaTime);
        }else if(blackBoard.input.inputDirX == blackBoard.moveDirectionX|| blackBoard.moveDirectionX == 0) {
            //�����뷽�����ƶ�������ͬʱ
            if (blackBoard.speedX > maxRunSpeed) {
                return Mathf.Max(maxRunSpeed,blackBoard.speedX- runDecelerate * Time.deltaTime);
            } else {
                //�ٶ�С����������ٶ�
                return Mathf.Min(maxRunSpeed, blackBoard.speedX + runAccelerate * Time.deltaTime);
            }
        } else {
            //�����뷽�����ƶ������෴
            var tmp = blackBoard.speedX - (runAccelerate + runAccelerate) * Time.deltaTime;
            if (tmp < 0) blackBoard.moveDirectionX = -blackBoard.moveDirectionX;
            return Mathf.Abs(Mathf.Max(-maxRunSpeed, tmp));
        }
        
    }

    private float CalcSpeedY() {
        float fallAccel = gravityAccalerate;

        //������Ծ��������
        if (blackBoard.input.isPressingJump && 0 < blackBoard.speedY && blackBoard.speedY <= 4) {
            fallAccel = 0.5f * gravityAccalerate;
        } else
            fallAccel = gravityAccalerate;

        //��Ծ��ͷ
        if (blackBoard.isHeadColliding) {
            blackBoard.headCollideGraceTimer -= Time.deltaTime;
            if (blackBoard.headCollideGraceTimer <= 0) {
                //��ײ�ٶȱ���ʱ�����
                blackBoard.jumpTimer = 0;
                blackBoard.isJumping = false;
                blackBoard.isWallJumping = false;
                blackBoard.headCollideGraceTimer = 0;
                blackBoard.isHeadColliding = false;
                blackBoard.moveDirectionY = -1;
                return 0;
            }
        }

        //��Ծ����ʱ��
        if (blackBoard.isJumping) {
            blackBoard.jumpTimer-=Time.deltaTime;
            blackBoard.moveDirectionY = 1;
            if (blackBoard.jumpTimer <= 0) {
                blackBoard.jumpTimer = 0;
                blackBoard.isJumping = false;
                blackBoard.isWallJumping = false;
            }
            return jumpRaisingSpeed;
        }

        //���
        if (blackBoard.stateCheck.isOnGround) {
            blackBoard.moveDirectionY = 0;
            return 0;
        }

        //����
        if (blackBoard.moveDirectionY == 1) {
            //��ͨ����
            float newSpeed = blackBoard.speedY - gravityAccalerate * Time.deltaTime;
            if (newSpeed < 0) {
                blackBoard.moveDirectionY = -1;
                return -newSpeed;
            } else {
                blackBoard.moveDirectionY = 1;
                return newSpeed;
            }
        } else {
            //��ǰ���½���ֹ
            blackBoard.moveDirectionY = -1;
            if(blackBoard.input.inputDirY == -1) {
                //�ٽ�
                return Mathf.Min(fastFallMaxFallSpeed, blackBoard.speedY + fallAccel * Time.deltaTime);
            } else {
                //��������
                if (blackBoard.speedY > normalMaxFallSpeed) {
                    return Mathf.Max(normalMaxFallSpeed, blackBoard.speedY - fallAccel * Time.deltaTime);
                } else {
                    return Mathf.Min(normalMaxFallSpeed, blackBoard.speedY + fallAccel * Time.deltaTime);
                }
            }
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
            //��Ծ����
            if (blackBoard.input.inputDirX != 0) {
                //���뷽��Ϊ��
                blackBoard.speedX += blackBoard.moveDirectionX == blackBoard.input.inputDirX ? 4 : -4;
            }
        } else {
            //��ǽ����������
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
            blackBoard.isJumping = true;
            blackBoard.jumpTimer = blackBoard.wallJumpTime;
            blackBoard.isWallJumping = true;

            blackBoard.speedX = blackBoard.input.inputDirX == 0 ? maxRunSpeed : maxRunSpeed + 4f;

            if(blackBoard.stateCheck.isFaceGround) {
                //���ȴ��泯��������
                blackBoard.moveDirectionX = -blackBoard.faceDirection;
            } else {
                //�ӱ�������
                blackBoard.moveDirectionX = blackBoard.faceDirection;
            }
        }

    }
}

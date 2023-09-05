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
        //速度重置
        blackBoard.rb.velocity = Vector2.zero;
        //确定冲刺方向
        DetermineDashDir();
    }

    public void OnExit() {
        Debug.Log("Exited Dash State");
    }

    public void OnFixedUpdate() {
    }

    public void OnUpdate() {
        //设置速度,保留进入冲刺前较大的水平速度
        blackBoard.rb.velocity = DetermineDashSpeed();
        DashTimerReduce();
        TryJump();
        TryClimb();
    }

    private Vector2 DetermineDashSpeed() {
        //若原运动方向与冲刺方向水平方向相同且原水平速度更大则保留原速
        if(dashDir.y == 1||dashDir.y == -1) return new Vector2(0, 24f*dashDir.y);
        if(pastMoveDirX == 0) return  24f * dashDir;
        if (pastMoveDirX == 1 && dashDir.x > 0) return new Vector2(Mathf.Max(pastSpeedX, 24f*dashDir.x), 24f * dashDir.y);
        if (pastMoveDirX == -1 && dashDir.x < 0) return new Vector2(-Mathf.Max(pastSpeedX, 24f*-dashDir.x), 24f * dashDir.y);
        return 24f * dashDir;
    }

    private void DetermineDashDir() {
        if(blackBoard.input.inputDir == Vector2.zero) {
            //输入方向为空
            dashDir = new Vector2(blackBoard.faceDirection, 0);
        } else {
            //输入方向不为空
            dashDir = blackBoard.input.inputDir;
        }
    }

    private void DashTimerReduce() {
        blackBoard.dashCoolDownTimer -= Time.deltaTime;
        if(blackBoard.dashCoolDownTimer <= 0) {
            //冲刺时间结束
            blackBoard.isDashingCoolingDown = false;
            blackBoard.dashCoolDownTimer = 0;
            //重置速度
            float newSpeedY, newSpeedX;

            if(dashDir.y == 1||dashDir.y == -1) {
                //竖直向上向下
                newSpeedY = blackBoard.speedY * blackBoard.moveDirectionY;
                newSpeedX = 0;
            }else if(dashDir.y == 0 || dashDir.y > 0||blackBoard.stateCheck.isOnGround) {
                //水平左右或者斜上或者冲到地面上
                newSpeedY = dashDir.y == 0? 0: blackBoard.speedY;
                newSpeedX = 16f * (dashDir.x>0? 1:-1);
            } else {
                //斜下保留速度
                newSpeedY = -blackBoard.speedY;
                newSpeedX = blackBoard.speedX * (dashDir.x > 0 ? 1 : -1);
            }

            blackBoard.rb.velocity = new Vector2(newSpeedX, newSpeedY);

            fsm.SwitchState(PlayerState.Normal);
        }
    }

    private void TryJump() {
        //跳跃预输入状态结束
        if (!blackBoard.isTryingJumping) return;

        //跳跃预输入计时器减少
        blackBoard.tryJumpTimer -= Time.deltaTime;

        //若跳跃预输入计时器小于0
        if (blackBoard.tryJumpTimer <= 0) {
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
        }

        //不可以跳跃
        if (!blackBoard.stateCheck.canJump) return;

        //可以地面跳跃
        if (blackBoard.isJumpGrace) {
            //正常可以跳跃
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
            blackBoard.isJumping = true;
            blackBoard.jumpTimer = blackBoard.jumpTime;
            //取消土狼时间
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
            //蹬墙跳
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
            blackBoard.isJumping = true;
            blackBoard.jumpTimer = blackBoard.wallJumpTime;
            blackBoard.isWallJumping = true;

            blackBoard.speedX = 19f;
            if (blackBoard.stateCheck.isFaceGround) {
                //优先从面朝方向跳开
                blackBoard.moveDirectionX = -blackBoard.faceDirection;
            } else {
                //从背面跳开
                blackBoard.moveDirectionX = blackBoard.faceDirection;
            }

            blackBoard.rb.velocity = new Vector2(19f * blackBoard.moveDirectionX, 10.5f);

            fsm.SwitchState(PlayerState.Normal);
        }
    }

    private void TryClimb() {
        //没有按住抓
        if (!blackBoard.input.isPressingGrab) return;

        //不面朝墙
        if (!blackBoard.stateCheck.isFaceGround) return;

        //体力小于等于0
        if (blackBoard.stamina <= 20) return;

        //可以攀爬
        fsm.SwitchState(PlayerState.Climb);
    }
}

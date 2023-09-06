using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StClimb : Istate {
    //攀爬相关常数
    private const float climbUpStaminaCost = 100 / 2.2f;
    private const float climbStillStaminaCost = 100 / 10f;
    private const float climbJumpStaminaCost = 110 / 4f;
    private const float climbUpSpeed = 4.5f;
    private const float climbDownSpeed = 8f;
    private const float climbAccel = 90f;

    private bool isCornerBoosted;

    //进入时速度
    private float EnterSpeed;

    FSM fsm;
    PlayerBlackBoard blackBoard;

    public StClimb(FSM fsm, PlayerBlackBoard blackboard) {
        this.fsm = fsm;
        this.blackBoard = blackboard;
    }

    public void OnEnter() {
        Debug.Log("Entered climb state");
        //记录进入速度
        EnterSpeed = blackBoard.speedX;
        //重置速度
        blackBoard.rb.velocity = Vector2.zero;
        blackBoard.speedX = 0;
        blackBoard.speedY = 0;
        blackBoard.moveDirectionY = 0;
        //重置一系列计时器及判定
        blackBoard.isJumping = false;
        blackBoard.jumpTimer = 0;
        blackBoard.isSuperDashing = false;
        //重置抓角加速
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
        //抓角加速计时器减少
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
            //没有按抓
            fsm.SwitchState(PlayerState.Normal);
        }

        if (!blackBoard.stateCheck.isFaceGround) {
            //不正面贴墙
            fsm.SwitchState(PlayerState.Normal);
        }

        //体力根据时间减少
        blackBoard.stamina -= Time.deltaTime * ((blackBoard.speedY > 0 && blackBoard.moveDirectionY > 0) ? climbUpStaminaCost : climbStillStaminaCost);
        if (blackBoard.stamina <= 0) {
            //体力用完
            fsm.SwitchState(PlayerState.Normal);
        }
    }

    private void Move() {
        blackBoard.rb.velocity = new Vector2(0f, CalcSpeedY() * blackBoard.moveDirectionY); ;
    }

    private float CalcSpeedY() {
        if (blackBoard.isJumping) {
            //若正在抓跳
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
            //竖直输入为空
            return Mathf.Max(0, blackBoard.speedX - climbAccel * Time.deltaTime);
        }else if (blackBoard.input.inputDirY > 0) {
            //竖直输入向上
            if(blackBoard.moveDirectionY >= 0) {
                //向上运动或静止
                blackBoard.moveDirectionY = 1;
                if (blackBoard.speedY > climbUpSpeed) {
                    //若速度大于最大速度
                    return Mathf.Max(climbUpSpeed, blackBoard.speedY - climbAccel * Time.deltaTime);
                } else {
                    //速度小于等于最大速度
                    return Mathf.Min(climbUpSpeed, blackBoard.speedY + climbAccel * Time.deltaTime);
                }
            } else {
                //向下运动
                var tmp = blackBoard.speedY - climbAccel * Time.deltaTime;
                if (tmp <= 0) blackBoard.moveDirectionY = 1;
                return Mathf.Abs(Mathf.Min(climbUpSpeed, tmp));
            }
        } else {
            //竖直输入向下
            if (blackBoard.moveDirectionY <= 0) {
                //向下运动或静止
                blackBoard.moveDirectionY = -1;
                if (blackBoard.speedY > climbDownSpeed) {
                    //若速度大于最大速度
                    return Mathf.Max(climbDownSpeed, blackBoard.speedY - climbAccel * Time.deltaTime);
                } else {
                    //速度小于等于最大速度
                    return Mathf.Min(climbDownSpeed, blackBoard.speedY + climbAccel * Time.deltaTime);
                }
            } else {
                //向上运动
                var tmp = blackBoard.speedY - climbAccel * Time.deltaTime;
                if (tmp < 0) blackBoard.moveDirectionY = -1;
                return Mathf.Abs(Mathf.Max(climbDownSpeed, tmp));
            }
        }
    }

    private void TryJump() {
        //不在跳跃预输入计时器内
        if (!blackBoard.isTryingJumping) return;

        //跳跃预输入计时器减少
        blackBoard.tryJumpTimer -= Time.deltaTime;
        if(blackBoard.tryJumpTimer <=0) {
            blackBoard.tryJumpTimer = 0;
            blackBoard.isTryingJumping = false;
        }

        //抓角加速
        isCornerBoosted = blackBoard.isCornerBoostGrace;
        blackBoard.isCornerBoostGrace = false;
        blackBoard.cornerBoostGraceTimer = 0;

        blackBoard.isTryingJumping = false;
        blackBoard.tryJumpTimer = 0;
        blackBoard.isJumping = true;
        blackBoard.jumpTimer = blackBoard.jumpTime;
        blackBoard.stamina -= climbJumpStaminaCost;

        //设置速度
        blackBoard.rb.velocity = new Vector2(0, 10.5f);
    }

    private void TryDash() {
        //冲刺预输入时间结束
        if (!blackBoard.isTryingDashing) return;

        //冲刺无充能
        if (!blackBoard.stateCheck.isDashRefilled) return;

        //冲刺预输入计时器减少
        blackBoard.tryDashTimer -= Time.deltaTime;

        //冲刺冷却
        if (blackBoard.isDashingCoolingDown) {
            blackBoard.dashCoolDownTimer -= Time.deltaTime;
            if (blackBoard.dashCoolDownTimer <= 0) {
                blackBoard.isDashingCoolingDown = false;
                blackBoard.dashRefillTimer = 0;
            }
        }

        //若跳跃预输入计时器小于零
        if (blackBoard.tryDashTimer <= 0) {
            blackBoard.isTryingDashing = false;
            blackBoard.tryDashTimer = 0;
        }

        //若仍在冲刺冷却时间
        if (blackBoard.isDashingCoolingDown) return;

        //正常可以冲刺
        //非冲刺相关计时器设置
        blackBoard.isJumpGrace = false;
        blackBoard.jumpGraceTimer = 0;
        blackBoard.isJumping = false;
        blackBoard.jumpTimer = 0;
        blackBoard.isWallJumping = false;
        blackBoard.isTryingJumping = false;
        blackBoard.tryJumpTimer = 0;
        //停止预输入
        blackBoard.isTryingDashing = false;
        blackBoard.tryDashTimer = 0;
        //冲刺进入冷却
        blackBoard.isDashingCoolingDown = true;
        blackBoard.dashCoolDownTimer = blackBoard.dashCoolDownTime;
        //充能进入冷却
        blackBoard.stateCheck.isDashRefilled = false;
        blackBoard.isDashRefilling = true;
        blackBoard.dashRefillTimer = blackBoard.dashRefillTime;
        //切换状态
        fsm.SwitchState(PlayerState.Dash);
    }
}

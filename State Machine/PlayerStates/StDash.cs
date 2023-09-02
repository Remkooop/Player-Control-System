using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StDash : Istate
{
    Vector2 dashDir;
    float pastSpeedX;
    int pastMoveDirX;

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
        Debug.Log(blackBoard.speedX);
        DashTimerReduce();
    }

    private Vector2 DetermineDashSpeed() {
        //若原运动方向与冲刺方向水平方向相同且原水平速度更大则保留原速
        if(dashDir.y == 1||dashDir.y == -1) return new Vector2(0, 24f*dashDir.y);
        if(pastMoveDirX == 0) return  24f * dashDir;
        if (pastMoveDirX == 1 && dashDir.x > 0) return new Vector2(Mathf.Max(pastSpeedX, 24f) * dashDir.x, 24f * dashDir.y);
        if (pastMoveDirX == -1 && dashDir.x < 0) return new Vector2(Mathf.Max(pastSpeedX, 24f) * dashDir.x, 24f * dashDir.y);
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
            if (dashDir.y == 1 || dashDir.y == -1) blackBoard.rb.velocity = new Vector2(0, blackBoard.speedY);
            else blackBoard.rb.velocity = new Vector2(16f*blackBoard.moveDirectionX, blackBoard.speedY);
            fsm.SwitchState(PlayerState.Normal);
        }
    }
}

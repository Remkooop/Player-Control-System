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
        Debug.Log(blackBoard.speedX);
        DashTimerReduce();
    }

    private Vector2 DetermineDashSpeed() {
        //��ԭ�˶��������̷���ˮƽ������ͬ��ԭˮƽ�ٶȸ�������ԭ��
        if(dashDir.y == 1||dashDir.y == -1) return new Vector2(0, 24f*dashDir.y);
        if(pastMoveDirX == 0) return  24f * dashDir;
        if (pastMoveDirX == 1 && dashDir.x > 0) return new Vector2(Mathf.Max(pastSpeedX, 24f) * dashDir.x, 24f * dashDir.y);
        if (pastMoveDirX == -1 && dashDir.x < 0) return new Vector2(Mathf.Max(pastSpeedX, 24f) * dashDir.x, 24f * dashDir.y);
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
            if (dashDir.y == 1 || dashDir.y == -1) blackBoard.rb.velocity = new Vector2(0, blackBoard.speedY);
            else blackBoard.rb.velocity = new Vector2(16f*blackBoard.moveDirectionX, blackBoard.speedY);
            fsm.SwitchState(PlayerState.Normal);
        }
    }
}

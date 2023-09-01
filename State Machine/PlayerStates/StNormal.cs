using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class StNormal : Istate {

    #region consts

    //正常状态横向速度相关
    public const float maxRunSpeed = 9f;
    public const float runAccelerate = 100f;
    public const float runDecelerate = 40f;
    public const float airFriction = 65f;
    public const float airDecelerate = 26f;

    //正常状态纵向速度相关
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
            //在踢墙跳时间内
            return blackBoard.speedX;
        }


        if(!blackBoard.stateCheck.isOnGround) {
            //不在地面时
            if (blackBoard.input.inputDirX == 0) {
                //若键入方向为空
                return Mathf.Max(0, blackBoard.speedX - airFriction * Time.deltaTime);
            } else if (blackBoard.input.inputDirX == blackBoard.moveDirectionX || blackBoard.moveDirectionX == 0) {
                //若键入方向与移动方向相同时
                if (blackBoard.speedX > maxRunSpeed) {
                    return Mathf.Max(maxRunSpeed, blackBoard.speedX - airDecelerate * Time.deltaTime);
                } else {
                    //速度小于最大行走速度
                    return Mathf.Min(maxRunSpeed, blackBoard.speedX + runAccelerate * Time.deltaTime);
                }
            } else {
                //若键入方向与移动方向相反
                var tmp = blackBoard.speedX - (runAccelerate + runAccelerate) * Time.deltaTime;
                if (tmp < 0) blackBoard.moveDirectionX = -blackBoard.moveDirectionX;
                return Mathf.Abs(Mathf.Max(-maxRunSpeed, tmp));
            }
        }
        
        
        //在地面行走时
        if(blackBoard.input.inputDirX == 0) {
            //若键入方向为空
            return Mathf.Max(0, blackBoard.speedX - runAccelerate * Time.deltaTime);
        }else if(blackBoard.input.inputDirX == blackBoard.moveDirectionX|| blackBoard.moveDirectionX == 0) {
            //若键入方向与移动方向相同时
            if (blackBoard.speedX > maxRunSpeed) {
                return Mathf.Max(maxRunSpeed,blackBoard.speedX- runDecelerate * Time.deltaTime);
            } else {
                //速度小于最大行走速度
                return Mathf.Min(maxRunSpeed, blackBoard.speedX + runAccelerate * Time.deltaTime);
            }
        } else {
            //若键入方向与移动方向相反
            var tmp = blackBoard.speedX - (runAccelerate + runAccelerate) * Time.deltaTime;
            if (tmp < 0) blackBoard.moveDirectionX = -blackBoard.moveDirectionX;
            return Mathf.Abs(Mathf.Max(-maxRunSpeed, tmp));
        }
        
    }

    private float CalcSpeedY() {
        float fallAccel = gravityAccalerate;

        //悬浮跳跃降速修正
        if (blackBoard.input.isPressingJump && 0 < blackBoard.speedY && blackBoard.speedY <= 4) {
            fallAccel = 0.5f * gravityAccalerate;
        } else
            fallAccel = gravityAccalerate;

        //跳跃碰头
        if (blackBoard.isHeadColliding) {
            blackBoard.headCollideGraceTimer -= Time.deltaTime;
            if (blackBoard.headCollideGraceTimer <= 0) {
                //碰撞速度保留时间结束
                blackBoard.jumpTimer = 0;
                blackBoard.isJumping = false;
                blackBoard.isWallJumping = false;
                blackBoard.headCollideGraceTimer = 0;
                blackBoard.isHeadColliding = false;
                blackBoard.moveDirectionY = -1;
                return 0;
            }
        }

        //跳跃上升时间
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

        //落地
        if (blackBoard.stateCheck.isOnGround) {
            blackBoard.moveDirectionY = 0;
            return 0;
        }

        //下落
        if (blackBoard.moveDirectionY == 1) {
            //普通上升
            float newSpeed = blackBoard.speedY - gravityAccalerate * Time.deltaTime;
            if (newSpeed < 0) {
                blackBoard.moveDirectionY = -1;
                return -newSpeed;
            } else {
                blackBoard.moveDirectionY = 1;
                return newSpeed;
            }
        } else {
            //当前在下降或静止
            blackBoard.moveDirectionY = -1;
            if(blackBoard.input.inputDirY == -1) {
                //速降
                return Mathf.Min(fastFallMaxFallSpeed, blackBoard.speedY + fallAccel * Time.deltaTime);
            } else {
                //正常下落
                if (blackBoard.speedY > normalMaxFallSpeed) {
                    return Mathf.Max(normalMaxFallSpeed, blackBoard.speedY - fallAccel * Time.deltaTime);
                } else {
                    return Mathf.Min(normalMaxFallSpeed, blackBoard.speedY + fallAccel * Time.deltaTime);
                }
            }
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
            //跳跃加速
            if (blackBoard.input.inputDirX != 0) {
                //输入方向不为空
                blackBoard.speedX += blackBoard.moveDirectionX == blackBoard.input.inputDirX ? 4 : -4;
            }
        } else {
            //蹬墙跳或中性跳
            blackBoard.isTryingJumping = false;
            blackBoard.tryJumpTimer = 0;
            blackBoard.isJumping = true;
            blackBoard.jumpTimer = blackBoard.wallJumpTime;
            blackBoard.isWallJumping = true;

            blackBoard.speedX = blackBoard.input.inputDirX == 0 ? maxRunSpeed : maxRunSpeed + 4f;

            if(blackBoard.stateCheck.isFaceGround) {
                //优先从面朝方向跳开
                blackBoard.moveDirectionX = -blackBoard.faceDirection;
            } else {
                //从背面跳开
                blackBoard.moveDirectionX = blackBoard.faceDirection;
            }
        }

    }
}

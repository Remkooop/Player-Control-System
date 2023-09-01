using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour {
    #region consts
    //正常状态横向速度相关
    public const float maxRunSpeed = 9f;
    public const float runAccelerate = 100f;

    //正常状态纵向速度相关
    public const float gravityAccalerate = 90f;
    public const float normalMaxFallSpeed = 16;
    public const float fastFallMaxFallSpeed = 24;
    public const float jumpRaisingSpeed = 10.5f;
    #endregion

    #region vars
    private PlayerInputControl playerInputControl;
    private Rigidbody2D rb;
    private StateCheck stateCheck;

    private Vector2 inputDirection;
    public int inputDirectionX;
    public int inputDirectionY;
    public int moveDirectionX;
    public int moveDirectionY;
    public float speedX;
    public float speedY;
    #endregion

    private void Awake() {
        playerInputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();
        stateCheck = GetComponent<StateCheck>();
    }

    private void Update() {
        //获取输入方向、运动方向和速度分量
        GetDirection();
        //尝试跳跃
        TryJump();

        //正常移动
        Move();
    }

    private void OnEnable() {
        playerInputControl.Enable();
    }

    private void OnDisable() {
        playerInputControl.Disable();
    }



    //获取输入方向、移动方向和速度分量
    private void GetDirection() {
        inputDirection = playerInputControl.Gameplay.Move.ReadValue<Vector2>();
        speedX = Mathf.Abs(rb.velocity.x);
        speedY = Mathf.Abs(rb.velocity.y);

        moveDirectionX = rb.velocity.x != 0 ? (int)Mathf.Sign(rb.velocity.x) : 0;
        moveDirectionY = rb.velocity.y != 0 ? (int)Mathf.Sign(rb.velocity.y) : 0;

        inputDirectionX = inputDirection.x != 0 ? (int)Mathf.Sign(inputDirection.x) : 0;
        inputDirectionY = inputDirection.y != 0 ? (int)Mathf.Sign(inputDirection.y) : 0;
    }


    #region StNormal

    //计算新的速度
    private void Move() {
        rb.velocity = new Vector2(NormalCalcSpeedX() * (moveDirectionX == 0 ? inputDirectionX : moveDirectionX), NormalCalcSpeedY() * moveDirectionY);
    }

    //计算新的Y轴速度
    private float NormalCalcSpeedY() {

        float fallAccalerate = gravityAccalerate;

        //悬浮跳跃降速修正
        if (stateCheck.isPressingJump && 0 < speedY && speedY <= 4) {
            fallAccalerate *= 0.5f;
        }

        //碰撞到头顶
        if (stateCheck.isCollidingHead) {
            if (stateCheck.headCollideGraceTimer == 0)
                //重置碰撞期计时器
                stateCheck.headCollideGraceTimer = 0.05f;
            else stateCheck.headCollideGraceTimer = Mathf.Max(0, stateCheck.headCollideGraceTimer - Time.deltaTime);

            //在碰撞期内
            if (stateCheck.headCollideGraceTimer > 0) {
                stateCheck.jumpTimer = 0;
                return 0;
            }
        }

        //跳跃上升时间
        if (stateCheck.jumpTimer > 0) {
            moveDirectionY = 1;
            stateCheck.jumpTimer = Mathf.Max(stateCheck.jumpTimer - Time.deltaTime, 0);
            return jumpRaisingSpeed;
        }

        //到达地面
        if (stateCheck.isOnGround) {
            return 0;
        }

        //下落
        if (moveDirectionY == 1) {
            //普通上升
            float newSpeed = speedY - gravityAccalerate * Time.deltaTime;
            if (newSpeed < 0) {
                moveDirectionY = -1;
                return -newSpeed;
            } else {
                moveDirectionY = 1;
                return newSpeed;
            }
        } else {
            //当前在下降
            moveDirectionY = -1;
            if (inputDirectionY == -1) {
                //速降
                return Mathf.Min(fastFallMaxFallSpeed, speedY + gravityAccalerate * Time.deltaTime);
            } else {
                //正常下落
                if (speedY > normalMaxFallSpeed) {
                    return Mathf.Max(normalMaxFallSpeed, speedY - gravityAccalerate * Time.deltaTime);
                } else {
                    return Mathf.Min(normalMaxFallSpeed, speedY + gravityAccalerate * Time.deltaTime);
                }
            }
        }
    }

    //计算新的X轴速度
    private float NormalCalcSpeedX() {

        float frictionaccelerate = runAccelerate;
        float runAccel = runAccelerate;

        //正常走路时
        if (inputDirectionX == 0) {
            //若键入方向为空
            return Mathf.Max(0, speedX - frictionaccelerate * Time.deltaTime);
        } else if (inputDirectionX == moveDirectionX || moveDirectionX == 0) {
            //若键入方向与移动方向相同
            return Mathf.Min(maxRunSpeed, speedX + runAccel * Time.deltaTime);
        } else {
            //若键入方向与移动方向相反
            var tmp = speedX - (frictionaccelerate + runAccel) * Time.deltaTime;
            if (tmp < 0) moveDirectionX = -moveDirectionX;
            return Mathf.Abs(Mathf.Max(-maxRunSpeed, tmp));
        }
    }

    //尝试跳跃
    private void TryJump() {
        //跳跃预输入结束
        if (stateCheck.tryJumpTimer <= 0) return;

        //不可以跳跃
        if (!stateCheck.canJump) {
            stateCheck.tryJumpTimer = Mathf.Max(0, stateCheck.tryJumpTimer - Time.deltaTime); ;
            return;
        }

        //正常可以跳跃
        stateCheck.tryJumpTimer = 0;
        stateCheck.jumpGraceTimer = 0;
        stateCheck.canJump = false;
        stateCheck.jumpTimer = 0.25f;
    }

    #endregion


}

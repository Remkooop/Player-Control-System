using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateCheck : MonoBehaviour
{
    #region consts

    private const int StNormal = 1;
    private const int StClimb = 2;
    private const int StDash = 3;

    private const float tryJumpTime = 1 / 12f;
    private const float jumpTime = 0.25f;
    private const float jumpGraceTime = 1 / 12f;

    #endregion

    #region vars

    public PlayerInputControl playerInputControl;
    public Rigidbody2D rb;

    public int state;
    public LayerMask layerMask;

    [Header("状态")]
    public bool isOnGround;
    public bool isPressingJump;
    public bool isCollidingHead;
    public bool canJump;

    [Header("计时器")]
    public float jumpTimer;
    public float tryJumpTimer;
    public float headCollideGraceTimer;
    public float jumpGraceTimer;

    [Header("检测相关变量")]
    public float CheckRadius;
    public Vector2 groundCheckOffset;
    public Vector2 headCollideCheckOffset;

    #endregion

    private void Awake() {
        playerInputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();

        playerInputControl.Gameplay.Jump.started += StartPressJump;
        playerInputControl.Gameplay.Jump.canceled += StopPressJump;
    }

    private void OnEnable() {
        playerInputControl.Enable();
    }

    private void OnDisable() {
        playerInputControl.Enable();
    }

    private void Update() {
        GroundCheck();
        HeadColliedeCheck();
        CanJumpCheck();
    }

    #region JumpCheck
    private void StartPressJump(InputAction.CallbackContext context) {
        isPressingJump = true;
        //重置跳跃预输入计时器
        tryJumpTimer = tryJumpTime;
    }
    private void StopPressJump(InputAction.CallbackContext context) {
        isPressingJump =false;

        //将跳跃上升时间减为0
        jumpTimer = 0;
    }

    private void CanJumpCheck() {
        //在地面上
        if(isOnGround) {
            canJump = true;
            //重置土狼时间
            jumpGraceTimer = jumpGraceTime;
            return;
        }

        //离开地面
        if(jumpGraceTimer <= 0) {
            //土狼时间结束
            canJump = false;
            return;
        }

        //土狼时间内
        canJump = true;
        jumpGraceTimer = Math.Max(jumpGraceTimer-Time.deltaTime, 0);
    }

    #endregion

    #region CollideCheck
    public void GroundCheck() {
        isOnGround = Physics2D.OverlapCircle((Vector2)transform.position+groundCheckOffset, CheckRadius,layerMask);
    }

    public void HeadColliedeCheck() {
        isCollidingHead = (rb.velocity.y > 0 && Physics2D.OverlapCircle((Vector2)transform.position + headCollideCheckOffset, CheckRadius, layerMask));
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, CheckRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + headCollideCheckOffset, CheckRadius);
    }

    #endregion
}

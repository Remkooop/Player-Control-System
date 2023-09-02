using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour {
    public PlayerInputControl playerInputControl;
    public PlayerBlackBoard blackBoard;
    public Vector2 inputDir;
    public int inputDirX;
    public int inputDirY;

    public bool isPressingJump;
    public bool isPressingDash;
    public bool isPressingGrab;

    private void Awake() {
        playerInputControl = new PlayerInputControl();
        blackBoard = GetComponent<PlayerControl>().blackBoard;

        playerInputControl.Gameplay.Jump.started += StartPressingJump;
        playerInputControl.Gameplay.Jump.canceled += StopPressingJump;

        playerInputControl.Gameplay.Dash.started += StartPressingDash;
        playerInputControl.Gameplay.Dash.canceled += StopPressingDash;

        playerInputControl.Gameplay.Grab.started += StartPressingGrab;
        playerInputControl.Gameplay.Grab.canceled += StopPressingGrab;
    }


    private void OnEnable() {
        playerInputControl.Enable();
    }

    private void OnDisable() {
        playerInputControl.Disable();
    }

    private void Update() {
        GetInput();
    }

    private void GetInput() {
        //方向
        inputDir = playerInputControl.Gameplay.Move.ReadValue<Vector2>();
        inputDirX = inputDir.x != 0 ? (int)Mathf.Sign(inputDir.x) : 0;
        inputDirY = inputDir.y != 0 ? (int)Mathf.Sign(inputDir.y) : 0;
    }

    private void StartPressingJump(InputAction.CallbackContext context) {
        isPressingJump = true;
        //重置跳跃预输入计时器
        blackBoard.isTryingJumping = true;
        blackBoard.tryJumpTimer = blackBoard.tryJumpTime;
    }

    private void StopPressingJump(InputAction.CallbackContext context) {
        isPressingJump = false;
        //停止跳跃上升
        blackBoard.isJumping = false;
        blackBoard.isWallJumping = false;
        blackBoard.jumpTimer = 0;
    }

    private void StartPressingDash(InputAction.CallbackContext context) {
        isPressingDash = true;
        //重置冲刺预输入计时器
        blackBoard.isTryingDashing = true;
        blackBoard.tryDashTimer = blackBoard.tryDashTime; ;
    }

    private void StopPressingDash(InputAction.CallbackContext context) {
        isPressingDash = false;
    }

    private void StartPressingGrab(InputAction.CallbackContext context) {
        isPressingGrab = true;
    }

    private void StopPressingGrab(InputAction.CallbackContext context) {
        isPressingGrab = false;
    }
}

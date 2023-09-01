using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour {
    #region consts
    //����״̬�����ٶ����
    public const float maxRunSpeed = 9f;
    public const float runAccelerate = 100f;

    //����״̬�����ٶ����
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
        //��ȡ���뷽���˶�������ٶȷ���
        GetDirection();
        //������Ծ
        TryJump();

        //�����ƶ�
        Move();
    }

    private void OnEnable() {
        playerInputControl.Enable();
    }

    private void OnDisable() {
        playerInputControl.Disable();
    }



    //��ȡ���뷽���ƶ�������ٶȷ���
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

    //�����µ��ٶ�
    private void Move() {
        rb.velocity = new Vector2(NormalCalcSpeedX() * (moveDirectionX == 0 ? inputDirectionX : moveDirectionX), NormalCalcSpeedY() * moveDirectionY);
    }

    //�����µ�Y���ٶ�
    private float NormalCalcSpeedY() {

        float fallAccalerate = gravityAccalerate;

        //������Ծ��������
        if (stateCheck.isPressingJump && 0 < speedY && speedY <= 4) {
            fallAccalerate *= 0.5f;
        }

        //��ײ��ͷ��
        if (stateCheck.isCollidingHead) {
            if (stateCheck.headCollideGraceTimer == 0)
                //������ײ�ڼ�ʱ��
                stateCheck.headCollideGraceTimer = 0.05f;
            else stateCheck.headCollideGraceTimer = Mathf.Max(0, stateCheck.headCollideGraceTimer - Time.deltaTime);

            //����ײ����
            if (stateCheck.headCollideGraceTimer > 0) {
                stateCheck.jumpTimer = 0;
                return 0;
            }
        }

        //��Ծ����ʱ��
        if (stateCheck.jumpTimer > 0) {
            moveDirectionY = 1;
            stateCheck.jumpTimer = Mathf.Max(stateCheck.jumpTimer - Time.deltaTime, 0);
            return jumpRaisingSpeed;
        }

        //�������
        if (stateCheck.isOnGround) {
            return 0;
        }

        //����
        if (moveDirectionY == 1) {
            //��ͨ����
            float newSpeed = speedY - gravityAccalerate * Time.deltaTime;
            if (newSpeed < 0) {
                moveDirectionY = -1;
                return -newSpeed;
            } else {
                moveDirectionY = 1;
                return newSpeed;
            }
        } else {
            //��ǰ���½�
            moveDirectionY = -1;
            if (inputDirectionY == -1) {
                //�ٽ�
                return Mathf.Min(fastFallMaxFallSpeed, speedY + gravityAccalerate * Time.deltaTime);
            } else {
                //��������
                if (speedY > normalMaxFallSpeed) {
                    return Mathf.Max(normalMaxFallSpeed, speedY - gravityAccalerate * Time.deltaTime);
                } else {
                    return Mathf.Min(normalMaxFallSpeed, speedY + gravityAccalerate * Time.deltaTime);
                }
            }
        }
    }

    //�����µ�X���ٶ�
    private float NormalCalcSpeedX() {

        float frictionaccelerate = runAccelerate;
        float runAccel = runAccelerate;

        //������·ʱ
        if (inputDirectionX == 0) {
            //�����뷽��Ϊ��
            return Mathf.Max(0, speedX - frictionaccelerate * Time.deltaTime);
        } else if (inputDirectionX == moveDirectionX || moveDirectionX == 0) {
            //�����뷽�����ƶ�������ͬ
            return Mathf.Min(maxRunSpeed, speedX + runAccel * Time.deltaTime);
        } else {
            //�����뷽�����ƶ������෴
            var tmp = speedX - (frictionaccelerate + runAccel) * Time.deltaTime;
            if (tmp < 0) moveDirectionX = -moveDirectionX;
            return Mathf.Abs(Mathf.Max(-maxRunSpeed, tmp));
        }
    }

    //������Ծ
    private void TryJump() {
        //��ԾԤ�������
        if (stateCheck.tryJumpTimer <= 0) return;

        //��������Ծ
        if (!stateCheck.canJump) {
            stateCheck.tryJumpTimer = Mathf.Max(0, stateCheck.tryJumpTimer - Time.deltaTime); ;
            return;
        }

        //����������Ծ
        stateCheck.tryJumpTimer = 0;
        stateCheck.jumpGraceTimer = 0;
        stateCheck.canJump = false;
        stateCheck.jumpTimer = 0.25f;
    }

    #endregion


}

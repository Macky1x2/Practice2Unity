using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GroundChecker groundCheck;
    public RoofChecker roofCheck;
    public WallChecker wallCheck;

    public float maxHorizontalSpeed;
    public float horizontalAccel;
    public float horizontalStopAccel;

    public float gravityAccel;
    public float maxGravitySpeed;

    public float jumpVelocity;
    public float jumpUpTime;

    public float wallStaminaMax;
    public float wallClimbStaminaTimeMultiple;
    public float wallVerticalJumpStamina;
    public float wallClimbSpeed;
    public float wallVerticalJumpSpeed;
    public float wallReverseJumpSpeedX;
    public float wallReverseJumpSpeedY;

    public float wallSlideJumpSpeedX;
    public float wallSlideJumpSpeedY;
    public float wallSlideJumpedSpeedXRetentionTime;
    public float wallSlideMaxSpeed;
    public float wallSlideJumpUpTime;


    private bool onGround;
    private bool preOnGround;
    private Vector2 horizonMoveDirection;
    private float horizonSpeed;

    private bool onRoof;
    private bool onRoofAnd90;

    private float wallStaminaRemain;
    private bool onWall;
    private bool onWallAndLeft;
    private bool onWallJumped;
    private bool onWallVerticalJumped;

    private bool onWallSlideJumped;
    private float wallSlideJumpedSpeedXRetentionTimer;

    private bool jumped;
    private bool jumping;
    private float jumpTimeProgress;
    
    private Rigidbody2D rb;
    private Vector2 velocity;
    private PlayerIs playerState;


    // Start is called before the first frame update
    void Start()
    {
        rb = this.transform.GetComponent<Rigidbody2D>();
        velocity = rb.velocity;
        horizonSpeed = 0;
        jumped = false;
        jumpTimeProgress = 0;
        jumping = false;
        preOnGround = false;
        playerState = PlayerIs.NormalMove;
        onWallJumped = false;
        onWallSlideJumped = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetInformationFromChildren();
        VelocityUpdate();

        preOnGround = onGround;
    }

    private void VelocityUpdate()
    {
        //時間変数更新
        TimerUpdate();

        //プレイヤーの状態決定
        PlayerStateUpdate();

        //HorizonAndGravityUpdate(), JumpUpdate()の実行順で固定
        //左右移動と重力処理
        if (playerState != PlayerIs.onWall)
        {
            HorizonAndGravityUpdate();
        }
        else if(playerState == PlayerIs.onWall)
        {
            WallMoveUpdate();
        }

        //ジャンプ処理
        JumpUpdate();

        //この時点でvevlocityにはプレイヤーが取るべき速度が入っている
        rb.velocity = velocity;
    }

    private void JumpEnd()
    {
        jumping = false;
    }

    private void JumpStart()
    {
        bool tmpJumped = false;
        if (playerState == PlayerIs.NormalMove) tmpJumped = NormalJumpStart();
        else if (playerState == PlayerIs.onWall) tmpJumped = WallJumpStart();
        else if (playerState == PlayerIs.onWallSlide) tmpJumped = WallSlideJumpStart();

        if (tmpJumped)
        {
            horizonSpeed = velocity.x;
            jumped = true;
            jumping = true;
            jumpTimeProgress = 0;
        }
    }

    private bool NormalJumpStart() {
        JumpFlagsReset();
        velocity = new Vector2(velocity.x, jumpVelocity);
        return true;
    }
    private bool WallJumpStart()
    {
        bool ret = false;
        if (onWallAndLeft)
        {
            if (Input.GetAxis("Horizontal") == -1)
            {
                JumpFlagsReset();
                playerState = PlayerIs.NormalMove;
                onWallJumped = true;
                velocity = new Vector2(-wallReverseJumpSpeedX, wallReverseJumpSpeedY);
                horizonSpeed = -wallReverseJumpSpeedX;
                ret = true;
            }
            else
            {
                if (wallStaminaRemain >= wallVerticalJumpStamina)
                {
                    JumpFlagsReset();
                    playerState = PlayerIs.NormalMove;
                    onWallJumped = true;
                    onWallVerticalJumped = true;
                    velocity.y = wallVerticalJumpSpeed;
                    wallStaminaRemain -= wallVerticalJumpStamina;
                    ret = true;
                }
            }
        }
        else
        {
            if (Input.GetAxis("Horizontal") == 1)
            {
                JumpFlagsReset();
                playerState = PlayerIs.NormalMove;
                onWallJumped = true;
                velocity = new Vector2(wallReverseJumpSpeedX, wallReverseJumpSpeedY);
                horizonSpeed = wallReverseJumpSpeedX;
                ret = true;
            }
            else
            {
                if (wallStaminaRemain >= wallVerticalJumpStamina)
                {
                    JumpFlagsReset();
                    playerState = PlayerIs.NormalMove;
                    onWallJumped = true;
                    onWallVerticalJumped = true;
                    velocity.y = wallVerticalJumpSpeed;
                    wallStaminaRemain -= wallVerticalJumpStamina;
                    ret = true;
                }
            }
        }
        return ret;
    }

    private bool WallSlideJumpStart()
    {
        JumpFlagsReset();
        onWallSlideJumped = true;
        if (Input.GetAxis("Horizontal") == 1)
        {
            velocity = new Vector2(-wallSlideJumpSpeedX, wallSlideJumpSpeedY);
            horizonSpeed = -wallSlideJumpSpeedX;
            wallSlideJumpedSpeedXRetentionTimer = 0;
        }
        else if (Input.GetAxis("Horizontal") == -1)
        {
            velocity = new Vector2(wallSlideJumpSpeedX, wallSlideJumpSpeedY);
            horizonSpeed = wallSlideJumpSpeedX;
            wallSlideJumpedSpeedXRetentionTimer = 0;
        }
        return true;
    }

    private void JumpFlagsReset()
    {
        onWallJumped = false;
        onWallVerticalJumped = false;
        onWallSlideJumped = false;
    }

    private void HorizonAndGravityUpdate()
    {
        float verticalSpeed;
        if (velocity.y == 10) Debug.Log("test");
        if (rb.velocity.magnitude == 0) horizonSpeed = 0;
        //horizonSpeed = velocity.x;
        verticalSpeed = velocity.y;

        //←→速度について
        if(!(onWallSlideJumped) || wallSlideJumpedSpeedXRetentionTimer >= wallSlideJumpedSpeedXRetentionTime)         //壁ジャンプしてないか、してから基底時間経過しているという条件
        {
            if (-maxHorizontalSpeed < horizonSpeed && horizonSpeed < maxHorizontalSpeed && Input.GetAxis("Horizontal") != 0)
            {
                float deltaSpeed = Input.GetAxis("Horizontal") * horizontalAccel * Time.deltaTime;
                horizonSpeed += deltaSpeed;
                if (horizonSpeed < -maxHorizontalSpeed) horizonSpeed = -maxHorizontalSpeed;
                if (horizonSpeed > maxHorizontalSpeed) horizonSpeed = maxHorizontalSpeed;
            }
            else if (Input.GetAxis("Horizontal") == 0)
            {
                float deltaSpeed = horizontalStopAccel * Time.deltaTime;
                if (horizonSpeed >= -deltaSpeed && horizonSpeed <= deltaSpeed) horizonSpeed = 0;
                else horizonSpeed = horizonSpeed >= 0 ? horizonSpeed - deltaSpeed : horizonSpeed + deltaSpeed;
                if (onGround && !jumped) verticalSpeed = 0;
            }
        }

        //重力速度について
        if (!onGround)
        {
            float tmpMax;
            if (playerState == PlayerIs.onWallSlide) tmpMax = wallSlideMaxSpeed;
            else tmpMax = maxGravitySpeed;
            if (verticalSpeed > -tmpMax)
            {
                verticalSpeed -= gravityAccel * Time.deltaTime;
            }
            if (verticalSpeed < -tmpMax) verticalSpeed = -tmpMax;
        }

        if (onGround && !jumped)
        {
            velocity = horizonMoveDirection * horizonSpeed;
        }
        else
        {
            velocity = new Vector2(horizonSpeed, verticalSpeed);
        }
    }

    private void JumpUpdate()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (onGround)
            {
                JumpStart();
            }
            else if (playerState == PlayerIs.onWall)
            {
                JumpStart();
            }
            else if (playerState == PlayerIs.onWallSlide)
            {
                JumpStart();
            }
        }
        else if (Input.GetButton("Jump"))
        {
            if (!onGround && jumping)
            {
                if (onWallJumped)
                {
                    if(jumpTimeProgress <= jumpUpTime)
                    {
                        velocity.y = onWallVerticalJumped ? wallVerticalJumpSpeed : wallReverseJumpSpeedY;
                        jumpTimeProgress += Time.deltaTime;
                    }
                }
                else if (onWallSlideJumped)
                {
                    if(jumpTimeProgress <= wallSlideJumpUpTime)
                    {
                        velocity.y = wallSlideJumpSpeedY;
                        jumpTimeProgress += Time.deltaTime;
                    }
                }
                else
                {
                    if(jumpTimeProgress <= jumpUpTime)
                    {
                        velocity.y = jumpVelocity;
                        jumpTimeProgress += Time.deltaTime;
                    }
                }
            }
        }
        else if (Input.GetButtonUp("Jump"))
        {
            JumpEnd();
        }
        if (onRoofAnd90)
        {
            velocity = new Vector2(velocity.x, 0);
        }
    }

    private void PlayerStateUpdate()
    {
        //壁処理
        if (!WallUpdate())
        {
            playerState = PlayerIs.NormalMove;
        }
    }

    private bool WallUpdate()
    {
        bool ret = false;
        if (!onGround && onWall)
        {
            if (onWallAndLeft && horizonSpeed > 0 || !onWallAndLeft && horizonSpeed < 0)
            {
                horizonSpeed = 0;
            }

            if (wallStaminaRemain > 0)
            {
                if ((playerState == PlayerIs.NormalMove || playerState == PlayerIs.onWallSlide) && Input.GetButtonDown("Dash"))                    //PlayerIs.NormalMove -> PlayerIs.onWallとなるときに入るべき所
                {
                    wallStaminaRemain -= Time.deltaTime;
                    ret = true;
                    playerState = PlayerIs.onWall;
                    JumpValInit();
                    velocity = new Vector2(0, 0);
                }
                else if (playerState == PlayerIs.onWall && Input.GetButton("Dash"))                                                     //playerState == PlayerIs.onWallのときに入るべき所
                {
                    wallStaminaRemain -= Time.deltaTime;
                    ret = true;
                }
            }
            if (!ret && (onWallAndLeft && Input.GetAxis("Horizontal") == 1 || !onWallAndLeft && Input.GetAxis("Horizontal") == -1))
            {
                ret = true;
                playerState = PlayerIs.onWallSlide;
            }
            //if (onWallAndLeft)
            //{
            //    if (Input.GetAxis("Horizontal") == 1)
            //    {
            //        playerState = PlayerIs.onWall;
            //        JumpValInit();
            //    }
            //}
            //else
            //{
            //    if (Input.GetAxis("Horizontal") == -1)
            //    {
            //        playerState = PlayerIs.onWall;
            //        JumpValInit();
            //    }
            //}
        }
        return ret;
    }

    private void WallMoveUpdate()
    {
        velocity.y = wallClimbSpeed * Input.GetAxis("Vertical");
        if (Input.GetAxis("Vertical") == 1)
        {
            wallStaminaRemain -= Time.deltaTime * wallClimbStaminaTimeMultiple;
        }
    }

    private void TimerUpdate()
    {
        if (onWallSlideJumped)
        {
            wallSlideJumpedSpeedXRetentionTimer += Time.deltaTime;
        }
    }

    private void GetInformationFromChildren()
    {
        onGround = groundCheck.OnGroundCheck();
        if (!preOnGround && onGround)
        {
            JumpValInit();
            wallStaminaRemain = wallStaminaMax;
        }
        horizonMoveDirection = groundCheck.getMoveDirection();

        onRoof = roofCheck.GetOnRoofEnter();
        onRoofAnd90 = roofCheck.GetOnRoof90();

        onWall = wallCheck.OnWallCheck();
        onWallAndLeft = wallCheck.GetIsLeftIfOnWall();
    }

    private void JumpValInit()
    {
        jumped = false;
        jumping = false;
    }


    private enum PlayerIs
    {
        NormalMove,
        onWall,
        onWallSlide
    }
}

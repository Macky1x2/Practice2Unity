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

    public float wallClimbSpeed;
    public float wallVerticalJumpSpeed;
    public float wallReverseJumpSpeedX;
    public float wallReverseJumpSpeedY;
    public float wallJumpedSpeedXRetentionTime;


    private bool onGround;
    private bool preOnGround;
    private Vector2 horizonMoveDirection;
    private float horizonSpeed;

    private bool onRoof;
    private bool onRoofAnd90;

    private bool onWall;
    private bool onWallAndLeft;
    private bool onWallJumped;
    private bool onWallVerticalJumped;
    private float wallJumpedSpeedXRetentionTimer;

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
        onWallJumped = false;
        onWallVerticalJumped = false;
        if (playerState == PlayerIs.NormalMove)
        {
            velocity = new Vector2(velocity.x, jumpVelocity);
        }
        else if(playerState == PlayerIs.onWall)
        {
            playerState = PlayerIs.NormalMove;
            onWallJumped = true;
            if (onWallAndLeft)
            {
                if(Input.GetAxis("Horizontal") == -1)
                {
                    velocity = new Vector2(-wallReverseJumpSpeedX, wallReverseJumpSpeedY);
                    horizonSpeed = -wallReverseJumpSpeedX;
                }
                else
                {
                    onWallVerticalJumped = true;
                    velocity.y = wallVerticalJumpSpeed;
                }
            }
            else
            {
                if(Input.GetAxis("Horizontal") == 1)
                {

                    velocity = new Vector2(wallReverseJumpSpeedX, wallReverseJumpSpeedY);
                    horizonSpeed = wallReverseJumpSpeedX;
                }
                else
                {
                    onWallVerticalJumped = true;
                    velocity.y = wallVerticalJumpSpeed;
                }
            }
        }
        horizonSpeed = velocity.x;
        jumped = true;
        jumping = true;
        jumpTimeProgress = 0;
    }

    private void HorizonAndGravityUpdate()
    {
        float verticalSpeed;
        if (velocity.y == 10) Debug.Log("test");
        if (rb.velocity.magnitude == 0) horizonSpeed = 0;
        //horizonSpeed = velocity.x;
        verticalSpeed = velocity.y;

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

        if (!onGround)
        {
            if (verticalSpeed > -maxGravitySpeed)
            {
                verticalSpeed -= gravityAccel * Time.deltaTime;
                if (verticalSpeed < -maxGravitySpeed) verticalSpeed = -maxGravitySpeed;
            }
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
            if (onGround && !onRoof)
            {
                JumpStart();
            }
            else if (playerState == PlayerIs.onWall)
            {
                JumpStart();
            }
        }
        else if (Input.GetButton("Jump"))
        {
            if (!onGround && jumping && jumpTimeProgress <= jumpUpTime)
            {
                if (onWallJumped)
                {
                    velocity.y = onWallVerticalJumped ? wallVerticalJumpSpeed : wallReverseJumpSpeedY;
                    jumpTimeProgress += Time.deltaTime;
                }
                else
                {
                    velocity.y = jumpVelocity;
                    jumpTimeProgress += Time.deltaTime;
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
            if (onWallAndLeft && Input.GetAxis("Horizontal") == 1|| !onWallAndLeft && Input.GetAxis("Horizontal") == -1)
            {
                
            }
            if (onWallAndLeft && horizonSpeed > 0 || !onWallAndLeft && horizonSpeed < 0)
            {
                horizonSpeed = 0;
            }
            if (playerState == PlayerIs.NormalMove && Input.GetButtonDown("Dash"))                    //PlayerIs.NormalMove -> PlayerIs.onWallとなるときに入るべき所
            {
                ret = true;
                playerState = PlayerIs.onWall;
                JumpValInit();
                velocity = new Vector2(0, 0);
            }
            else if (playerState == PlayerIs.onWall && Input.GetButton("Dash"))                                                     //playerState == PlayerIs.onWallのときに入るべき所
            {
                ret = true;
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
    }

    private void GetInformationFromChildren()
    {
        onGround = groundCheck.OnGroundCheck();
        if (!preOnGround && onGround)
        {
            JumpValInit();
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
        onWall
    }
}

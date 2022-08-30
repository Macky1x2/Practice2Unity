using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public GroundChecker groundCheck;
    public RoofChecker roofCheck;
    public WallChecker wallCheck;
    public DarksideChecker darksideCheck;
    public Animator playerAnimator;     //playerStateについて　0:Idle 1:Run 2:Jump 3:JumptoFall 4:Fall 5:Edge-Grab 6:Edge-Idle 7:Wall-Slide 8:Dashing

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

    public float lightDashSpeed;
    public float lightDashTime;
    public float lightDashUpEndSpeedY;
    public float lightDashRLUpEndSpeedY;
    public float putSquareLightSpan;

    public Vector2[] animeOffsetXY;


    private bool onGround;
    private bool preOnGround;
    private Vector2 horizonMoveDirection;
    private float horizonSpeed;

    private bool onRoof;
    private bool onRoofAnd90;

    private float wallStaminaRemain;
    private bool onWall;
    private bool onWallAndLeft;
    private bool onWallAndRight;
    private bool onWallJumped;
    private bool onWallVerticalJumped;

    private bool onWallSlideJumped;
    private float wallSlideJumpedSpeedXRetentionTimer;

    private bool jumped;
    private bool jumping;
    private float jumpTimeProgress;

    private Vector2 lightDashDirection;
    private bool lightDashed;
    private bool lightDashing;
    private float lightDashTimer;
    private Light2D squareLightPrefabs;
    private Light2D squareLightSubPrefabs;
    private float putSquareLightTimer;

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
        squareLightPrefabs = Resources.Load<Light2D>("Prefabs/Player Light");
        squareLightSubPrefabs= Resources.Load<Light2D>("Prefabs/Player Light Sub");
    }

    // Update is called once per frame
    void Update()
    {
        GetInformationFromChildren();
        VelocityUpdate();
        AnimeOffsetUpdate();

        KeepPreFlags();
    }

    private void VelocityUpdate()
    {
        //時間変数更新
        TimerUpdate();

        //フラグ更新
        FlagsUpdate();

        //プレイヤーの状態決定
        PlayerStateUpdate();
        
        //HorizonAndGravityUpdate(), JumpUpdate()の実行順で固定
        //左右移動と重力処理
        if (playerState != PlayerIs.onWall && playerState !=PlayerIs.LightDashing)
        {
            HorizonAndGravityUpdate();
        }
        else if(playerState == PlayerIs.onWall)
        {
            WallMoveUpdate();
        }

        //ジャンプ処理
        JumpUpdate();

        //光ダッシュ
        LightDashUpdate();

        //この時点でvevlocityにはプレイヤーが取るべき速度が入っている
        rb.velocity = velocity;
    }

    private void JumpEnd()
    {
        jumping = false;
    }

    private void JumpStart()
    {
        playerAnimator.SetInteger("playerState", 2);
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
                WallJumpStartWhenJump();
                velocity = new Vector2(-wallReverseJumpSpeedX, wallReverseJumpSpeedY);
                horizonSpeed = -wallReverseJumpSpeedX;
                ret = true;
            }
            else
            {
                if (wallStaminaRemain >= wallVerticalJumpStamina)
                {
                    WallJumpStartWhenJump();
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
                WallJumpStartWhenJump();
                velocity = new Vector2(wallReverseJumpSpeedX, wallReverseJumpSpeedY);
                horizonSpeed = wallReverseJumpSpeedX;
                ret = true;
            }
            else
            {
                if (wallStaminaRemain >= wallVerticalJumpStamina)
                {
                    WallJumpStartWhenJump();
                    onWallVerticalJumped = true;
                    velocity.y = wallVerticalJumpSpeed;
                    wallStaminaRemain -= wallVerticalJumpStamina;
                    ret = true;
                }
            }
        }
        return ret;
    }

    private void WallJumpStartWhenJump()
    {
        JumpFlagsReset();
        playerState = PlayerIs.NormalMove;
        onWallJumped = true;
    }

    private bool WallSlideJumpStart()
    {
        JumpFlagsReset();
        onWallSlideJumped = true;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        playerAnimator.SetBool("isRight", !playerAnimator.GetBool("isRight"));
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
        if (rb.velocity.magnitude == 0) horizonSpeed = 0;
        verticalSpeed = velocity.y;

        //←→速度について
        if(!(onWallSlideJumped) || wallSlideJumpedSpeedXRetentionTimer >= wallSlideJumpedSpeedXRetentionTime)         //壁ジャンプしてないか、してから基底時間経過しているという条件
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                float deltaSpeed = Input.GetAxis("Horizontal") * horizontalAccel * Time.deltaTime;
                horizonSpeed += deltaSpeed;
                if (horizonSpeed < -maxHorizontalSpeed) horizonSpeed = -maxHorizontalSpeed;
                if (horizonSpeed > maxHorizontalSpeed) horizonSpeed = maxHorizontalSpeed;
                bool preAnimeIsRight = playerAnimator.GetBool("isRight");
                if (preAnimeIsRight)
                {
                    if (Input.GetAxis("Horizontal") < 0)
                    {
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                        playerAnimator.SetBool("isRight", false);
                    }
                }
                else
                {
                    if (Input.GetAxis("Horizontal") > 0)
                    {
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                        playerAnimator.SetBool("isRight", true);
                    }
                }
                if (playerAnimator.GetInteger("playerState") == 0)
                {
                    playerAnimator.SetInteger("playerState", 1);
                }
            }
            else if (Input.GetAxis("Horizontal") == 0)
            {
                float deltaSpeed = horizontalStopAccel * Time.deltaTime;
                if (horizonSpeed >= -deltaSpeed && horizonSpeed <= deltaSpeed) horizonSpeed = 0;
                else horizonSpeed = horizonSpeed >= 0 ? horizonSpeed - deltaSpeed : horizonSpeed + deltaSpeed;
                if (onGround && !jumped) verticalSpeed = 0;
                if (playerAnimator.GetInteger("playerState") == 1)
                {
                    playerAnimator.SetInteger("playerState", 0);
                }
            }
        }

        //重力速度について
        if (!onGround)
        {
            float tmpMax;
            if (playerState == PlayerIs.onWallSlide) tmpMax = wallSlideMaxSpeed;
            else tmpMax = maxGravitySpeed;

            if (playerAnimator.GetInteger("playerState") == 2)
            {
                if (verticalSpeed>0&& verticalSpeed- gravityAccel * Time.deltaTime <= 0)
                {
                    playerAnimator.SetInteger("playerState", 3);
                }
            }
            else if(playerAnimator.GetInteger("playerState") == 3)
            {
                if (verticalSpeed == -tmpMax)
                {
                    playerAnimator.SetInteger("playerState", 4);
                }
            }

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
        if (playerState != PlayerIs.LightDashing)
        {
            if (!WallUpdate())
            {
                playerState = PlayerIs.NormalMove;
                if (playerAnimator.GetInteger("playerState") == 7 || playerAnimator.GetInteger("playerState") == 6 || playerAnimator.GetInteger("playerState") == 5)
                {
                    if (onGround)
                    {
                        playerAnimator.SetInteger("playerState", 0);
                    }
                    else
                    {
                        AirAnimeSelect();
                    }
                }
            }
        }
    }

    private bool WallUpdate()
    {
        bool ret = false;
        if (!onGround && onWall)
        {
            if (onWallAndLeft && horizonSpeed > 0 || onWallAndRight && horizonSpeed < 0)
            {
                horizonSpeed = 0;
            }

            if (wallStaminaRemain > 0)
            {
                if ((playerState == PlayerIs.NormalMove || playerState == PlayerIs.onWallSlide) && Input.GetButtonDown("Dash"))         //PlayerIs.NormalMove -> PlayerIs.onWallとなるときに入るべき所
                {
                    //Debug.Log("test");
                    wallStaminaRemain -= Time.deltaTime;
                    ret = true;
                    playerState = PlayerIs.onWall;
                    JumpValInit();
                    velocity = new Vector2(0, 0);
                    playerAnimator.SetInteger("playerState", 5);
                }
                else if (playerState == PlayerIs.onWall && Input.GetButton("Dash"))                                                     //playerState == PlayerIs.onWallのときに入るべき所
                {
                    wallStaminaRemain -= Time.deltaTime;
                    ret = true;
                }
            }
            if (!ret && !onGround && (onWallAndLeft && Input.GetAxis("Horizontal") == 1 || onWallAndRight && Input.GetAxis("Horizontal") == -1))
            {
                ret = true;
                if(velocity.y < 0)
                {
                    playerAnimator.SetInteger("playerState", 7);
                }
                playerState = PlayerIs.onWallSlide;
            }
        }
        return ret;
    }

    private void WallMoveUpdate()
    {
        velocity.y = wallClimbSpeed * Input.GetAxis("Vertical");
        if (Input.GetAxis("Vertical") == 1)
        {
            wallStaminaRemain -= Time.deltaTime * wallClimbStaminaTimeMultiple;
            playerAnimator.SetInteger("playerState", 5);
        }
        else if(Input.GetAxis("Vertical") == -1 || Input.GetAxis("Vertical") == 0)
        {
            playerAnimator.SetInteger("playerState", 6);
        }
    }

    private void TimerUpdate()
    {
        if (onWallSlideJumped)
        {
            wallSlideJumpedSpeedXRetentionTimer += Time.deltaTime;
        }
    }

    private void LightDashUpdate()
    {
        if (!lightDashed && Input.GetButtonDown("LightDash") && !(Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)) 
        {
            playerState = PlayerIs.LightDashing;
            LightDashStart();
        }
        else if (lightDashing && Input.GetButton("LightDash") && lightDashTimer > 0)
        {
            LightDashing();
        }
        else if (lightDashing && (Input.GetButtonUp("LightDash") || lightDashTimer <= 0))
        {
            LightDashEnd();
        }
    }

    private void LightDashStart()
    {
        lightDashDirection = (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))).normalized;
        lightDashed = true;
        lightDashing = true;
        lightDashTimer = lightDashTime;
        playerAnimator.SetInteger("playerState", 8);
        Light2D squareLight;
        squareLight = Instantiate(squareLightPrefabs, this.transform.position, Quaternion.identity);
        ResizeChildTo111(squareLight);
        squareLight = Instantiate(squareLightSubPrefabs, this.transform.position, Quaternion.identity);
        ResizeChildTo111(squareLight);
        putSquareLightTimer = putSquareLightSpan;
    }

    private void LightDashing()
    {
        velocity = lightDashDirection * lightDashSpeed;
        lightDashTimer -= Time.deltaTime;
        if (putSquareLightTimer <= 0)
        {
            Light2D squareLight;
            squareLight = Instantiate(squareLightPrefabs, this.transform.position, Quaternion.identity);
            ResizeChildTo111(squareLight);
            squareLight = Instantiate(squareLightSubPrefabs, this.transform.position, Quaternion.identity);
            ResizeChildTo111(squareLight);
            putSquareLightTimer = putSquareLightSpan;
        }
        putSquareLightTimer -= Time.deltaTime;
    }

    private void LightDashEnd()
    {
        playerState = PlayerIs.NormalMove;
        lightDashing = false;
        if (lightDashDirection.x > 0) horizonSpeed = maxHorizontalSpeed;
        else if (lightDashDirection.x < 0) horizonSpeed = -maxHorizontalSpeed;
        else horizonSpeed = 0;
        if(lightDashDirection.y > 0)
        {
            if (lightDashDirection.x == 0) velocity.y = lightDashUpEndSpeedY;
            else velocity.y = lightDashRLUpEndSpeedY;
        }
        if (onGround)
        {
            if (horizonSpeed == 0) playerAnimator.SetInteger("playerState", 0);
            else playerAnimator.SetInteger("playerState", 1);
        }
        else
        {
            if (lightDashDirection.y > 0) playerAnimator.SetInteger("playerState", 2);
            else playerAnimator.SetInteger("playerState", 4);
        }
    }

    private void AnimeOffsetUpdate()
    {
        playerAnimator.transform.localPosition = new Vector3(animeOffsetXY[playerAnimator.GetInteger("playerState")].x, animeOffsetXY[playerAnimator.GetInteger("playerState")].y, 0);
    }

    private void GetInformationFromChildren()
    {
        onGround = groundCheck.IsTriggerCheck();
        if (!preOnGround && onGround)
        {
            JumpValInit();
            wallStaminaRemain = wallStaminaMax;
            if (playerState != PlayerIs.LightDashing)
            {
                playerAnimator.SetInteger("playerState", 0);
            } 
        }
        else if(preOnGround && !onGround)
        {
            AirAnimeSelect();
        }
        horizonMoveDirection = groundCheck.MoveDirection;

        onRoof = roofCheck.GetOnRoofEnter();
        onRoofAnd90 = roofCheck.GetOnRoof90();

        onWall = wallCheck.IsTriggerCheck();
        onWallAndLeft = wallCheck.IsLeftOnWall;
        onWallAndRight = wallCheck.IsRightOnWall;
    }

    private void FlagsUpdate()
    {
        if (onGround && lightDashed && !lightDashing)
        {
            LightDashValInit();
        }
    }

    private void ResizeChildTo111(MonoBehaviour go) 
    {
        go.transform.localScale = new Vector3(
                go.transform.localScale.x / go.transform.lossyScale.x,
                go.transform.localScale.y / go.transform.lossyScale.y,
                go.transform.localScale.z / go.transform.lossyScale.z
                );
    }

    private void JumpValInit()
    {
        jumped = false;
        jumping = false;
    }

    private void LightDashValInit()
    {
        lightDashed = false;
        lightDashing = false;
    }

    private void KeepPreFlags()
    {
        preOnGround = onGround;
    }
    private void AirAnimeSelect()
    {
        if (playerState != PlayerIs.LightDashing)
        {
            if (velocity.y > 0) playerAnimator.SetInteger("playerState", 2);
            else playerAnimator.SetInteger("playerState", 4);
        }
    }


    private enum PlayerIs
    {
        NormalMove,
        onWall,
        onWallSlide,
        LightDashing
    }
}

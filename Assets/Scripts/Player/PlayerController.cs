using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private ActionGameManager gameManager;
    [SerializeField] private GroundChecker groundCheck;
    [SerializeField] private BlockChecker leftRoofCheck;
    [SerializeField] private BlockChecker rightRoofCheck;
    [SerializeField] private BlockChecker upBlockCheckForLightDash;
    [SerializeField] private BlockChecker downBlockCheckForLightDash;
    [SerializeField] private BlockChecker leftBlockCheckForLightDash;
    [SerializeField] private BlockChecker rightBlockCheckForLightDash;
    [SerializeField] private WallChecker wallCheck;
    [SerializeField] private DarksideAndDeathChecker darksideCheck;
    [SerializeField] private DarksideAndDeathChecker fallDeathCheck;
    [SerializeField] private ParticleSystem blackFire;
    [SerializeField] private Animator playerAnimator;     //playerStateについて　0:Idle 1:Run 2:Jump 3:JumptoFall 4:Fall 5:Edge-Grab 6:Edge-Idle 7:Wall-Slide 8:Dashing

    [SerializeField] private float maxHorizontalSpeed;
    [SerializeField] private float maxHorizontalSpeedDarkside;
    [SerializeField] private float horizontalAccel;
    [SerializeField] private float horizontalAccelDarkside;
    [SerializeField] private float horizontalStopAccel;
    [SerializeField] private float horizontalStopAccelDarkside;
    
    [SerializeField] private float gravityAccel;
    [SerializeField] private float maxGravitySpeed;
    
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float jumpVelocityDarkside;
    [SerializeField] private float jumpUpTime;
    [SerializeField] private float jumpUpTimeDarkside;
    [SerializeField] private float jumpCoyoteTime;
    [SerializeField] private float jumpBufferTime;
    
    [SerializeField] private float airJumpVelocityDarkside;
    [SerializeField] private float airJumpUpTimeDarkside;
    
    [SerializeField] private float wallStaminaMax;
    [SerializeField] private float wallClimbStaminaTimeMultiple;
    [SerializeField] private float wallVerticalJumpStamina;
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private float wallClimbSpeedDarkside;
    [SerializeField] private float wallVerticalJumpSpeed;
    [SerializeField] private float wallVerticalJumpSpeedDarkside;
    [SerializeField] private float wallReverseJumpSpeedX;
    [SerializeField] private float wallReverseJumpSpeedXDarkside;
    [SerializeField] private float wallReverseJumpSpeedY;
    [SerializeField] private float wallReverseJumpSpeedYDarkside;
    [SerializeField] private float onWallBufferTime;
    [SerializeField] private float lfPushSpeedBySlide;
    [SerializeField] private float lfPushSpeedBySlideForLightDash;
    [SerializeField] private float udPushSpeedBySlideForLightDash;
    
    [SerializeField] private float wallSlideJumpSpeedX;
    [SerializeField] private float wallSlideJumpSpeedXDarkside;
    [SerializeField] private float wallSlideJumpSpeedY;
    [SerializeField] private float wallSlideJumpSpeedYDarkside;
    [SerializeField] private float wallSlideJumpedSpeedXRetentionTime;
    [SerializeField] private float wallSlideJumpedSpeedXRetentionTimeDarkside;
    [SerializeField] private float wallSlideMaxSpeed;
    [SerializeField] private float wallSlideJumpUpTime;
    [SerializeField] private float wallSlideJumpUpTimeDarkside;
    
    [SerializeField] private float lightDashSpeed;
    [SerializeField] private float lightDashTime;
    [SerializeField] private float lightDashUpEndSpeedY;
    [SerializeField] private float lightDashRLUpEndSpeedY;
    [SerializeField] private float putSquareLightSpan;
    
    [SerializeField] private float inDarksideReinforceTime;
    [SerializeField] private float inDarksideDeathTime;

    [SerializeField] private Vector2[] animeOffsetXY;

    //デバッグ
    [SerializeField] private Text debugText;

    public Vector2 spawnPoint { get; set; }
    public int hp { get; set; } = 0;

    
    private bool onGround;
    private bool preOnGround;
    private Vector2 horizonMoveDirection;
    private float horizonSpeed;

    private bool onRoofAnd90;
    private bool onLeftRoof90;
    private bool onRightRoof90;
    private bool onLeftRightRoofEnter;

    private bool onUpBlockForLightDash;
    private bool onDownBlockForLightDash;
    private bool onLeftBlockForLightDash;
    private bool onRightBlockForLightDash;

    private float wallStaminaRemain;
    private bool onWall;
    private bool onWallAndLeft;
    private bool onWallAndRight;
    private float onWallBufferTimer;

    private bool onWallSlideJumped;
    private float wallSlideJumpedSpeedXRetentionTimer;

    private bool jumped;
    private float jumpTimeProgress;
    private int airJumpCounter;
    private JumpIs nowJumpIs;
    private float jumpCoyoteTimer;
    private float jumpBufferTimer;

    private Vector2 lightDashDirection;
    private bool lightDashed;
    private bool lightDashing;
    private float lightDashTimer;
    private Light2D squareLightPrefabs;
    private Light2D squareLightSubPrefabs;
    private float putSquareLightTimer;

    private bool inDarkside;
    private float inDarksideTimer;

    private bool isDeadByFalling = false;       //死のフラグはRespawnPlayerInit()に含めない
    private bool isDeadNow = false;
    private bool isDeadAnimation = false;
    private bool isDeadFadeInNow = false;
    private ParticleSystem deadParticle;

    private Rigidbody2D rb;
    private Vector2 velocity;
    private PlayerIs playerState;
    private PlayerDarkIs playerDarkState;


    // Start is called before the first frame update
    void Start()
    {
        rb = this.transform.GetComponent<Rigidbody2D>();
        squareLightPrefabs = Resources.Load<Light2D>("Prefabs/Player Light");
        squareLightSubPrefabs= Resources.Load<Light2D>("Prefabs/Player Light Sub");
        deadParticle = Resources.Load<ParticleSystem>("Prefabs/Tris Spark");
        RespawnPlayerInit();
    }

    // Update is called once per frame
    void Update()
    {
        GetInformationFromChildren();       //子であるトリガーなどから情報を得てフィールドに代入(Velocityの計算の前にできる限りVelocityに関わる変数を決定しておく)
        if (!isDeadNow)
        {
            TimerUpdate();                      //時間変数更新(ただし、時間変数の変更はここ以外でも存在する)
            VelocityUpdate();                   //プレイヤーの速度計算およびrb.velocityへの代入  (ユーザーの操作(Input系)はこのメソッドでのみ存在する)
            AnimeOffsetUpdate();                //各アニメーションの位置を当たり判定にあわせる
            EffectsUpdate();                    //エフェクトの更新など(ただし、ここだけではない)
        }
        DieAndSpawnUpdate();                //プレイヤーが死ぬか確認　　外的要因による死では、hpが0以下

        KeepPreFlags();                     //次のUpdateのために必要な情報を保存
    }

    private void VelocityUpdate()
    {
        //入力バッファ更新
        InputBufferUpdate();

        //フラグ更新
        FlagsUpdate();

        //プレイヤーの状態決定(ただし、playerStateの変更はここ以外でも行われている)
        PlayerStateUpdate();
        
        //HorizonAndGravityUpdate(), JumpUpdate()の実行順は固定
        if (playerState != PlayerIs.onWall && playerState !=PlayerIs.LightDashing)
        {
            HorizonAndGravityUpdate();              //左右移動と重力処理
        }
        else if(playerState == PlayerIs.onWall)
        {
            WallMoveUpdate();                       //壁つかまり状態での上下移動
        }

        //ジャンプ処理
        JumpUpdate();

        //光ダッシュ
        LightDashUpdate();

        //衝突補正(天井にぶつかって停止やスライドなど)
        StopAndSlideUpdate();

        //この時点でvevlocityにはプレイヤーが取るべき速度が入っている
        rb.velocity = velocity;
    }

    private void StopAndSlideUpdate()
    {
        if (playerState != PlayerIs.LightDashing)
        {
            if (onLeftRoof90 && onRightRoof90)
            {
                velocity = new Vector2(velocity.x, Mathf.Min(velocity.y, 0.0f));
            }
            else if ((transform.localScale.x > 0 && onLeftRoof90 && !onRightRoof90 || transform.localScale.x < 0 && !onLeftRoof90 && onRightRoof90) && Input.GetAxis("Horizontal") >= 0 && velocity.y > 0)
            {
                velocity.x = Mathf.Max(velocity.x, lfPushSpeedBySlide);
                //horizonSpeed += 3;
                //if (horizonSpeed > maxHorizontalSpeed) horizonSpeed = maxHorizontalSpeed;
            }
            else if ((transform.localScale.x < 0 && onLeftRoof90 && !onRightRoof90 || transform.localScale.x > 0 && !onLeftRoof90 && onRightRoof90) && Input.GetAxis("Horizontal") <= 0 && velocity.y > 0)
            {
                velocity.x = Mathf.Min(velocity.x, -lfPushSpeedBySlide);
                //horizonSpeed -= 3;
                //if (horizonSpeed < -maxHorizontalSpeed) horizonSpeed = -maxHorizontalSpeed;
            }
        }
        else
        {
            if(lightDashDirection==new Vector2(0, 1))
            {
                if(transform.localScale.x > 0 && onLeftRoof90 && !onRightRoof90 || transform.localScale.x < 0 && !onLeftRoof90 && onRightRoof90)
                {
                    velocity = new Vector2(velocity.x + lfPushSpeedBySlideForLightDash, velocity.y);
                }
                else if(transform.localScale.x < 0 && onLeftRoof90 && !onRightRoof90 || transform.localScale.x > 0 && !onLeftRoof90 && onRightRoof90)
                {
                    velocity = new Vector2(velocity.x - lfPushSpeedBySlideForLightDash, velocity.y);
                }
            }
            else if(lightDashDirection == new Vector2(0, -1))
            {
                if (transform.localScale.x > 0 && onLeftBlockForLightDash && !onRightBlockForLightDash || transform.localScale.x < 0 && !onLeftBlockForLightDash && onRightBlockForLightDash)
                {
                    velocity = new Vector2(velocity.x + lfPushSpeedBySlideForLightDash, velocity.y);
                }
                else if (transform.localScale.x < 0 && onLeftBlockForLightDash && !onRightBlockForLightDash || transform.localScale.x > 0 && !onLeftBlockForLightDash && onRightBlockForLightDash)
                {
                    velocity = new Vector2(velocity.x - lfPushSpeedBySlideForLightDash, velocity.y);
                }
            }
            else if(lightDashDirection == new Vector2(1, 0) || lightDashDirection == new Vector2(-1, 0))
            {
                if(onUpBlockForLightDash && !onDownBlockForLightDash)
                {
                    velocity.y -= udPushSpeedBySlideForLightDash;
                }
                else if(!onUpBlockForLightDash && onDownBlockForLightDash)
                {
                    velocity.y += udPushSpeedBySlideForLightDash;
                }
            }
        }
    }

    private void InputBufferUpdate()
    {
        //壁つかまり入力バッファ
        if (Input.GetButtonDown("Dash"))
        {
            onWallBufferTimer = onWallBufferTime;
        }
        else if (Input.GetButton("Dash"))
        {
            onWallBufferTimer -= Time.deltaTime;
        }
        else if (Input.GetButtonUp("Dash"))
        {
            onWallBufferTimer = 0;
        }

        //ジャンプバッファ
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else if (Input.GetButton("Jump"))
        {
            jumpBufferTimer -= Time.deltaTime;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            jumpBufferTimer = 0;
        }
    }

    private void JumpEnd()
    {
        ResetNumForJumping();
    }

    private void NormalJumpStart() {
        velocity.y = GetStatusJumpVelocity();
    }

    private void AirJumpStart()
    {
        velocity.y = airJumpVelocityDarkside;       //闇状態でしか空中ジャンプはできないためGetStatusは不要
        airJumpCounter++;
    }


    private void WallJumpStartNonVertical()
    {
        if (onWallAndLeft)
        {
            WallJumpStartWhenJump();
            velocity = new Vector2(-GetStatusWallReverseJumpSpeedX(), GetStatusWallReverseJumpSpeedY());
            horizonSpeed = -GetStatusWallReverseJumpSpeedX();
        }
        else
        {
            WallJumpStartWhenJump();
            velocity = new Vector2(GetStatusWallReverseJumpSpeedX(), GetStatusWallReverseJumpSpeedY());
            horizonSpeed = GetStatusWallReverseJumpSpeedX();
        }
    }

    private void WallJumpStartVertical()
    {
        WallJumpStartWhenJump();
        velocity.y = GetStatusWallVerticalJumpSpeed();
        wallStaminaRemain -= wallVerticalJumpStamina;
    }

    private void WallJumpStartWhenJump()
    {
        playerState = PlayerIs.NormalMove;
    }

    private void WallSlideJumpStart()
    {
        WallSlideJumpValsReset();
        onWallSlideJumped = true;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        if (Input.GetAxis("Horizontal") > 0)
        {
            velocity = new Vector2(-GetStatusWallSlideJumpSpeedX(), GetStatusWallSlideJumpSpeedY());
            horizonSpeed = -GetStatusWallSlideJumpSpeedX();
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            velocity = new Vector2(GetStatusWallSlideJumpSpeedX(), GetStatusWallSlideJumpSpeedY());
            horizonSpeed = GetStatusWallSlideJumpSpeedX();
        }
    }

    private void WallSlideJumpValsReset()
    {
        onWallSlideJumped = false;
        wallSlideJumpedSpeedXRetentionTimer = 0;
    }

    private void ResetNumForJumping()
    {
        nowJumpIs = JumpIs.None;
    }

    private void HorizonAndGravityUpdate()
    {
        float verticalSpeed;
        if (rb.velocity.magnitude == 0) horizonSpeed = 0;
        verticalSpeed = velocity.y;

        //←→速度について
        if(!(onWallSlideJumped) || wallSlideJumpedSpeedXRetentionTimer >= GetStatusWallSlideJumpedSpeedXRetentionTime())         //壁ジャンプしてないか、してから基底時間経過しているという条件
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                float deltaSpeed = Input.GetAxis("Horizontal") * GetStatusHorizontalAccel() * Time.deltaTime;
                horizonSpeed += deltaSpeed;
                if (horizonSpeed < -GetStatusMaxHorizontalSpeed()) horizonSpeed = -GetStatusMaxHorizontalSpeed();
                if (horizonSpeed > GetStatusMaxHorizontalSpeed()) horizonSpeed = GetStatusMaxHorizontalSpeed();

                if (Input.GetAxis("Horizontal") < 0)
                {
                    transform.localScale = new Vector3(-0.8f, transform.localScale.y, transform.localScale.z);
                }
                else if (Input.GetAxis("Horizontal") > 0)
                {
                    transform.localScale = new Vector3(0.8f, transform.localScale.y, transform.localScale.z);
                }

                if (playerAnimator.GetInteger("playerState") == 0)
                {
                    playerAnimator.SetInteger("playerState", 1);
                }
            }
            else if (Input.GetAxis("Horizontal") == 0)
            {
                float deltaSpeed = GetStatusHorizontalStopAccel() * Time.deltaTime;
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

            ////デバッグ
            //if((verticalSpeed > 0 && verticalSpeed - gravityAccel * Time.deltaTime <= 0)){
            //    debugText.text = $"重力半分";
            //}
            //else
            //{
            //    debugText.text = $"重力通常";
            //}

            verticalSpeed -= ((verticalSpeed > 0 && verticalSpeed - gravityAccel * Time.deltaTime <= 0) ? gravityAccel / 2 : gravityAccel) * Time.deltaTime;
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
        //ジャンプバッファリングデモ
        //if (Input.GetButtonDown("Jump"))
        //{
        //    Instantiate(Resources.Load("Prefabs/DebugCharBoxImage"),this.transform.position,Quaternion.identity);
        //}

        if (Input.GetButtonDown("Jump") || IsJumpBuffer())
        {
            bool tmpJumped = false;
            if (playerState == PlayerIs.NormalMove && (onGround || jumpCoyoteTimer > 0 && !jumped))
            {
                tmpJumped = true;
                NormalJumpStart();
                nowJumpIs = JumpIs.NormalJump;
            }
            else if (playerState == PlayerIs.NormalMove && CanAirJump())
            {
                tmpJumped = true;
                AirJumpStart();
                nowJumpIs = JumpIs.AirJump;
            }
            else if (playerState == PlayerIs.onWall && ((onWallAndLeft && Input.GetAxis("Horizontal") < 0) || (!onWallAndLeft && Input.GetAxis("Horizontal") > 0)))
            {
                tmpJumped = true;
                WallJumpStartNonVertical();
                nowJumpIs = JumpIs.WallNonVerticalJump;
            }
            else if (playerState == PlayerIs.onWall && wallStaminaRemain >= wallVerticalJumpStamina)
            {
                tmpJumped = true;
                WallJumpStartVertical();
                nowJumpIs = JumpIs.WallVerticalJump;
            }
            else if (playerState == PlayerIs.onWallSlide)
            {
                tmpJumped = true;
                WallSlideJumpStart();
                nowJumpIs = JumpIs.WallSlideJump;
            }

            if (tmpJumped)
            {
                playerAnimator.SetInteger("playerState", 2);
                horizonSpeed = velocity.x;
                jumped = true;
                jumpTimeProgress = 0;
                jumpBufferTimer = 0;
            }
        }
        else if (Input.GetButton("Jump"))
        {
            if (!onGround && nowJumpIs != JumpIs.None)
            {
                float tmpJumpUpTime = 0;
                if(nowJumpIs== JumpIs.NormalJump)
                {
                    velocity.y = GetStatusJumpVelocity();
                    tmpJumpUpTime = GetStatusJumpUpTime();
                }
                else if (nowJumpIs == JumpIs.AirJump)
                {
                    velocity.y = airJumpVelocityDarkside;
                    tmpJumpUpTime = airJumpUpTimeDarkside;      //闇状態でしか空中ジャンプはできないためGetStatusは不要
                }
                else if (nowJumpIs == JumpIs.WallNonVerticalJump)
                {
                    velocity.y = GetStatusWallReverseJumpSpeedY();
                    tmpJumpUpTime = GetStatusJumpUpTime();
                }
                else if (nowJumpIs == JumpIs.WallVerticalJump)
                {
                    velocity.y = GetStatusWallVerticalJumpSpeed();
                    tmpJumpUpTime = GetStatusJumpUpTime();
                }
                else if (nowJumpIs == JumpIs.WallSlideJump)
                {
                    velocity.y = GetStatusWallSlideJumpSpeedY();
                    tmpJumpUpTime = GetStatusWallSlideJumpUpTime();
                }

                jumpTimeProgress += Time.deltaTime;
                if (jumpTimeProgress > tmpJumpUpTime)
                {
                    JumpEnd();
                }
            }
        }
        else if (Input.GetButtonUp("Jump"))
        {
            JumpEnd();
        }
    }

    private void PlayerStateUpdate()
    {
        //壁処理
        if (playerState != PlayerIs.LightDashing)
        {
            if (!WallUpdate())                  //壁つかまり状態にも、壁ずり状態にもならなかったなら入る
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

        //闇状態決定
        if (inDarksideTimer < inDarksideReinforceTime)
        {
            playerDarkState = PlayerDarkIs.NonDark;
        }
        else if(inDarksideTimer < inDarksideDeathTime)
        {
            playerDarkState = PlayerDarkIs.Dark1;
        }
        else
        {
            playerDarkState = PlayerDarkIs.Death;
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
                if ((playerState == PlayerIs.NormalMove || playerState == PlayerIs.onWallSlide) && (Input.GetButtonDown("Dash") || IsWallGrabBuffer()))         //PlayerIs.NormalMove -> PlayerIs.onWallとなるときに入るべき所
                {
                    wallStaminaRemain -= Time.deltaTime;
                    ret = true;
                    playerState = PlayerIs.onWall;
                    JumpValInit();
                    velocity = new Vector2(0, 0);
                    playerAnimator.SetInteger("playerState", 5);
                    onWallBufferTimer = 0;
                }
                else if (playerState == PlayerIs.onWall && Input.GetButton("Dash"))                                                     //playerState == PlayerIs.onWallのときに入るべき所
                {
                    wallStaminaRemain -= Time.deltaTime;
                    ret = true;
                }
            }
            if (!ret && !onGround && (onWallAndLeft && Input.GetAxis("Horizontal") > 0 || onWallAndRight && Input.GetAxis("Horizontal") < 0))
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
        velocity.y = GetStatusWallClimbSpeed() * Input.GetAxis("Vertical");
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
        //壁
        if (onWallSlideJumped)
        {
            wallSlideJumpedSpeedXRetentionTimer += Time.deltaTime;
        }

        //闇
        if (inDarkside)
        {
            inDarksideTimer += Time.deltaTime;
        }
        else
        {
            inDarksideTimer = Mathf.Max(0, inDarksideTimer - Time.deltaTime);
        }
        //Debug.Log(inDarksideTimer);

        if (!onGround)
        {
            jumpCoyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void LightDashUpdate()
    {
        if (!lightDashed && Input.GetButtonDown("LightDash") && playerDarkState == PlayerDarkIs.NonDark && !(Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)) 
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
        float directionX, directionY;
        if (Input.GetAxis("Horizontal") > 0) directionX = 1;
        else if (Input.GetAxis("Horizontal") < 0) directionX = -1;
        else directionX = 0;
        if (Input.GetAxis("Vertical") > 0) directionY = 1;
        else if (Input.GetAxis("Vertical") < 0) directionY = -1;
        else directionY = 0;
        lightDashDirection = (new Vector2(directionX, directionY)).normalized;     //directionX==0&&directionY==0とはならないことが保障されている(必要がある)
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
        if (lightDashDirection.x > 0) horizonSpeed = GetStatusMaxHorizontalSpeed();
        else if (lightDashDirection.x < 0) horizonSpeed = -GetStatusMaxHorizontalSpeed();
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
        //地面
        onGround = groundCheck.IsTriggerCheck();
        if (!preOnGround && onGround)
        {
            JumpValInit();                                      //ジャンプ回復(空中ジャンプは含まない)
            jumpCoyoteTimer = jumpCoyoteTime;
            airJumpCounter = 0;
            wallStaminaRemain = wallStaminaMax;                 //壁スタミナ回復
            if (playerState != PlayerIs.LightDashing)
            {
                playerAnimator.SetInteger("playerState", 0);
            } 
        }
        else if(preOnGround && !onGround)
        {
            AirAnimeSelect();                                   //空中用のアニメーションに移る
        }
        horizonMoveDirection = groundCheck.MoveDirection;

        //天井
        onLeftRoof90 = leftRoofCheck.IsTriggerCheck();
        onRightRoof90 = rightRoofCheck.IsTriggerCheck();
        if (onLeftRoof90 && onRightRoof90 && !onLeftRightRoofEnter)
        {
            onLeftRightRoofEnter = true;
        }
        else
        {
            onLeftRightRoofEnter = false;
        }

        //ブロック(光ダッシュ用)
        onUpBlockForLightDash = upBlockCheckForLightDash.IsTriggerCheck();
        onDownBlockForLightDash = downBlockCheckForLightDash.IsTriggerCheck();
        onLeftBlockForLightDash = leftBlockCheckForLightDash.IsTriggerCheck();
        onRightBlockForLightDash = rightBlockCheckForLightDash.IsTriggerCheck();

        //壁
        onWall = wallCheck.IsTriggerCheck();
        onWallAndLeft = wallCheck.IsLeftOnWall;
        onWallAndRight = wallCheck.IsRightOnWall;

        //闇
        inDarkside = darksideCheck.IsTriggerCheck();

        //落下死
        isDeadByFalling = fallDeathCheck.IsTriggerCheck();
    }

    private void FlagsUpdate()
    {
        if (onGround && lightDashed && !lightDashing)
        {
            LightDashValInit();
        }
    }

    private void EffectsUpdate()
    {
        if (inDarksideTimer >= inDarksideReinforceTime)
        {
            if (!blackFire.IsAlive())
            {
                blackFire.Play();
            }
        }
        else
        {
            if (blackFire.IsAlive())
            {
                blackFire.Stop();
            }
        }
    }

    private bool CanAirJump()
    {
        return playerDarkState == PlayerDarkIs.Dark1 && airJumpCounter == 0;
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
        ResetNumForJumping();
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

    private void DieStart()
    {
        isDeadNow = true;
        rb.velocity = Vector2.zero;
        playerAnimator.SetBool("isDead", true);
        isDeadAnimation = true;
    }

    private void DieAndSpawnUpdate()
    {
        if (playerDarkState == PlayerDarkIs.Death || isDeadByFalling || isDeadNow)
        {
            if (!isDeadFadeInNow)
            {
                if (!isDeadAnimation)
                {
                    DieStart();
                }
                else
                {
                    if (playerAnimator.gameObject.activeSelf)
                    {
                        float deathAnimationTime = playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                        if (deathAnimationTime < 1 && !isDeadByFalling)     //死亡時アニメーションの位置動かした方が良いかも？
                        {
                            //playerAnimator.transform.localPosition(0)
                        }
                        else
                        {
                            Instantiate(deadParticle, this.transform.position, Quaternion.identity);
                            playerAnimator.gameObject.SetActive(false);
                            gameManager.FadeStart();
                        }
                    }
                    else
                    {
                        gameManager.FadeUpdate();
                        if (gameManager.fadeBlackStart)
                        {
                            blackFire.gameObject.SetActive(false);
                            blackFire.gameObject.SetActive(true);
                            RespawnPlayerInit();
                            //rb.MovePosition(spawnPoint);
                            this.transform.position = spawnPoint;
                            isDeadFadeInNow = true;
                        }
                    }
                }
            }
            else
            {
                gameManager.FadeUpdate();
                if (gameManager.fadeEnd)
                {
                    isDeadFadeInNow = false;
                    isDeadAnimation = false;
                    isDeadByFalling = false;
                    isDeadNow = false;
                }
            }
        }
    }

    private bool IsJumpBuffer()
    {
        return jumpBufferTimer > 0;
    }

    private bool IsWallGrabBuffer()
    {
        return onWallBufferTimer > 0;
    }

    private void RespawnPlayerInit()
    {
        playerAnimator.gameObject.SetActive(true);
        velocity = Vector2.zero;
        horizonSpeed = 0;
        jumped = false;
        jumpTimeProgress = 0;
        preOnGround = false;
        playerState = PlayerIs.NormalMove;
        playerDarkState = PlayerDarkIs.NonDark;
        WallSlideJumpValsReset();
        inDarksideTimer = 0;
        blackFire.Stop();
        ResetNumForJumping();
        jumpCoyoteTimer = 0;
        jumpBufferTimer = 0;
        onWallBufferTimer = 0;
    }

    private T GetStatusByInDarksideTimer<T>(in T normal, in T darkside)
    {
        if (inDarksideTimer < inDarksideReinforceTime)
        {
            return normal;
        }
        else if (inDarksideTimer < inDarksideDeathTime)
        {
            return darkside;
        }
        else
        {
            return darkside;     //ここに入ったならプレイヤーは死ぬ必要がある
        }
    }

    private float GetStatusHorizontalAccel()
    {
        return GetStatusByInDarksideTimer<float>(in horizontalAccel, in horizontalAccelDarkside);
    }

    private float GetStatusMaxHorizontalSpeed()
    {
        return GetStatusByInDarksideTimer<float>(in maxHorizontalSpeed, in maxHorizontalSpeedDarkside);
    }

    private float GetStatusHorizontalStopAccel()
    {
        return GetStatusByInDarksideTimer<float>(in horizontalStopAccel, in horizontalStopAccelDarkside);
    }

    private float GetStatusJumpVelocity()
    {
        return GetStatusByInDarksideTimer<float>(in jumpVelocity, in jumpVelocityDarkside);
    }

    private float GetStatusJumpUpTime()
    {
        return GetStatusByInDarksideTimer<float>(in jumpUpTime, in jumpUpTimeDarkside);
    }

    private float GetStatusWallClimbSpeed()
    {
        return GetStatusByInDarksideTimer<float>(in wallClimbSpeed, in wallClimbSpeedDarkside);
    }

    private float GetStatusWallVerticalJumpSpeed()
    {
        return GetStatusByInDarksideTimer<float>(in wallVerticalJumpSpeed, in wallVerticalJumpSpeedDarkside);
    }

    private float GetStatusWallReverseJumpSpeedX()
    {
        return GetStatusByInDarksideTimer<float>(in wallReverseJumpSpeedX, in wallReverseJumpSpeedXDarkside);
    }

    private float GetStatusWallReverseJumpSpeedY()
    {
        return GetStatusByInDarksideTimer<float>(in wallReverseJumpSpeedY, in wallReverseJumpSpeedYDarkside);
    }

    private float GetStatusWallSlideJumpSpeedX()
    {
        return GetStatusByInDarksideTimer<float>(in wallSlideJumpSpeedX, in wallSlideJumpSpeedXDarkside);
    }

    private float GetStatusWallSlideJumpSpeedY()
    {
        return GetStatusByInDarksideTimer<float>(in wallSlideJumpSpeedY, in wallSlideJumpSpeedYDarkside);
    }

    private float GetStatusWallSlideJumpedSpeedXRetentionTime()
    {
        return GetStatusByInDarksideTimer<float>(in wallSlideJumpedSpeedXRetentionTime, in wallSlideJumpedSpeedXRetentionTimeDarkside);
    }

    private float GetStatusWallSlideJumpUpTime()
    {
        return GetStatusByInDarksideTimer<float>(in wallSlideJumpUpTime, in wallSlideJumpUpTimeDarkside);
    }


    private enum PlayerIs
    {
        NormalMove,
        onWall,
        onWallSlide,
        LightDashing
    }

    private enum PlayerDarkIs
    {
        NonDark,
        Dark1,
        Death
    }

    private enum JumpIs
    {
        None,
        NormalJump,
        WallNonVerticalJump,
        WallVerticalJump,
        WallSlideJump,
        AirJump
    }
}
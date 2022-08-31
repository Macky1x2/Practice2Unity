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
    public ParticleSystem blackFire;
    public Animator playerAnimator;     //playerStateについて　0:Idle 1:Run 2:Jump 3:JumptoFall 4:Fall 5:Edge-Grab 6:Edge-Idle 7:Wall-Slide 8:Dashing

    public float maxHorizontalSpeed;
    public float maxHorizontalSpeedDarkside;
    public float horizontalAccel;
    public float horizontalAccelDarkside;
    public float horizontalStopAccel;
    public float horizontalStopAccelDarkside;

    public float gravityAccel;
    public float maxGravitySpeed;

    public float jumpVelocity;
    public float jumpVelocityDarkside;
    public float jumpUpTime;
    public float jumpUpTimeDarkside;

    public float wallStaminaMax;
    public float wallClimbStaminaTimeMultiple;
    public float wallVerticalJumpStamina;
    public float wallClimbSpeed;
    public float wallClimbSpeedDarkside;
    public float wallVerticalJumpSpeed;
    public float wallVerticalJumpSpeedDarkside;
    public float wallReverseJumpSpeedX;
    public float wallReverseJumpSpeedXDarkside;
    public float wallReverseJumpSpeedY;
    public float wallReverseJumpSpeedYDarkside;

    public float wallSlideJumpSpeedX;
    public float wallSlideJumpSpeedXDarkside;
    public float wallSlideJumpSpeedY;
    public float wallSlideJumpSpeedYDarkside;
    public float wallSlideJumpedSpeedXRetentionTime;
    public float wallSlideJumpedSpeedXRetentionTimeDarkside;
    public float wallSlideMaxSpeed;
    public float wallSlideJumpUpTime;
    public float wallSlideJumpUpTimeDarkside;

    public float lightDashSpeed;
    public float lightDashTime;
    public float lightDashUpEndSpeedY;
    public float lightDashRLUpEndSpeedY;
    public float putSquareLightSpan;

    public float inDarksideReinforceTime;
    public float inDarksideDeathTime;

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

    private bool onWallSlideJumped;
    private float wallSlideJumpedSpeedXRetentionTimer;

    private bool jumped;
    private float jumpTimeProgress;
    private int airJumpCounter;
    private JumpIs nowJumpIs;

    private Vector2 lightDashDirection;
    private bool lightDashed;
    private bool lightDashing;
    private float lightDashTimer;
    private Light2D squareLightPrefabs;
    private Light2D squareLightSubPrefabs;
    private float putSquareLightTimer;

    private bool inDarkside;
    private float inDarksideTimer;

    private Rigidbody2D rb;
    private Vector2 velocity;
    private PlayerIs playerState;
    private PlayerDarkIs playerDarkState;


    // Start is called before the first frame update
    void Start()
    {
        rb = this.transform.GetComponent<Rigidbody2D>();
        velocity = rb.velocity;
        horizonSpeed = 0;
        jumped = false;
        jumpTimeProgress = 0;
        preOnGround = false;
        playerState = PlayerIs.NormalMove;
        playerDarkState = PlayerDarkIs.NonDark;
        onWallSlideJumped = false;
        squareLightPrefabs = Resources.Load<Light2D>("Prefabs/Player Light");
        squareLightSubPrefabs= Resources.Load<Light2D>("Prefabs/Player Light Sub");
        inDarksideTimer = 0;
        blackFire.Stop();
        ResetNumForJumping();
    }

    // Update is called once per frame
    void Update()
    {
        GetInformationFromChildren();       //子であるトリガーなどから情報を得てフィールドに代入(Velocityの計算の前にできる限りVelocityに関わる変数を決定しておく)
        VelocityUpdate();                   //プレイヤーの速度計算およびrb.velocityへの代入
        AnimeOffsetUpdate();                //各アニメーションの位置を当たり判定にあわせる
        EffectsUpdate();                    //エフェクトの更新など(ただし、ここだけではない)

        KeepPreFlags();                     //次のUpdateのために必要な情報を保存
    }

    private void VelocityUpdate()
    {
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

        //この時点でvevlocityにはプレイヤーが取るべき速度が入っている
        rb.velocity = velocity;
    }

    private void JumpEnd()
    {
        ResetNumForJumping();
    }

    private bool NormalJumpStart() {
        JumpFlagsReset();
        velocity = new Vector2(velocity.x, GetStatusJumpVelocity());
        return true;
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
        JumpFlagsReset();
        playerState = PlayerIs.NormalMove;
    }

    private bool WallSlideJumpStart()
    {
        JumpFlagsReset();
        onWallSlideJumped = true;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        playerAnimator.SetBool("isRight", !playerAnimator.GetBool("isRight"));
        if (Input.GetAxis("Horizontal") > 0)
        {
            velocity = new Vector2(-GetStatusWallSlideJumpSpeedX(), GetStatusWallSlideJumpSpeedY());
            horizonSpeed = -GetStatusWallSlideJumpSpeedX();
            wallSlideJumpedSpeedXRetentionTimer = 0;
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            velocity = new Vector2(GetStatusWallSlideJumpSpeedX(), GetStatusWallSlideJumpSpeedY());
            horizonSpeed = GetStatusWallSlideJumpSpeedX();
            wallSlideJumpedSpeedXRetentionTimer = 0;
        }
        return true;
    }

    private void JumpFlagsReset()
    {
        onWallSlideJumped = false;
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
            bool tmpJumped = false;
            if (onGround)
            {
                tmpJumped = true;
                NormalJumpStart();
                nowJumpIs = JumpIs.NormalJump;
                //JumpStart();
            }
            else if (playerState == PlayerIs.NormalMove && CanAirJump())
            {

            }
            else if (playerState == PlayerIs.onWall && ((onWallAndLeft && Input.GetAxis("Horizontal") < 0) || (!onWallAndLeft && Input.GetAxis("Horizontal") > 0)))
            {
                tmpJumped = true;
                WallJumpStartNonVertical();
                nowJumpIs = JumpIs.WallNonVerticalJump;
                //JumpStart();
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
                //JumpStart();
            }

            if (tmpJumped)
            {
                playerAnimator.SetInteger("playerState", 2);
                horizonSpeed = velocity.x;
                jumped = true;
                jumpTimeProgress = 0;
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
        //if(!Input.GetButton("Dash")) Debug.Log("test");

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
        onRoof = roofCheck.GetOnRoofEnter();
        onRoofAnd90 = roofCheck.GetOnRoof90();

        //壁
        onWall = wallCheck.IsTriggerCheck();
        onWallAndLeft = wallCheck.IsLeftOnWall;
        onWallAndRight = wallCheck.IsRightOnWall;

        //闇
        inDarkside = darksideCheck.IsTriggerCheck();

        //時間変数更新(ただし、時間変数の変更はここ以外でも存在する)
        TimerUpdate();
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
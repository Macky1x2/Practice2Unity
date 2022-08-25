using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GroundChecker groundCheck;
    public RoofChecker roofCheck;
    public float maxHorizontalSpeed;
    public float horizontalAccel;
    public float horizontalStopAccel;
    public float gravityAccel;
    public float maxGravitySpeed;
    public float JumpVelocity;
    public float jumpUpTime;

    private bool onGround;
    private bool preOnGround;
    private bool onRoof;
    private Rigidbody2D rb;
    private Vector2 horizonMoveDirection;
    private Vector2 velocity;
    private float horizonSpeed;
    private bool jumped;
    private bool jumping;
    private float jumpTimeProgress;
    private bool onRoofAnd90;

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
    }

    // Update is called once per frame
    void Update()
    {
        onGround = groundCheck.OnGroundCheck();
        onRoof = roofCheck.GetOnRoofEnter();
        onRoofAnd90 = roofCheck.GetOnRoof90();
        if (!preOnGround && onGround)
        {
            jumped = false;
            jumping = false;
        }
        horizonMoveDirection = groundCheck.getMoveDirection();
        VelocityUpdate();

        preOnGround = onGround;
    }

    private void VelocityUpdate()
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
            if(onGround && !jumped) verticalSpeed = 0;
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

        //ƒWƒƒƒ“ƒvˆ—
        if (Input.GetButtonDown("Jump"))
        {
            if (onGround && !onRoof)
            {
                velocity = new Vector2(velocity.x, JumpVelocity);
                horizonSpeed = velocity.x;
                jumped = true;
                jumping = true;
                jumpTimeProgress = 0;
            }
            //else if (jumped && Input.GetButton("Jump"))
            //{
            //    rb.velocity = new Vector2(rb.velocity.x, JumpVelocity);
            //}
        }
        else if (Input.GetButton("Jump"))
        {
            if (onRoof)
            {
                JumpEnd();
            }
            else if(!onGround && jumping && jumpTimeProgress <= jumpUpTime)
            {
                velocity = new Vector2(velocity.x, JumpVelocity);
                jumpTimeProgress += Time.deltaTime;
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

        rb.velocity = velocity;
    }

    private void JumpEnd()
    {
        jumping = false;
    }
}

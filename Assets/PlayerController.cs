using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GroundChecker groundCheck;
    public float maxHorizontalSpeed;
    public float horizontalAccel;
    public float horizontalStopAccel;
    public float gravityAccel;
    public float maxGravitySpeed;
    public float jumpInitialVelocity;

    private bool onGround;
    private Rigidbody2D rb;
    private Vector2 horizonMoveDirection;
    private Vector2 velocity;
    private float horizonSpeed;
    private bool jumped;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.transform.GetComponent<Rigidbody2D>();
        velocity = rb.velocity;
        horizonSpeed = 0;
        jumped = false;
    }

    // Update is called once per frame
    void Update()
    {
        onGround = groundCheck.OnGroundCheck();
        if (groundCheck.getOnGroundEnter()) jumped = false;
        horizonMoveDirection = groundCheck.getMoveDirection();
        VelocityUpdate();
    }

    void VelocityUpdate()
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
            if (onGround)
            {
                velocity = new Vector2(velocity.x, jumpInitialVelocity);
                horizonSpeed = velocity.x;
                jumped = true;
            }
            //else if (jumped && Input.GetButton("Jump"))
            //{
            //    rb.velocity = new Vector2(rb.velocity.x, jumpInitialVelocity);
            //}
        }

        rb.velocity = velocity;
    }

    void HorizonUpdate(ref float horizonSpeed)
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
        }
    }

    void VerticalUpdate(ref float verticalSpeed)
    {
        if (!onGround)
        {
            if (verticalSpeed > -maxGravitySpeed)
            {
                verticalSpeed -= gravityAccel * Time.deltaTime;
                if (verticalSpeed < -maxGravitySpeed) verticalSpeed = -maxGravitySpeed;
            }
        }
        else
        {
            verticalSpeed = 0;
        }
    }
}

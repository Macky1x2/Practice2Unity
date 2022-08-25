using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    public float maxHorizontalSpeed;
    public float horizontalAccel;
    public float horizontalStopAccel;
    public float gravityAccel;
    public float maxGravitySpeed;

    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateVelocity();
        transform.Translate(velocity);
    }

    void UpdateVelocity()
    {
        float horizonSpeed = velocity.x;
        float verticalSpeed = velocity.y;
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

        if (verticalSpeed > -maxGravitySpeed)
        {
            verticalSpeed -= gravityAccel * Time.deltaTime;
            if (verticalSpeed < -maxGravitySpeed) verticalSpeed = -maxGravitySpeed;
        }

        velocity = new Vector3(horizonSpeed, verticalSpeed, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Is Trigger : OFF ‚Å, “–‚½‚è”»’è‚É“ü‚Á‚½‚Æ‚«.");
    }
}

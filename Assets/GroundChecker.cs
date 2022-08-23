using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private bool onGroundEnter, onGroundStay, onGroundExit;
    private string groundTag;
    private bool onGround;
    private Vector2 moveDirection;

    // Start is called before the first frame update
    void Start()
    {
        onGround = false;
        onGroundEnter = false;
        onGroundStay = false;
        onGroundExit = false;
        groundTag = "Ground";
        moveDirection = Vector2.right;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        onGroundEnter = false;
        onGroundStay = false;
        onGroundExit = false;
    }

    public bool OnGroundCheck()
    {
        if (onGroundEnter || onGroundStay) onGround = true;
        else if(onGroundExit) onGround = false;
        return onGround;
    }

    public Vector2 getMoveDirection()
    {
        return moveDirection;
    }

    public bool getOnGroundEnter()
    {
        return onGroundEnter;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == groundTag)
        {
            ContactPoint2D[] contacts = new ContactPoint2D[4];
            int contactsNum = collision.GetContacts(contacts);
            if (contactsNum > 0)
            {
                onGroundEnter = true;
                Vector2 normal = contacts[0].normal;
                Vector2 direction = moveDirection - Vector2.Dot(moveDirection, normal) * normal;
                moveDirection = direction.normalized;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            ContactPoint2D[] contacts = new ContactPoint2D[4];
            int contactsNum = collision.GetContacts(contacts);
            if (contactsNum > 0)
            {
                onGroundStay = true;
                Vector2 normal = contacts[0].normal;
                Vector2 direction = moveDirection - Vector2.Dot(moveDirection, normal) * normal;
                moveDirection = direction.normalized;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            onGroundExit = true;
            moveDirection = Vector2.right;
        }
    }
}

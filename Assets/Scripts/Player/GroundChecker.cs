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
        groundTag = "Block";
        moveDirection = Vector2.right;
    }

    private void LateUpdate()
    {
        onGroundEnter = false;
        onGroundStay = false;
        onGroundExit = false;
    }

    public bool OnGroundCheck()
    {
        //OnTriggerStay2DはUpdateの前に必ずしも実行されない(FixedUpdateの前に実行されている?)ためonGroundExit, onGroundが必要
        if (onGroundEnter || onGroundStay) onGround = true;
        else if(onGroundExit) onGround = false;
        return onGround;
    }

    public Vector2 getMoveDirection()
    {
        return moveDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            collisionProcessOnGround(in collision, ref onGroundEnter);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            collisionProcessOnGround(in collision, ref onGroundStay);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == groundTag)
        {
            collisionExitProcessOnGround();
        }
    }
    
    //地面に対する処理
    private void collisionProcessOnGround(in Collider2D collision, ref bool onGroundEnterOrStay)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[8];
        int contactsNum = collision.GetContacts(contacts);
        if (contactsNum > 0)
        {
            Vector2 normal = new Vector2(0, 0);
            float angle = 0;
            for (int i = 0; i < contactsNum; i++)
            {
                Vector2 tmpNormal = contacts[i].normal;
                float tmpAngle = Vector2.SignedAngle(tmpNormal, new Vector2(1, 0));
                if (45 <= tmpAngle && tmpAngle <= 135)
                {
                    normal = contacts[i].normal;
                    angle = tmpAngle;
                    break;
                }
            }
            //Vector2 normal = contacts[0].normal;
            //float angle = Vector2.Angle(new Vector2(1, 0), normal);
            if (45 <= angle && angle <= 135)
            {
                Vector2 direction = moveDirection - Vector2.Dot(moveDirection, normal) * normal;
                moveDirection = direction.normalized;
                onGroundEnterOrStay = true;
            }
            else
            {
                collisionExitProcessOnGround();
            }
        }
    }
    private void collisionExitProcessOnGround()
    {
        onGroundExit = true;
        moveDirection = Vector2.right;
    }
}

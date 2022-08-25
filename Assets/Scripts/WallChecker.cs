using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallChecker : MonoBehaviour
{
    private string wallTag;
    private bool onWallEnter, onWallStay, onWallExit;
    private bool onWall;
    private bool isLeftIfOnWall;
    
    // Start is called before the first frame update
    void Start()
    {
        wallTag = "Block";
        onWall = false;
        onWallEnter = false;
        onWallStay = false;
        onWallExit = false;
        isLeftIfOnWall = false;
    }

    private void LateUpdate()
    {
        onWallEnter = false;
        onWallStay = false;
        onWallExit = false;
    }

    public bool OnWallCheck()
    {
        //OnTriggerStay2DはUpdateの前に必ずしも実行されない(FixedUpdateの前に実行されている?)ためonWallExit, onWallが必要
        if (onWallEnter || onWallStay) onWall = true;
        else if (onWallExit) onWall = false;
        return onWall;
    }

    public bool GetIsLeftIfOnWall()
    {
        return isLeftIfOnWall;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == wallTag)
        {
            collisionProcessOnWall(in collision, ref onWallEnter);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == wallTag)
        {
            collisionProcessOnWall(in collision, ref onWallStay);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == wallTag)
        {
            onWallExit = true;
        }
    }

    //地面に対する処理
    private void collisionProcessOnWall(in Collider2D collision, ref bool onWallEnterOrStay)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[4];
        int contactsNum = collision.GetContacts(contacts);
        if (contactsNum > 0)
        {
            Vector2 normal = contacts[0].normal;
            float angle = Vector2.Angle(new Vector2(1, 0), normal);
            if (angle == 0 || angle == 180)
            {
                onWallEnterOrStay = true;
                isLeftIfOnWall = angle == 0;
            }
        }
    }
}

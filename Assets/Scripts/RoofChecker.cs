using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofChecker : MonoBehaviour
{
    private string roofTag;
    private bool onRoofEnter;
    private bool onRoof90;
    
    // Start is called before the first frame update
    void Start()
    {
        roofTag = "Block";
        onRoofEnter = false;
        onRoof90 = false;
    }

    void LateUpdate()
    {
        onRoofEnter = false;
        onRoof90 = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == roofTag)
        {
            collisionProcessOnRoof(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == roofTag)
        {
            collisionProcessOnRoof(collision);
        }
    }

    public bool GetOnRoofEnter()
    {
        return onRoofEnter;
    }

    public bool GetOnRoof90()
    {
        return onRoof90;
    }

    private void collisionProcessOnRoof(in Collider2D collision)
    {
        onRoofEnter = true;
        onRoof90 = false;
        ContactPoint2D[] contacts = new ContactPoint2D[4];
        int contactsNum = collision.GetContacts(contacts);
        if (contactsNum > 0)
        {
            for (int i = 0; i < contactsNum; i++)
            {
                Vector2 normal = contacts[i].normal;
                float angle = Vector2.SignedAngle(normal, new Vector2(1, 0));
                if (angle == -90)
                {
                    onRoof90 = true;
                    break;
                }
            }
        }
    }
}

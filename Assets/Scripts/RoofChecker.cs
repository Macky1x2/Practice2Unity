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
            onRoofEnter = true;
            onRoof90 = false;
            ContactPoint2D[] contacts = new ContactPoint2D[4];
            int contactsNum = collision.GetContacts(contacts);
            if (contactsNum > 0)
            {
                Vector2 normal = contacts[0].normal;
                float angle = Vector2.Angle(new Vector2(1, 0), normal);
                if (angle == 90) onRoof90 = true;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == roofTag)
        {
            onRoofEnter = true;
            onRoof90 = false;
            ContactPoint2D[] contacts = new ContactPoint2D[4];
            int contactsNum = collision.GetContacts(contacts);
            if (contactsNum > 0)
            {
                Vector2 normal = contacts[0].normal;
                float angle = Vector2.Angle(new Vector2(1, 0), normal);
                if (angle == 90) onRoof90 = true;
            }
        }
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.tag == roofTag)
    //    {
    //        onRoofEnter = false;
    //        onRoof90 = false;
    //    }
    //}

    public bool GetOnRoofEnter()
    {
        return onRoofEnter;
    }

    public bool GetOnRoof90()
    {
        return onRoof90;
    }
}

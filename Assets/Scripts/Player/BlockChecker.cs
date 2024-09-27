using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockChecker : BaseTrigger
{
    [SerializeField] private float targetAngle;
    [SerializeField] private bool lfMirror;


    override protected void collisionProcessInTrigger(in Collider2D collision, ref bool triggerEnterOrStay)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[4];
        int contactsNum = collision.GetContacts(contacts);
        if (contactsNum > 0)
        {
            bool firstFlag = false;
            for (int i = 0; i < contactsNum; i++)
            {
                Vector2 normal = contacts[i].normal;
                float angle = Vector2.SignedAngle(normal, new Vector2(1, 0));
                if (angle == targetAngle || lfMirror && angle == 180 - targetAngle)
                {
                    firstFlag = true;
                    triggerEnterOrStay = true;
                    break;
                }
            }
            if (!firstFlag)
            {
                collisionProcessExitTrigger(collision);
            }
        }
    }

    override protected void collisionProcessExitTrigger(in Collider2D collision)
    {
        triggerExit = true;
    }
}
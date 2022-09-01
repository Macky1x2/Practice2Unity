using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarksideAndDeathChecker : BaseTrigger
{
    override protected void collisionProcessInTrigger(in Collider2D collision, ref bool triggerEnterOrStay)
    {
        triggerEnterOrStay = true;
    }

    override protected void collisionProcessExitTrigger(in Collider2D collision)
    {
        triggerExit = true;
    }
}

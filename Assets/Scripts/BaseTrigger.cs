using UnityEngine;

public class BaseTrigger : MonoBehaviour
{
    public string[] targetTag;


    protected bool triggerEnter, triggerStay, triggerExit;
    protected bool isTrigger;


    // Start is called before the first frame update
    protected void Start()
    {
        isTrigger = false;
        triggerEnter = false;
        triggerStay = false;
        triggerExit = false;
    }

    private void LateUpdate()
    {
        triggerEnter = false;
        triggerStay = false;
        triggerExit = false;
    }

    public virtual bool IsTriggerCheck()
    {
        //OnTriggerStay2D��Update�̑O�ɕK���������s����Ȃ�(FixedUpdate�̑O�Ɏ��s����Ă���?)����onGroundExit, onGround���K�v
        if (triggerEnter || triggerStay) isTrigger = true;
        else if (triggerExit) isTrigger = false;
        return isTrigger;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        for(int i = 0; i < targetTag.Length; i++)
        {
            if (collision.tag == targetTag[i])
            {
                collisionProcessInTrigger(in collision, ref triggerEnter);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        for (int i = 0; i < targetTag.Length; i++)
        {
            if (collision.tag == targetTag[i])
            {
                collisionProcessInTrigger(in collision, ref triggerStay);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        for (int i = 0; i < targetTag.Length; i++)
        {
            if (collision.tag == targetTag[i])
            {
                collisionProcessExitTrigger(in collision);
            }
        }
    }

    protected virtual void collisionProcessInTrigger(in Collider2D collision, ref bool triggerEnterOrStay)
    {

    }

    protected virtual void collisionProcessExitTrigger(in Collider2D collision)
    {

    }
}

using UnityEngine;

public class RoofCheckerBase : MonoBehaviour
{
    public string targetTag;
    public float targetAngle;


    protected bool onTargetAndTargetAngle;

    // Start is called before the first frame update
    void Start()
    {
        onTargetAndTargetAngle = false;
    }

    void LateUpdate()
    {
        onTargetAndTargetAngle = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == targetTag)
        {
            collisionProcessOnTarget(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == targetTag)
        {
            collisionProcessOnTarget(collision);
        }
    }

    public bool GetOnTarget()
    {
        return onTargetAndTargetAngle;
    }

    protected virtual void collisionProcessOnTarget(in Collider2D collision)
    {
        onTargetAndTargetAngle = false;
        ContactPoint2D[] contacts = new ContactPoint2D[4];
        int contactsNum = collision.GetContacts(contacts);
        if (contactsNum > 0)
        {
            for (int i = 0; i < contactsNum; i++)
            {
                Vector2 normal = contacts[i].normal;
                float angle = Vector2.SignedAngle(normal, new Vector2(1, 0));
                if (angle == targetAngle)
                {
                    onTargetAndTargetAngle = true;
                    break;
                }
            }
        }
    }
}

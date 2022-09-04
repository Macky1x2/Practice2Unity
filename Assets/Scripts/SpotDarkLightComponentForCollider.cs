using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotDarkLightComponentForCollider : MonoBehaviour
{
    private UnityEngine.Rendering.Universal.Light2D myLight;
    private PolygonCollider2D myCollider;

    // Start is called before the first frame update
    void Start()
    {
        myLight = this.transform.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        myCollider = this.transform.GetComponent<PolygonCollider2D>();
        Vector2[] path = new Vector2[myLight.shapePath.Length];
        for (int i=0;i< myLight.shapePath.Length; i++)
        {
            path[i] = new Vector2(myLight.shapePath[i].x, myLight.shapePath[i].y);
        }
        myCollider.SetPath(0, path);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

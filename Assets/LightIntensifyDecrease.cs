using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightIntensifyDecrease : MonoBehaviour
{
    public float decreaseSpeed;

    private Light2D decreaseLight;
    
    // Start is called before the first frame update
    void Start()
    {
        decreaseLight = this.transform.GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        decreaseLight.intensity -= decreaseSpeed * Time.deltaTime;
        if (decreaseLight.intensity < 1)
        {
            Destroy(this.gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightDashSubLightLifetimer : MonoBehaviour
{
    private float lifetime;
    private Light2D squareLightPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        squareLightPrefabs = Resources.Load<Light2D>("Prefabs/Player Light");
        LightIntensifyDecrease lid = squareLightPrefabs.transform.GetComponent<LightIntensifyDecrease>();
        lifetime = (squareLightPrefabs.intensity - 1) / lid.DecreaseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime <= 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            lifetime -= Time.deltaTime;
        }
    }
}

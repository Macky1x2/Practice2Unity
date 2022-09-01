using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFadeOut : MonoBehaviour
{
    public float fadeTime;
    public float blackTime;


    private Image blackImage;
    private bool isActive = false;
    private float fadeTimer;
    private float blackTimer;
    private bool isBlack = false;


    // Start is called before the first frame update
    void Start()
    {
        blackImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            if (fadeTimer > 0)
            {
                blackImage.color = new Color(0, 0, 0, (fadeTime - fadeTimer) / fadeTime);
            }
            else if (fadeTimer > -fadeTime)
            {
                if (blackTimer > 0)
                {
                    isBlack = true;
                    blackImage.color = new Color(0, 0, 0, 1);
                    blackTimer -= Time.deltaTime;
                }
                else
                {
                    isBlack = false;
                    blackImage.color = new Color(0, 0, 0, (fadeTime + fadeTimer) / fadeTime);
                }
            }
            else
            {
                blackImage.color = new Color(0, 0, 0, 0);
                isActive = false;
            }
            fadeTimer -= Time.deltaTime;
        }
    }

    public void StartFadeOutAndIn()
    {
        isActive = true;
        fadeTimer = fadeTime;
        blackTimer = blackTime;
    }

    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    public bool IsBlack
    {
        get
        {
            return isBlack;
        }
    }
}

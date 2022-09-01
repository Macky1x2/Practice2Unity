using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionGameManager : MonoBehaviour
{
    public CameraFadeOut fade;
    [System.NonSerialized] public bool fadeEnd = false;
    [System.NonSerialized] public bool fadeBlackStart = false;


    private bool fadeActive = false;
    private bool fadeActivePre = false;

    private bool fadeIsBlack = false;
    private bool fadeIsBlackPre = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fadeActive = fade.IsActive;
        if(fadeActivePre && !fadeActive)
        {
            fadeEnd = true;
        }

        fadeIsBlack = fade.IsBlack;
        if (fadeIsBlackPre && !fadeIsBlack)
        {
            fadeBlackStart = true;
        }

        fadeActivePre = fadeActive;
        fadeIsBlackPre = fadeIsBlack;
    }

    private void LateUpdate()
    {
        fadeEnd = false;
        fadeBlackStart = false;
    }

    public void FadeStart()
    {
        fade.StartFadeOutAndIn();
    }
}

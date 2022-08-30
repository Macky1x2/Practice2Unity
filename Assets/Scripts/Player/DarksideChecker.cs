using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarksideChecker : MonoBehaviour
{
    private bool darksideEnter, darksideStay, darksideExit;
    private string darksideTag;
    private bool inDarkside;

    // Start is called before the first frame update
    void Start()
    {
        inDarkside = false;
        darksideEnter = false;
        darksideStay = false;
        darksideExit = false;
        darksideTag = "Darkside";
    }

    private void LateUpdate()
    {
        darksideEnter = false;
        darksideStay = false;
        darksideExit = false;
    }

    public bool inDarksideCheck()
    {
        //OnTriggerStay2D��Update�̑O�ɕK���������s����Ȃ�(FixedUpdate�̑O�Ɏ��s����Ă���?)����darksideExit, inDarkside���K�v
        if (darksideEnter || darksideStay) inDarkside = true;
        else if (darksideExit) inDarkside = false;
        return inDarkside;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

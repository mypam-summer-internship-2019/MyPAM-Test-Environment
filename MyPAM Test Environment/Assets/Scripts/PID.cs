using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PID : MonoBehaviour
{


    public Slider KP;
    public Slider KI;
    public Slider KD;

    public Text Kp;
    public Text Ki;
    public Text Kd;

    static bool MyPAMMode = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (!MyPAMMode)
        {
            UDP_Handling.X_Attractor1 = (double)KP.value;
            Kp.text = KP.value.ToString();

            UDP_Handling.Y_Attractor1 = (double)KI.value;
            Ki.text = KI.value.ToString();

            UDP_Handling.Z_Attractor1 = (double)KD.value;
            Kd.text = KD.value.ToString();
        }
    }
}

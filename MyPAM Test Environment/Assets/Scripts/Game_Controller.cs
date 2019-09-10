using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
public class Game_Controller : MonoBehaviour
{
    public Text xText;
    public Text yText;
    public Text rText;

    public float savedXPos = 0;
    public float savedYPos = 0;

    public GameObject endeffector ;
    void Start()
    {
        
    }

    void Update()
    {
        xText.text = UDP_Handling.X0pos.ToString();
        yText.text = UDP_Handling.Y0pos.ToString();

        savedXPos = (float)(UDP_Handling.X2pos * 0.0233);
        savedYPos = (float)(UDP_Handling.Y2pos * 0.0233);
        endeffector.transform.position = new Vector3(savedXPos, savedYPos, -6);

        rText.text = UDP_Handling.receivedData;
    }
}

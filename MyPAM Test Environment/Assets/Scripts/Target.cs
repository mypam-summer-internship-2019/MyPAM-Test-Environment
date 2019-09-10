using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool colliding = true;
    private static float ranY = 0;
    private static float ranX = 0;
    public Toggle followMouse;
    private Vector3 target;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followMouse.isOn)
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = target + new Vector3 (0,0,10f);

            UDP_Handling.Xtarget = target.x/ 0.0233f;
            UDP_Handling.Ytarget = target.y / 0.0233f;
        }
        else
        {
            if (colliding == true)
            {
                ranX = Random.Range(-150f, 150f);
                if (ranX < (Mathf.Abs((float)(UDP_Handling.X0pos)) + 20f))
                {
                    ranX = Random.Range(-150f, 150f);
                }

                ranY = Random.Range(-105f, 105f);
                if (ranY < (Mathf.Abs((float)(UDP_Handling.Y0pos)) + 20f))
                {
                    ranY = Random.Range(-105f, 105f);
                }

                UDP_Handling.Xtarget = ranX;
                UDP_Handling.Ytarget = ranY;

                transform.position = new Vector3(ranX * 0.0233f, ranY * 0.0233f, -6f);
                colliding = false;
            }
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        colliding = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        colliding = true;
    }
}

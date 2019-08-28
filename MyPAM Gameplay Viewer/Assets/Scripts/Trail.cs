using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Threading;
using UnityEngine.UI;

public class Trail : MonoBehaviour
{
    int nameVal;
    void Start()
    {
        nameVal = int.Parse(name);
    }

    // Update is called once per frame
    void Update()
    {
        if (nameVal <= Main.playerCount && nameVal > (Main.playerCount - Main.trailLengthVal))
        {

        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}

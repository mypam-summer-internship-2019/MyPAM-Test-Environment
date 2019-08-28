using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class play_pause : MonoBehaviour
{
    public Sprite play;
    public Sprite pause;
    public Button playPause;

    public void flip()
    {
        if (playPause.image.sprite == play)
            playPause.image.sprite = pause;
        else
        {
            playPause.image.sprite = play;
        }
    }

    public void reset()
    {
        playPause.image.sprite = play;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls the FPS counter for reference to spinning
/// </summary>
public class FPSManager : MonoBehaviour
{
    private static FPSManager _instance;
    public static FPSManager instance
    {
        get
        {
            if (_instance)
                _instance = GameObject.FindObjectOfType<FPSManager>();
            return _instance;
        }
    }

    public static float highest_fps = 30.0f;
    //Keeps track of the FPS so the reels spinning are affected by the games FPS
    float updateInterval = 0.5f;

    private float accum = 0.0f; // FPS accumulated over the interval
    private float framesPerSecond = 0;
    public int frames = 0; // Frames drawn over the interval
    private float timeleft = .5f; // Left time for current interval
    
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0)
        {
            PlayAnimation();
        }*/
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            framesPerSecond = accum / frames;
            ResetTimeLeftFPS();
            accum = 0.0f;
            frames = 0;
        }
    }
    void ResetTimeLeftFPS()
    {
        timeleft = updateInterval;
    }
}

public static class StaticFPSManager
{

}

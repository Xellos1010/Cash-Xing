using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemClock : MonoBehaviour
{
    static System.Timers.Timer t;
    public delegate void SystemTimeIntervalActivated();
    public static event SystemTimeIntervalActivated timerReached;
    DateTime now;
    public TMPro.TextMeshPro tmpSystemTime;
    public void SetDateTimeToNow()
    {
        now = DateTime.Now;
    }
    // Start is called before the first frame update
    void OnEnable()
    {
        InitializeTimer();
        SetClockToSystemTime();
    }

    void OnDisable()
    {
        timerReached -= SetClockToSystemTime;
        t.Stop();
    }

    private void InitializeTimer()
    {
        timerReached += SetClockToSystemTime; 
        t = new System.Timers.Timer();
        t.AutoReset = false;
        t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
        t.Interval = GetInterval();
        t.Start();
    }

    private void SetClockToSystemTime()
    {
        SetDateTimeToNow();
        tmpSystemTime.text = $"{now:HH:mm}";
    }


    static double GetInterval()
    {
        DateTime now = DateTime.Now;
        return ((60 - now.Second) * 1000 - now.Millisecond);
    }

    static void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        timerReached?.Invoke();
        t.Interval = GetInterval();
        t.Start();
    }
}

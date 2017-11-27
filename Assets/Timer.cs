using UnityEngine;
using System.Collections;

public class Timer
{
    public delegate void Action();

    private float fullLength;
    private float timeLeft;
    private Action onEnd;

    public Timer(float length, Action onEnd)
    {
        fullLength = timeLeft = length;
        this.onEnd = onEnd;
    }

    // Update is called once per frame
    public bool Update()
    {
        if (timeLeft <= 0) return true;
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            onEnd();
        }
        return false;
    }
}

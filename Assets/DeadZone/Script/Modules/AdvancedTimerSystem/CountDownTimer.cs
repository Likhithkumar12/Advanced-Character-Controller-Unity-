using ImprovedTimers;
using UnityEngine;

public class CountDownTimer : Timer
{
    public CountDownTimer(float value) : base(value)
    {
    }
    public override void Tick()
    {
        if (IsRunning && CurrentTime > 0)
        {
            CurrentTime-=Time.deltaTime;
        }
        if(IsRunning && CurrentTime <= 0)
        {
            Stop();
        }   
    }

    public override bool IsFinished => CurrentTime <= 0;
}
using UnityEngine;

public class TimerController : MonoBehaviour
{
    public Timer timer;
    public TimerView timerView;

    private void Start()
    {
        timer.timerIsRunning = true;
        timer.OnTimerFinished += OnTimerFinishedCallback;
    }

    private void Update()
    {
        if (timer.timerIsRunning)
        {
            timerView.DisplayTime(timer.timeRemaining);
        }
    }

    public void PauseTimer()
    {
        timer.timerIsRunning = false;
    }

    public void ResumeTimer()
    {
        timer.timerIsRunning = true;
    }

    private void OnTimerFinishedCallback()
    {
        
    }
}

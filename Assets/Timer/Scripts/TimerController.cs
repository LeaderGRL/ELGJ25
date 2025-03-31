using UnityEngine;

public class TimerController : MonoBehaviour
{
    public Timer timer;
    public TimerView timerView;

    private void Start()
    {
        timer.timerIsRunning = true;
    }

    private void Update()
    {
        if (timer.timerIsRunning)
        {
            timerView.DisplayTime(SecondToMinuteText(timer.timeRemaining));
            SetTimerPanelSize();
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

    public void AddTime(float time)
    {
        timer.timeRemaining += time;
    }

    public void SubtractTime(float time)
    {
        timer.timeRemaining -= time;
    }

    private string SecondToMinuteText(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void SetTimerPanelSize()
    {
        float width = timer.timeRemaining / timer.maxTime;
        timerView.SetTimerPanelSize(width);
    }

}

using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 600;
    public bool timerIsRunning = false;

    public event Action OnTimerFinished;

    private void Start()
    {
        // Starts the timer automatically
        timerIsRunning = true;
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                Debug.Log("Game Over!");
                timeRemaining = 0;
                timerIsRunning = false;
                OnTimerFinished?.Invoke();
            }
        }
    }
}

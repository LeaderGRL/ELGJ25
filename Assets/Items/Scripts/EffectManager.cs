using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private static EffectManager instance;

    [Header("References")]
    public Player player;
    public Timer timer;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void ApplyTimerEffect(float time)
    {
        timer.timeRemaining += time;
    }

    public static EffectManager GetInstance()
    {
        return instance;
    }
}

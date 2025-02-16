using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    public void DisplayTime(float timeToDisplay)
    {
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = seconds.ToString();
    }
}

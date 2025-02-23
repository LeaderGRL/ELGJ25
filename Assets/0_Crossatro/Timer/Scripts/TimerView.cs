using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    public void DisplayTime(float timeToDisplay)
    {
        timeText.text = Mathf.Floor(timeToDisplay).ToString();
    }
}

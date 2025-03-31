using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public GameObject timerPanel;

    public void DisplayTime(string time)
    {
        timeText.text = time;
    }

    public void SetTimerPanelSize(float width)
    {
        timerPanel.transform.localScale = new Vector3(width, 1, 1);
    }


}

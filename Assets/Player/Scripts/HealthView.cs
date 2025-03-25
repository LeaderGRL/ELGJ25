using Rive.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour
{
    [SerializeField] private RiveWidget m_riveWidget;

    public void UpdateHealth(int health)
    {
        m_riveWidget.Artboard.SetTextRun("HeartText", health.ToString());
    }

}

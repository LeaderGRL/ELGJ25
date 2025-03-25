using Rive.Components;
using TMPro;
using UnityEngine;

public class ScoreView : MonoBehaviour
{
    [SerializeField] private RiveWidget m_riveWidget;

    public void UpdateScore(int score)
    {
        m_riveWidget.Artboard.SetTextRun("ScoreText", score.ToString());
    }
}

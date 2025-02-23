using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public Player Player;
    public ScoreView ScoreView;

    private void OnEnable()
    {
        Player.OnScoreChange += OnScoreChanged;
    }

    private void OnDisable()
    {
        Player.OnScoreChange -= OnScoreChanged;
    }

    private void OnScoreChanged(int score)
    {
        ScoreView.UpdateScore(score);
    }

    public void AddScore(int amount)
    {
        Player.AddScore(amount);
    }

    public void RemoveScore(int amount)
    {
        Player.RemoveScore(amount);
    }
}

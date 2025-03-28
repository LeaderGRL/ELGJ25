using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public Player Player;
    public ScoreView ScoreView;
    
    public void Start()
    {
        InitScore();
    }
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
    private void InitScore()
    {
        ScoreView.UpdateScore(Player.GetScore());
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

using TMPro;
using UnityEngine;

public class ScoreView : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public void UpdateScore(int score)
    {
        scoreText.text = "Score : " + score.ToString();
    }
}

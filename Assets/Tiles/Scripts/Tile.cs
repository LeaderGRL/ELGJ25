using DG.Tweening;
using UnityEngine;

public enum TileState
{
    Default,
    Selected,
    Validated
}
public class Tile : MonoBehaviour
{
    public int width;
    public int height;

    Vector2 position;
    int score;

    //[Header("Events")]
    public delegate void TileClicked(Tile tile);
    public event TileClicked OnTileClicked;


    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public int Score
    {
        get => score;
        set => score = value;
    }

    public void SpawnAnimation()
    {
        // Start with a scale of zero.
        transform.localScale = Vector3.zero;
        // Animate to full scale.
        transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack);
    }
}

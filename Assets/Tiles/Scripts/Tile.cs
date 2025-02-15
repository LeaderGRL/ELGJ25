using UnityEngine;

public class Tile : MonoBehaviour
{
    public int width;
    public int height;

    Vector2Int position;
    int score;


    public Vector2Int Position
    {
        get => position;
        set => position = value;
    }

    public int Score
    {
        get => score;
        set => score = value;
    }
}

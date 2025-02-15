using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private List<GameObject> tilePrefabs;
    private Board Instance;
    private Dictionary<Vector2Int, Tile> tiles;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        tiles = new Dictionary<Vector2Int, Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PlaceTile(new Vector2Int(x, y));
            }
        }
    }

    public Board GetInstance()
    {
        return Instance;
    }

    public void PlaceTile(Vector2Int pos)
    {
        if (tiles.ContainsKey(pos))
        {
            return;
        }

        var tile = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Count)], transform);
        tile.transform.position = new Vector3(pos.x, 0, pos.y);
        tiles.Add(pos, tile.GetComponent<Tile>());
    }
}

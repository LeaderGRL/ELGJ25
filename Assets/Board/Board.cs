using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

public class Board : MonoBehaviour
{
    public Camera camera;

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private List<GameObject> tilePrefabs;
    [SerializeField] private LetterTile letterTilePrefab;
    
    [SerializeField]
    private GridGenerationData generationData; 

    private Board Instance;
    private Dictionary<Vector2Int, GameObject> tiles;
    private Vector2Int currentHoverTile;
    private Grid m_grid;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Vector2Int> OnTileClicked;



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
        tiles = new Dictionary<Vector2Int, GameObject>();
        m_grid = CharacterPlacementGenerator.GenerateCharPlacements(generationData.PossibleWords,
            generationData.NumWorToGenerate, "");
        Vector2Int gridSize = m_grid.GetGridSize();
        var minMaxPosGrid = m_grid.GetMinAndMaxPositionCharacterPlacement();
        for (int x =  minMaxPosGrid.Key.x; x <= minMaxPosGrid.Value.x; x++)
        {
            for (int y =  minMaxPosGrid.Key.y; y <= minMaxPosGrid.Value.y; y++)
            {
                PlaceTile(new Vector2Int(x, y));
            }
        }
        
        // for (int x = 0; x < width; x++)
        // {
        //     for (int y = 0; y < height; y++)
        //     {
        //         PlaceTile(new Vector2Int(x, y));
        //     }
        // }
    }

    private void Update()
    {
        if (!camera)
        {
            camera = Camera.main;
            return;
        }

        RaycastHit hitInfo;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Letter", "Hover", "Select")))
        {
            HandleTileHover(hitInfo.transform.gameObject);
            HandleMouseInputOnTile(hitInfo.transform.gameObject);
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

        if (m_grid.CharacterPlacements.ContainsKey(pos))
        {
            var letterTile = Instantiate(letterTilePrefab, transform);
            letterTile.transform.position = new Vector3(pos.x, 0, pos.y);
            letterTile.DisplayText.text = m_grid.CharacterPlacements[pos].ToString();
            tiles.Add(pos, letterTile.gameObject);
            return;
        }

        var tile = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Count)], transform);
        tile.transform.position = new Vector3(pos.x, 0, pos.y);
        tiles.Add(pos, tile);
    }

    private Vector2Int GetTileIndex(GameObject hitInfo)
    {
        foreach (var tile in tiles)
        {
            if (tile.Value == hitInfo)
                return tile.Key;
        }
        return -Vector2Int.one;
    }

    private void HandleTileHover(GameObject hitTile)
    {
        Vector2Int hitPosition = GetTileIndex(hitTile);

        if (hitPosition == -Vector2Int.one)
        {
            ResetHoverState();
            return;
        }

        if (hitTile.layer != LayerMask.NameToLayer("Letter"))
        {
            return;
        }

        // Hovering over a new tile
        if (currentHoverTile == -Vector2Int.one)
        {
            Debug.Log("Hovering over tile at " + hitPosition);
            currentHoverTile = hitPosition;
            SetTileLayer(currentHoverTile, "Hover");
            return;
        }

        if (currentHoverTile != hitPosition)
        {
            SetTileLayer(currentHoverTile, "Letter");
            currentHoverTile = hitPosition;
            SetTileLayer(currentHoverTile, "Hover");
        }
    }

    private void HandleMouseInputOnTile(GameObject hitTile)
    {
        Vector2Int hitPosition = GetTileIndex(hitTile);
        if (hitPosition == -Vector2Int.one) return;

        // Left mouse button pressed: pick up a piece if it exists at that position
        if (Input.GetMouseButtonDown(0) && GetTile(hitPosition).layer == LayerMask.NameToLayer("Select"))
        {
            OnTileClicked.Invoke(hitPosition);

            if (tiles.TryGetValue(hitPosition, out GameObject tile))
            {
               

            }

        }
    }

    private void ResetHoverState()
    {
        if (currentHoverTile == -Vector2Int.one) return;

        currentHoverTile = -Vector2Int.one;
    }

    private void SetTileLayer(Vector2Int position, string layerName)
    {
        Debug.Log("Setting layer to " + layerName);
        if (tiles.TryGetValue(position, out GameObject tile))
        {
            tile.layer = LayerMask.NameToLayer(layerName);
        }
    }

    public GameObject GetTile(Vector2Int pos)
    {
        if (tiles.TryGetValue(pos, out GameObject tile))
            return tile;
        return null;
    }
}

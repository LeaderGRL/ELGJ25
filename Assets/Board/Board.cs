using System;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public Camera camera;

    [SerializeField] private int m_width;
    [SerializeField] private int m_height;
    [SerializeField] private List<GameObject> m_tilePrefabs;
    [SerializeField] private LetterTile m_letterTilePrefab;
    [SerializeField] private ShopTile m_shopTilePrefab;
    [SerializeField] private GameObject m_coinTilePrefab;
    [SerializeField] private GridGenerationData m_generationData;
    [SerializeField] private TMPro.TMP_InputField m_inputField;

    private Dictionary<Vector2, GameObject> m_tiles = new Dictionary<Vector2, GameObject>();
    private Vector2 m_currentHoverTile;
    private CrossWordsGameGrid_old _mCrossWordsGameGrid;
    private GridWord_old m_currentSelectedWord;
    private BoardInputHandler m_inputHandler;


    [Header("Events")]
    [HideInInspector] public UnityEvent<Vector2> OnTileClicked;
    public event Action<CrossWordsGameGrid_old> OnGenerateGrid;

    private float m_animationDelay = 0.01f;

    // Layers & Masks
    private int layerLetter, layerHover, layerSelect, layerValidate;
    private int maskLetterHoverSelect;



    private void Awake()
    {
        layerLetter = LayerMask.NameToLayer("Letter");
        layerHover = LayerMask.NameToLayer("Hover");
        layerSelect = LayerMask.NameToLayer("Select");
        layerValidate = LayerMask.NameToLayer("Validate");
        maskLetterHoverSelect = LayerMask.GetMask("Letter", "Hover", "Select");
    }

    private void Start()
    {
        //GenerateNewGrid();
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

        if (Physics.Raycast(ray, out hitInfo, 100, maskLetterHoverSelect))
        {
            HandleTileHover(hitInfo.transform.gameObject);
            //HandleMouseInputOnTile(hitInfo.transform.gameObject);
        }
    }

    public void GenerateNewGrid()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        _mCrossWordsGameGrid = CharacterPlacementGenerator.GenerateCharPlacements(
            m_generationData.Database,
            m_generationData.NumWordsToGenerate,
            ""
        );
    }

    public void ResetDoTweenDelay()
    {
        m_animationDelay = 0.0f;
    }

    public void SetGrid(CrossWordsGameGrid_old crossWordsGameGrid)
    {
        _mCrossWordsGameGrid = crossWordsGameGrid;
    }

    public void PlaceTile(Vector2 pos, Tile tile)
    {
        bool isExistingTile = m_tiles.ContainsKey(pos);
        if (isExistingTile)
        {
            m_tiles[pos].SetActive(false);
            Destroy(m_tiles[pos]);
        }

        tile.transform.localPosition = new Vector3(pos.x * 1.2f, -0.75f, pos.y * 1.2f);
        m_tiles[pos] = tile.gameObject;

        // Animation
        AnimateTileSpawn(tile.gameObject, m_animationDelay);
        m_animationDelay += 0.05f;
    }

    public void UpdateTileState(Vector2 position, TileState state)
    {
        if (m_tiles.TryGetValue(position, out var tile))
        {
            tile.layer = state switch
            {
                TileState.Validated => LayerMask.NameToLayer("Validate"),
                TileState.Selected => LayerMask.NameToLayer("Select"),
                _ => LayerMask.NameToLayer("Letter") // Default
            };
        }
    }

    private void AnimateTileSpawn(GameObject tile, float delay)
    {
        tile.transform.localScale = Vector3.zero;

        tile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(delay);
    }

    private Vector2 GetTileIndex(GameObject hitInfo)
    {
        foreach (var tile in m_tiles)
        {
            if (tile.Value == hitInfo)
                return tile.Key;
        }
        return -Vector2.one;
    }

    private void HandleTileHover(GameObject hitTile)
    {
        Vector2 hitPosition = GetTileIndex(hitTile);

        if (hitTile.layer != LayerMask.NameToLayer("Letter"))
        {
            return;
        }

        if (m_currentHoverTile != hitPosition)
        {
            string hitPositionLayer = GetTileLayer(hitPosition);
            string currentHoverTileLayer = GetTileLayer(m_currentHoverTile);

            if (currentHoverTileLayer == "Hover")
            {
                SetTileLayer(m_currentHoverTile, "Letter");
            }

            m_currentHoverTile = hitPosition;

            if (hitPositionLayer == "Letter")
            {
                SetTileLayer(m_currentHoverTile, "Hover");
            }
        }
    }

    public bool IsTileLocked(Vector2 tileLocation)
    {
        var words = _mCrossWordsGameGrid.GetAllWordAtLocation(tileLocation);
        foreach (var word in words)
        {
            if (word.IsValidated)
            {
                return true;
            }
        }
        return false;
    }

    public void CheckForCoinTile(CoinController coinController, Vector2 pos)
    {
        GetTile(pos).TryGetComponent(out CoinTile coinTile);
        if (coinTile)
        {
            Debug.Log("coin : " + coinTile.coinValue);
            coinController.AddCoins(coinTile.coinValue);
            Debug.Log("Coin tile clicked");
        }
    }


    //private void HandleMouseInputOnTile(GameObject hitTile)
    //{
    //    Vector2 hitPosition = GetTileIndex(hitTile);
    //    if (hitPosition == -Vector2.one) return;

    //    if (Input.GetMouseButtonDown(0) && GetTile(hitPosition).layer == LayerMask.NameToLayer("Letter"))
    //    {

    //    }
    //}

    private void SetTileLayer(Vector2 position, string layerName)
    {
        if (m_tiles.TryGetValue(position, out GameObject tile))
        {
            tile.layer = LayerMask.NameToLayer(layerName);
        }
    }

    private string GetTileLayer(Vector2 position)
    {
        if (m_tiles.TryGetValue(position, out GameObject tile))
        {
            return LayerMask.LayerToName(tile.layer);

        }

        return "";
    }

    public GameObject GetTile(Vector2 pos)
    {
        Debug.Log(m_tiles);
        if (m_tiles.TryGetValue(pos, out GameObject tile))
            return tile;
        return null;
    }

    public GridWord_old GetSelectedWord()
    {
        return m_currentSelectedWord;
    }

    public CrossWordsGameGrid_old GetWordGrid()
    {
        return _mCrossWordsGameGrid;
    }

    public Vector2 GetTilePosition(GameObject tile)
    {
        foreach (var tilePos in m_tiles)
        {
            if (tilePos.Value == tile)
            {
                return tilePos.Key;
            }
        }
        return -Vector2.one;
    }

    public BoardInputHandler GetInputHandler()
    {
        return m_inputHandler;
    }

    public void HideAllPopups()
    {
        foreach (var tilePositionObject in m_tiles)
        {
            var tile = tilePositionObject.Value.GetComponent<LetterTile>();
            if (tile == null)
            {
                continue;
            }
            tile.HidePopup();
        }

    }

    public void RevealLetter(char letter)
    {
        var affectedPositions = _mCrossWordsGameGrid.RevealLetterInAllWords(letter);

        foreach (Vector2 pos in affectedPositions)
        {
            UpdateTileVisual(pos);
            UpdateTileState(pos, TileState.Validated);

            if (m_tiles.TryGetValue(pos, out GameObject tile))
            {
                tile.layer = LayerMask.NameToLayer("Validate");
            }
        }
    }

    private void UpdateTileVisual(Vector2 position)
    {
        if (m_tiles.TryGetValue(position, out GameObject tileObj))
        {
            if (tileObj.TryGetComponent<LetterTile>(out LetterTile tile))
            {
                char currentChar = _mCrossWordsGameGrid.GetCurrentLetterAtPosition(position);
                tile.DisplayText.text = currentChar.ToString();
                tile.PlayJumpAnimation();
                UpdateTileState(position, TileState.Validated);
            }
        }
    }
}
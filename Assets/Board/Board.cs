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
    public CoinController coinController;

    [FormerlySerializedAs("width")]
    [SerializeField]
    private int m_width;
    [FormerlySerializedAs("height")]
    [SerializeField]
    private int m_height;
    [FormerlySerializedAs("tilePrefabs")]
    [SerializeField]
    private List<GameObject> m_tilePrefabs;
    [FormerlySerializedAs("letterTilePrefab")]
    [SerializeField]
    private LetterTile m_letterTilePrefab;
    [FormerlySerializedAs("shopTilePrefab")]
    [SerializeField]
    private ShopTile m_shopTilePrefab;
    [SerializeField] private GameObject m_coinTilePrefab;

    [FormerlySerializedAs("generationData")]
    [SerializeField]
    private GridGenerationData m_generationData;

    [FormerlySerializedAs("_inputField")]
    [SerializeField]
    private TMPro.TMP_InputField m_inputField;

    private static Board m_instance;
    private Dictionary<Vector2, GameObject> m_tiles;
    private Vector2 m_currentHoverTile;
    private CrossWordsGameGrid _mCrossWordsGameGrid;
    private GridWord m_currentSelectedWord;
    private BoardInputHandler m_inputHandler;


    private Sequence m_sequence;
    private float m_animationDelay = 0.01f;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Vector2> OnTileClicked;

    public event Action<CrossWordsGameGrid> OnGenerateGrid;



    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        m_tiles = new Dictionary<Vector2, GameObject>();
        GenerateNewGrid();
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

    private void InitializeLetterTile(LetterTile tile, Vector2 position)
    {
        tile.DisplayText.text = "";
        tile.OnTileClicked += HandleTileClick;
    }

    private void HandleTileClick(Tile tile)
    {
        var word = _mCrossWordsGameGrid.GetWordAtLocation(tile.Position);
        m_inputHandler.HandleWordSelection(word);
    }

    public void GenerateNewGrid()
    {
        CreateGrid();
        //SpawnAllTiles();
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

    public static Board GetInstance()
    {
        return m_instance;
    }

    public void SetGrid(CrossWordsGameGrid crossWordsGameGrid)
    {
        _mCrossWordsGameGrid = crossWordsGameGrid;
    }

    public void PlaceTileRefacto(Vector2 pos, Tile tile)
    {
        bool isExistingTile = m_tiles.ContainsKey(pos);
        if (isExistingTile)
        {
            m_tiles[pos].SetActive(false);
            Destroy(m_tiles[pos]);
        }

        tile.transform.position = new Vector3(pos.x * 1.2f, -0.75f, pos.y * 1.2f);
        m_tiles[pos] = tile.gameObject;

        // Animation
        AnimateTileSpawn(tile.gameObject, m_animationDelay);
        m_animationDelay += 0.05f;
    }

    public void SpawnTile(Vector2 position)
    {
        if (m_tiles.ContainsKey(position)) return;

        var tilePrefab = SelectTilePrefab(position);
        var newTile = Instantiate(tilePrefab, transform);

        newTile.transform.position = new Vector3(position.x * 1.2f, 0, position.y * 1.2f);
        m_tiles[position] = newTile;

        if (newTile.TryGetComponent<Tile>(out var basicTile))
        {
            basicTile.SpawnAnimation();
        }

        if (newTile.TryGetComponent<LetterTile>(out var letterTile))
        {
            InitializeLetterTile(letterTile, position);
            letterTile.SpawnAnimation();
        }

    }

    private GameObject SelectTilePrefab(Vector2 position)
    {
        if (_mCrossWordsGameGrid.GetWordsToGridValues().ContainsKey(position))
        {
            return Random.Range(0, 25) == 10
                ? m_coinTilePrefab.gameObject
                : m_letterTilePrefab.gameObject;
        }
        return m_tilePrefabs[Random.Range(0, m_tilePrefabs.Count)];
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

 
    //public void CheckForShopTile(Vector2Int pos)
    //{
    //    GetTile(pos).TryGetComponent(out ShopTile shopTile);
    //    if (shopTile)
    //    {
    //        ShopManager.Instance.OpenShop();
    //    }
    //}

    public void CheckForCoinTile(Vector2 pos)
    {
        GetTile(pos).TryGetComponent(out CoinTile coinTile);
        if (coinTile)
        {
            coinController.AddCoins(coinTile.coinValue);
            Debug.Log("Coin tile clicked");
        }
    }


    private void HandleMouseInputOnTile(GameObject hitTile)
    {
        Vector2 hitPosition = GetTileIndex(hitTile);
        if (hitPosition == -Vector2.one) return;

        if (Input.GetMouseButtonDown(0) && GetTile(hitPosition).layer == LayerMask.NameToLayer("Letter"))
        {

        }
    }

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
        if (m_tiles.TryGetValue(pos, out GameObject tile))
            return tile;
        return null;
    }

    public GridWord GetSelectedWord()
    {
        return m_currentSelectedWord;
    }

    public CrossWordsGameGrid GetWordGrid()
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
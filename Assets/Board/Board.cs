using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public Camera camera;

    [FormerlySerializedAs("width")] [SerializeField] 
    private int m_width;
    [FormerlySerializedAs("height")] [SerializeField] 
    private int m_height;
    [FormerlySerializedAs("tilePrefabs")] [SerializeField] 
    private List<GameObject> m_tilePrefabs;
    [FormerlySerializedAs("letterTilePrefab")] [SerializeField] 
    private LetterTile m_letterTilePrefab;
    [FormerlySerializedAs("shopTilePrefab")] [SerializeField]
    private ShopTile m_shopTilePrefab;

    [FormerlySerializedAs("generationData")] [SerializeField]
    private GridGenerationData m_generationData;

    [FormerlySerializedAs("_inputField")] [SerializeField] 
    private TMPro.TMP_InputField m_inputField;

    [SerializeField] 
    private Player m_player;
    
    private static Board m_instance;
    private Dictionary<Vector2Int, GameObject> m_tiles;
    private Vector2Int m_currentHoverTile;
    private Grid m_grid;
    private GridWord m_currentSelectedWord;
    

    private Sequence m_sequence;
    private float m_animationDelay = 0.01f;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Vector2Int> OnTileClicked;
    
    public event Action<Grid> OnGenerateGrid;



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
        m_tiles = new Dictionary<Vector2Int, GameObject>();
        return;
        m_grid = CharacterPlacementGenerator.GenerateCharPlacements(m_generationData.Database,
            m_generationData.NumWorToGenerate, "");
        Vector2Int gridSize = m_grid.GetGridSize();
        SpawnAllTiles();

        OnGenerateGrid?.Invoke(m_grid);
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

    private void SpawnAllTiles()
    {
        var minMaxPosGrid = m_grid.GetMinAndMaxPositionCharacterPlacement();
        for (int x = minMaxPosGrid.Key.x; x <= minMaxPosGrid.Value.x; x++)
        {
            for (int y = minMaxPosGrid.Key.y; y <= minMaxPosGrid.Value.y; y++)
            {
                PlaceTile(new Vector2Int(x, y));
            }
        }
    }

    public void ResetDoTweenDelay()
    {
        m_animationDelay = 0.0f;
    }

    public static Board GetInstance()
    {
        return m_instance;
    }

    public void SetGrid(Grid grid)
    {
        m_grid = grid;
    }

    public void PlaceTileRefacto(Vector2Int pos, Tile tile)
    {
        bool isExistingTile = m_tiles.ContainsKey(pos);
        if (isExistingTile)
        {
            m_tiles[pos].SetActive(false);
            Destroy(m_tiles[pos]);
        }
        
        tile.transform.position = new Vector3(pos.x, 0, pos.y);
        m_tiles[pos] = tile.gameObject;

        // Animation
        AnimateTileSpawn(tile.gameObject, m_animationDelay);
        m_animationDelay += 0.005f;
    }

    public void PlaceTile(Vector2Int pos)
    {
        bool isExistingTile = m_tiles.ContainsKey(pos);
        if (isExistingTile && m_tiles[pos].GetComponent<LetterTile>() != null)
        {
            return;
        }
        
        if (isExistingTile)
        {
            m_tiles[pos].SetActive(false);
        }

        GameObject newTile;

        if (m_grid.GetWordsToGridValues().ContainsKey(pos))
        {
            newTile = Instantiate(Random.Range(0, 100) == 50 ? m_shopTilePrefab.gameObject : m_letterTilePrefab.gameObject, transform); // Random shop tile spawn
            newTile.GetComponent<LetterTile>().DisplayText.text = "";
        }
        else
        {
            newTile = Instantiate(m_tilePrefabs[Random.Range(0, m_tilePrefabs.Count)], transform);
        }

        newTile.transform.position = new Vector3(pos.x, 0, pos.y);
        m_tiles[pos] = newTile;

        // Animation
        AnimateTileSpawn(newTile, m_animationDelay);
        m_animationDelay += 0.01f;
    }

    private void AnimateTileSpawn(GameObject tile, float delay)
    {
        tile.transform.localScale = Vector3.zero;
        
        // var position = tile.transform.position;
        // position = new Vector3(position.x,-1.1f, position.z);
        // tile.transform.position = position;
        // position = new Vector3(position.x,0.0f, position.z);
        //
        //tile.transform.DOMove(position, 0.5f).SetEase(Ease.Linear).SetDelay(delay);
        tile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(delay);
    }

    private Vector2Int GetTileIndex(GameObject hitInfo)
    {
        foreach (var tile in m_tiles)
        {
            if (tile.Value == hitInfo)
                return tile.Key;
        }
        return -Vector2Int.one;
    }

    private void HandleTileHover(GameObject hitTile)
    {
        Vector2Int hitPosition = GetTileIndex(hitTile);
        
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

    public void OnPlayerSelectTile(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        var tile =  m_tiles[m_currentHoverTile];
        var letterComponent = tile.GetComponent<LetterTile>();
        var word = m_grid.GetWordAtLocation(m_currentHoverTile);
        
        if ( m_currentSelectedWord != null && word != m_currentSelectedWord && word != null&& !m_currentSelectedWord.IsValidated)
        {
            foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
            {
                SetTileLayer(position, "Letter");
                foreach (var gridWord in m_grid.GetAllWordAtLocation(position))
                {
                    if (gridWord.IsValidated)
                    {
                        SetTileLayer(position, "Validate");
                    }
                }
            }
        }
        
        m_inputField.ActivateInputField();
        if (word != null && !word.IsValidated)
        {
            m_currentSelectedWord = word;
            string unlockedTilesToString = "";
            int numUnlockedTiles = 0;
            foreach (var letterLocation in m_currentSelectedWord.GetAllLetterCurrentWordPositions())
            {
                bool isTileLocked = IsTileLocked(letterLocation.Key);
                if (!isTileLocked)
                {
                    unlockedTilesToString += letterLocation.Value;
                    numUnlockedTiles++;
                }
            }
            m_inputField.text = unlockedTilesToString;
            m_inputField.characterLimit = numUnlockedTiles;
            
            foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
            {
                SetTileLayer(position, "Select");
                foreach (var gridWord in m_grid.GetAllWordAtLocation(position))
                {
                    if (gridWord.IsValidated)
                    {
                        SetTileLayer(position, "Validate");
                    }
                }
            }
            Debug.Log($"Word selected: {m_currentSelectedWord.SolutionWord}");
        }
        m_inputField.caretPosition = m_inputField.text.Length ;
    }

    private bool IsTileLocked(Vector2Int tileLocation)
    {
        var words = m_grid.GetAllWordAtLocation(tileLocation);
        foreach (var word in words)
        {
            if (word.IsValidated)
            {
                return true;
            }
        }
        return false;
    }

    public void SetCurrentWordText(string text)
    {
        if (m_currentSelectedWord == null)
        {
            return;
        }
        text = text.ToUpper();
        Dictionary<Vector2Int, char> currentwordLettersLocations = m_currentSelectedWord.GetAllLetterCurrentWordPositions();
        Dictionary<Vector2Int, char> lockedTiles = new ();
        Dictionary<Vector2Int, char> unlockedTiles = new ();
        string unlockedTilesToString = "";

        foreach (var letterLocation in currentwordLettersLocations)
        {
            bool isTileLocked = IsTileLocked(letterLocation.Key);
            if (isTileLocked)
            {
                unlockedTilesToString += letterLocation.Value;
            }
            (isTileLocked ? lockedTiles : unlockedTiles)[letterLocation.Key] = letterLocation.Value;
        }

        if (unlockedTilesToString == text)
        {
            return;
        }

        if (text.Length > unlockedTiles.Count)
        {
            m_inputField.text = unlockedTilesToString;
            return;
        }

        int i = 0;
        Dictionary<Vector2Int, char> tempDictionary = new();
        foreach (var letterLocation in unlockedTiles)
        {
            if (i < text.Length)
            {
                tempDictionary[letterLocation.Key] = text[i];
            }
            else
            {
                tempDictionary[letterLocation.Key] = '\0';
            }
            i++;
        }

        foreach (var letterLocation in tempDictionary)
        {
            currentwordLettersLocations[letterLocation.Key] = letterLocation.Value;
        }

        foreach (var letterLocation in currentwordLettersLocations)
        {
            LetterTile letterTile = GetTile(letterLocation.Key).GetComponent<LetterTile>();
            string newLetter = letterLocation.Value.ToString();

            if (letterTile.DisplayText.text != newLetter)
            {
                letterTile.DisplayText.text = newLetter;
                letterTile.PlayJumpAnimation();
            }
        }

    }
    

    public void ValidateText(string text)
    {
        text = text.ToUpper();
        string wordToString = m_currentSelectedWord.GetCurrentWordToString();
        if (wordToString.Length < m_currentSelectedWord.SolutionWord.Length)
        {
            return;
        }

        if (wordToString != m_currentSelectedWord.SolutionWord)
        {
            // Failed
            // Lose life?
            m_player.TakeDamage(10);
            return;
        }

        if (wordToString == m_currentSelectedWord.SolutionWord)
        {
            // Yeees
            m_player.AddScore(m_currentSelectedWord.Difficulty * 10);
            var letters = m_currentSelectedWord.GetAllLetterSolutionPositions();
            foreach (var letter in letters)
            {
                GetTile(letter.Key).layer = LayerMask.NameToLayer("Validate");
                var wordsAtLocaiton = m_grid.GetAllWordAtLocation(letter.Key);
                foreach (var word in wordsAtLocaiton)
                {
                    word.SetLetterAtLocation(letter.Key, letter.Value);
                }

                m_player.AddScore(LetterWeight.GetLetterWeight(letter.Value));
                CheckForShopTile(letter.Key);
            }
            m_currentSelectedWord.Validate();
            m_currentSelectedWord = null;
        }
    }

    private void CheckForShopTile(Vector2Int pos)
    {
        GetTile(pos).TryGetComponent(out ShopTile shopTile);
        if (shopTile)
        {
            ShopManager.Instance.OpenShop();
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

            if (m_tiles.TryGetValue(hitPosition, out GameObject tile))
            {
               

            }

        }
    }
    
    

    private void ResetHoverState()
    {
        if (m_currentHoverTile == -Vector2Int.one) return;

        m_currentHoverTile = -Vector2Int.one;
    }

    private void SetTileLayer(Vector2Int position, string layerName)
    {
        if (m_tiles.TryGetValue(position, out GameObject tile))
        {
            tile.layer = LayerMask.NameToLayer(layerName);
        }
    }

    private string GetTileLayer(Vector2Int position)
    {
        if (m_tiles.TryGetValue(position, out GameObject tile))
        {
            return LayerMask.LayerToName(tile.layer);

        }

        return "";
    }

    public GameObject GetTile(Vector2Int pos)
    {
        if (m_tiles.TryGetValue(pos, out GameObject tile))
            return tile;
        return null;
    }
}

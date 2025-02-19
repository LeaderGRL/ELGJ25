using System;
using DG.Tweening;
using System.Collections.Generic;
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
    
    private Board m_instance;
    private Dictionary<Vector2Int, GameObject> m_tiles;
    private Vector2Int m_currentHoverTile;
    private Grid m_grid;
    private Grid.GridWord m_currentSelectedWord;
    

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
        m_sequence = DOTween.Sequence();
        m_tiles = new Dictionary<Vector2Int, GameObject>();
        m_grid = CharacterPlacementGenerator.GenerateCharPlacements(m_generationData.Database,
            m_generationData.NumWorToGenerate, "");
        Vector2Int gridSize = m_grid.GetGridSize();
        var minMaxPosGrid = m_grid.GetMinAndMaxPositionCharacterPlacement();
        for (int x =  minMaxPosGrid.Key.x; x <= minMaxPosGrid.Value.x; x++)
        {
            for (int y =  minMaxPosGrid.Key.y; y <= minMaxPosGrid.Value.y; y++)
            {
                PlaceTile(new Vector2Int(x, y));
            }
        }
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

    public Board GetInstance()
    {
        return m_instance;
    }

    public void PlaceTile(Vector2Int pos)
    {
        if (m_tiles.ContainsKey(pos))
        {
            return;
        }


        if (m_grid.CharacterPlacements.ContainsKey(pos))
        {
            int randomValue = Random.Range(0, 100);
            var letterTile = Instantiate(randomValue == 50 ? m_shopTilePrefab : m_letterTilePrefab, transform);
            letterTile.transform.position = new Vector3(pos.x, 0, pos.y);
            letterTile.DisplayText.text = "";
            m_tiles.Add(pos, letterTile.gameObject);

            // Animation
            letterTile.transform.localScale = Vector3.zero;

            letterTile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(m_animationDelay);
            m_animationDelay += 0.01f;
            //letterTile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);


            return;
        }

        var tile = Instantiate(m_tilePrefabs[Random.Range(0, m_tilePrefabs.Count)], transform);
        tile.transform.position = new Vector3(pos.x, 0, pos.y);
        m_tiles.Add(pos, tile);

        // Animation
        tile.transform.localScale = Vector3.zero;
        tile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(m_animationDelay);
        m_animationDelay += 0.01f;

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
        var tile =  m_tiles[m_currentHoverTile];
        var letterComponent = tile.GetComponent<LetterTile>();
        var word = m_grid.GetWordAtLocation(m_currentHoverTile);
        
        if ( m_currentSelectedWord != null && word != m_currentSelectedWord && word != null&& !m_currentSelectedWord.IsLocked)
        {
            foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
            {
                SetTileLayer(position, "Letter");
                foreach (var gridWord in m_grid.GetAllWordAtLocation(position))
                {
                    if (gridWord.IsLocked)
                    {
                        SetTileLayer(position, "Validate");
                    }
                }
            }
        }
        
        m_inputField.ActivateInputField();
        if (word != null && !word.IsLocked)
        {
            m_currentSelectedWord = word;
            m_inputField.text = m_currentSelectedWord.GetCurrentWord();
            foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
            {
                SetTileLayer(position, "Select");
                foreach (var gridWord in m_grid.GetAllWordAtLocation(position))
                {
                    if (gridWord.IsLocked)
                    {
                        SetTileLayer(position, "Validate");
                    }
                }
            }
            Debug.Log($"Word selected: {m_currentSelectedWord.SolutionWord}");
        }
        m_inputField.caretPosition = m_inputField.text.Length ;
    }

    public void SetCurrentWordText(string text)
    {
        text = text.ToUpper();
        if (m_currentSelectedWord == null)
        {
            return;
        }

        if (m_inputField.text == m_currentSelectedWord.GetCurrentWord())
        {
            return;
        }
        string startWord = m_currentSelectedWord.GetCurrentWord();
        if (!m_currentSelectedWord.TrySetCurrentWord(text))
        {
            m_inputField.text = startWord;
        }
        else
        {

             var letterLocations = m_currentSelectedWord.GetAllLetterCurrentWordPositions();
             foreach (var letterLocation in letterLocations)
             {
                 LetterTile letterTile =  GetTile(letterLocation.Key).GetComponent<LetterTile>();
                 var words = m_grid.GetAllWordAtLocation(letterLocation.Key);
                 if (words != null && words.Count > 1)
                 {
                     var otherWord = words.Find(word => word != m_currentSelectedWord);
                     char otherWordLetterAtLocation = otherWord.GetCurrentLetterAtLocation(letterLocation.Key);
                     if (otherWord.IsLocked)
                     {
                         Debug.Log("If word is locked");

                         m_currentSelectedWord.SetLetterAtLocation(letterLocation.Key, otherWordLetterAtLocation);
                         letterTile.DisplayText.text = otherWordLetterAtLocation.ToString();
                         continue;
                     }
                     if (letterLocation.Value == '\0')
                     {
                         Debug.Log("letter location equal 0");

                         letterTile.DisplayText.text = letterLocation.Value.ToString();
                         letterTile.DisplayText.text = otherWordLetterAtLocation.ToString();
                         continue;
                     }
                     else if (letterLocation.Value != otherWordLetterAtLocation && otherWordLetterAtLocation != '\0')
                     {
                         Debug.Log("letter location value different otherwordletteratlocation");

                        otherWord.SetLetterAtLocation(letterLocation.Key, letterLocation.Value);
                     }
                 }
                 
                 letterTile.DisplayText.text = letterLocation.Value.ToString();
                 
             }

             // if (m_inputField.text != m_currentSelectedWord.GetCurrentWord())
             // {
             //    m_inputField.text = m_currentSelectedWord.GetCurrentWord();
             // }
        }

    }

    public void ValidateText(string text)
    {
        text = text.ToUpper();
        if (text.Length < m_currentSelectedWord.SolutionWord.Length)
        {
            return;
        }

        if (text != m_currentSelectedWord.SolutionWord)
        {
            // Failed
            // Lose life?
            m_player.TakeDamage(10);
            return;
        }

        if (text == m_currentSelectedWord.SolutionWord)
        {
            // Yeees
            m_player.AddScore(m_currentSelectedWord.Difficulty);
            m_currentSelectedWord.IsLocked = true;
            var letters = m_currentSelectedWord.GetAllLetterSolutionPositions();
            foreach (var letter in letters)
            {
                GetTile(letter.Key).layer = LayerMask.NameToLayer("Validate");
                CheckForShopTile(letter.Key);
            }
            
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

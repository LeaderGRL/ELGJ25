using DG.Tweening;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Board : MonoBehaviour
{
    public Camera camera;

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private List<GameObject> tilePrefabs;
    [SerializeField] private LetterTile letterTilePrefab;
    
    [SerializeField]
    private GridGenerationData generationData;

    [SerializeField] 
    private TMPro.TMP_InputField _inputField;

    private Board Instance;
    private Dictionary<Vector2Int, GameObject> tiles;
    private Vector2Int currentHoverTile;
    private Grid m_grid;
    private Grid.GridWord m_currentSelectedWord;
    

    private Sequence sequence;
    private float animationDelay = 0.01f;

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
        sequence = DOTween.Sequence();
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
            letterTile.DisplayText.text = "";
            tiles.Add(pos, letterTile.gameObject);

            // Animation
            letterTile.transform.localScale = Vector3.zero;

            letterTile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(animationDelay);
            animationDelay += 0.01f;
            //letterTile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);


            return;
        }

        var tile = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Count)], transform);
        tile.transform.position = new Vector3(pos.x, 0, pos.y);
        tiles.Add(pos, tile);

        // Animation
        tile.transform.localScale = Vector3.zero;
        tile.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(animationDelay);
        animationDelay += 0.01f;

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
        
        if (hitTile.layer != LayerMask.NameToLayer("Letter"))
        {
            return;
        }
        
        if (currentHoverTile != hitPosition)
        {
            string hitPositionLayer = GetTileLayer(hitPosition);
            string currentHoverTileLayer = GetTileLayer(currentHoverTile);
            if (currentHoverTileLayer == "Hover")
            {
                SetTileLayer(currentHoverTile, "Letter");
            }
            currentHoverTile = hitPosition;
            if (hitPositionLayer == "Letter")
            {
                SetTileLayer(currentHoverTile, "Hover");
            }
        }
    }

    public void OnPlayerSelectTile(InputAction.CallbackContext context)
    {
        Debug.Log("Select tile");
        var tile =  tiles[currentHoverTile];
        var letterComponent = tile.GetComponent<LetterTile>();
        var word = m_grid.GetWordAtLocation(currentHoverTile);
        
        if ( m_currentSelectedWord != null && word != m_currentSelectedWord && word != null)
        {
            foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
            {
                SetTileLayer(position, "Letter");
            }
        }


        _inputField.Select();
        
        if (word != null)
        {
            m_currentSelectedWord = word;
            _inputField.text = m_currentSelectedWord.GetCurrentWord();
            foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
            {
                SetTileLayer(position, "Select");
            }
        }
        _inputField.caretPosition = _inputField.text.Length ;
        //_inputField.
    }

    public void SetCurrentWordText(string text)
    {
        Debug.Log($"Value Changed: {text}");
        if (m_currentSelectedWord == null)
        {
            return;
        }
        string startWord = m_currentSelectedWord.GetCurrentWord();
        if (!m_currentSelectedWord.TrySetCurrentWord(text))
        {
            _inputField.text = startWord;
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
                     if (letterLocation.Value == '\0')
                     {
                         letterTile.DisplayText.text = letterLocation.Value.ToString();
                         letterTile.DisplayText.text = otherWordLetterAtLocation.ToString();
                         continue;
                     }
                     else if (letterLocation.Value != otherWordLetterAtLocation && otherWordLetterAtLocation != '\0')
                     {
                        otherWord.SetLetterAtLocation(letterLocation.Key, letterLocation.Value);
                     }
                 }
                 
                 letterTile.DisplayText.text = letterLocation.Value.ToString();
             }
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
        if (tiles.TryGetValue(position, out GameObject tile))
        {
            tile.layer = LayerMask.NameToLayer(layerName);
        }
    }

    private string GetTileLayer(Vector2Int position)
    {
        if (tiles.TryGetValue(position, out GameObject tile))
        {
            return LayerMask.LayerToName(tile.layer);

        }

        return "";
    }

    public GameObject GetTile(Vector2Int pos)
    {
        if (tiles.TryGetValue(pos, out GameObject tile))
            return tile;
        return null;
    }
}

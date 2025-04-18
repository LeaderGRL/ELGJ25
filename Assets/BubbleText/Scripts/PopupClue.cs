using TMPro;
using UnityEngine;

public class PopupClue : MonoBehaviour
{
    public GameObject popup;
    public TextMeshProUGUI text;
    public Board board;

    private void OnEnable()
    {
        //Board.GetInstance().OnTileClicked.AddListener(OnTileClicked);
    }

    private void OnDisable()
    {
        //Board.GetInstance().OnTileClicked.RemoveListener(OnTileClicked);
    }

    private void OnTileClicked(Vector2 tilePos)
    {
        Debug.Log("Tile clicked at " + tilePos);
        popup.SetActive(true);
        //text.text = Board.GetInstance().GetWordGrid().GetClue(tilePos);
    }
}

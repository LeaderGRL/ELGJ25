using TMPro;
using UnityEngine;

public class PopupClue : MonoBehaviour
{
    public GameObject popup;
    public TextMeshProUGUI text;

    private void OnEnable()
    {
        Board.GetInstance().OnTileClicked.AddListener(OnTileClicked);
    }

    private void OnDisable()
    {
        Board.GetInstance().OnTileClicked.RemoveListener(OnTileClicked);
    }

    private void OnTileClicked(Vector2Int tilePos)
    {
        Debug.Log("Tile clicked at " + tilePos);
        popup.SetActive(true);
        text.text = Board.GetInstance().GetWordGrid().GetClue(tilePos);
    }
}

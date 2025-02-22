using DG.Tweening;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "LetterTile", menuName = "Tiles/LetterTile")]
public class LetterTileObject : ScriptableObject
{
    public char letter;
    public Sprite sprite;
}

public class LetterTile : Tile, IPointerClickHandler
{
    [SerializeField] LetterTileObject letterTileObject;

    [field: SerializeField]
    public TextMeshProUGUI DisplayText { get; private set; }

    [Header("Animation Settings")]
    public float jumpPower = 1;
    public float jumpDuration = 0.2f;

    [Header("Popup")]
    public GameObject popup;
    public TextMeshProUGUI clueText;

    private void Start()
    {
        DOTween.defaultAutoKill = false; // Prevents animations from being killed when the object is destroyed
    }

    public void SetPopupPosAndRotByIsRow(bool isRow)
    {
        Vector3 position = popup.transform.position;
        Vector3 rotationEuler = popup.transform.rotation.eulerAngles;
        if (isRow)
        {
            position.x = 0;
            position.z = 0.5f;
            rotationEuler.y = 0;
        }
        else
        {
            position.x = 0.5f;
            position.z = 0f;
            rotationEuler.y = 90;
        }
        popup.transform.position = position;
        popup.transform.rotation = Quaternion.Euler( rotationEuler);
    }

    public void PlayJumpAnimation()
    {
        // Create a DOTween sequence for the jump.
        Sequence jumpSequence = DOTween.Sequence();

        // Upward movement: blendably move up by 'jumpPower' over half the duration.
        jumpSequence.Append(
            transform.DOBlendableMoveBy(new Vector3(0, jumpPower, 0), jumpDuration / 2)
                     .SetEase(Ease.OutQuad)
        );

        // Downward movement: blendably move down by 'jumpPower' over the remaining duration.
        jumpSequence.Append(
            transform.DOBlendableMoveBy(new Vector3(0, -jumpPower, 0), jumpDuration / 2)
                     .SetEase(Ease.InQuad)
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Tile clicked at " + this.gameObject.name);
        Vector2Int tilePos = Board.GetInstance().GetTilePosition(this.gameObject);
        popup.SetActive(true);
        clueText.text = Board.GetInstance().GetWordGrid().GetClue(tilePos);
    }

    public void ManagePopup()
    {
        Board.GetInstance().HideAllPopups();
        Vector2Int tilePos = Board.GetInstance().GetTilePosition(this.gameObject);
        popup.SetActive(true);
        clueText.text = Board.GetInstance().GetWordGrid().GetClue(tilePos);
    }

    public void HidePopup()
    {
        popup.SetActive(false);
    }
}


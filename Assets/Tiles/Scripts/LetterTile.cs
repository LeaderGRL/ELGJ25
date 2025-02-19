using DG.Tweening;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LetterTile", menuName = "Tiles/LetterTile")]
public class LetterTileObject : ScriptableObject
{
    public char letter;
    public Sprite sprite;
}

public class LetterTile : Tile
{
    [SerializeField] LetterTileObject letterTileObject;

    [field: SerializeField]
    public TextMeshProUGUI DisplayText { get; private set; }
    public float jumpPower = 1;
    public float jumpDuration = 0.2f;

    private void Start()
    {
        DOTween.defaultAutoKill = false; // Prevents animations from being killed when the object is destroyed
    }
    //public void PlayJumpAnimation()
    //{
    //    transform.DOJump(transform.position, jumpPower, 1, jumpDuration).SetEase(Ease.OutQuad);
    //}

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


    public void PlayPunchAnimation()
    {
        transform.DOPunchScale(new Vector3(0, 1f, 0), jumpDuration, 1, 1f).SetEase(Ease.OutQuad);
    }
}


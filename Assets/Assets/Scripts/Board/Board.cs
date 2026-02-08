using DG.Tweening;
using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// Manage the board and tile placement
    /// </summary>
    public class Board: MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _tileContainer;

        [Header("Grid Settings")]
        [SerializeField] private float _tileSpacing = 1.2f;
        [SerializeField] private Vector3 _boardOffset = Vector3.zero;

        [Header("Animation")]
        [SerializeField] private float _spawnAnimationDuration = 0.5f;
        [SerializeField] private float _spawnAnimationDelay = 0.05f;
        [SerializeField] private Ease _spawnEase = Ease.OutBack;

        [Header("Layer")]
        [SerializeField] private string _letterLayerName = "Letter";
        [SerializeField] private string _hoverLayerName = "Hover";
        [SerializeField] private string _selectLayerName = "select";
        [SerializeField] private string _validateLayerName = "Validate";

    }
}

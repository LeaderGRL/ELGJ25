using UnityEngine;
using Crossatro.Board;

namespace Crossatro.UI
{
    public class ClueController: MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardInputHandler _boardInputHandler;
        [SerializeField] private ClueView _clueView;

        private void OnEnable()
        {
            _boardInputHandler.OnTileClicked += OnTileClickedEvent;
        }

        private void OnDisable()
        {
            _boardInputHandler.OnTileClicked -= OnTileClickedEvent;
        }

        private void OnTileClickedEvent(Vector2 tilePosition)
        {
            Vector3 cluePosition = new Vector3(tilePosition.x, 6, tilePosition.y);
            _clueView.UpdateCluePosition(cluePosition);
        }
    }
}
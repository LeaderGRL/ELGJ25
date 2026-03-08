using TMPro;
using UnityEngine;

namespace Crossatro.UI
{
    public class ClueView: MonoBehaviour
    {
        [SerializeField] private GameObject _clueObject;
        [SerializeField] private TextMeshProUGUI _clueText;

        public void SetClueVisibility(bool visibility)
        {
            if (_clueObject == null)
            {
                Debug.LogWarning("[ClueView] clue object ui not assigned!");
                return;
            }

            _clueObject.SetActive(visibility);
        }

        public void UpdateCluePosition(Vector3 position)
        {
            if (_clueObject == null)
            {
                Debug.LogWarning("[ClueView] clue object ui not assigned!");
                return;
            }

            _clueObject.transform.position = position;
        }

        public void UpdateClueText(string clue)
        {
            if (_clueObject == null)
            {
                Debug.LogWarning("[ClueView] Clue object ui not assigned!");
                return;
            }

            if (_clueText == null)
            {
                Debug.LogWarning("[ClueView] Clue text ui not assigned!");
                return;
            }

            _clueText.text = clue;
        }
        
    }
}

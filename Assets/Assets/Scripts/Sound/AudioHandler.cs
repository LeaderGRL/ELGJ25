using UnityEngine;
using Crossatro.Board;
using Crossatro.Events;
namespace Crossatro.Sound
{
    /// <summary>
    /// Audio reaction from game events.
    /// </summary>
    public class AudioHandler: MonoBehaviour
    {
        [Header("Pop tile effect")]
        [SerializeField] private AudioClip _letterTypedSfx;
        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        [Header("Game Music")]
        [SerializeField] private AudioClip _gameMusic;

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<LetterTypedEvent>(OnLetterTyped);
            EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStart);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<LetterTypedEvent>(OnLetterTyped);
        }

        private void OnLetterTyped(LetterTypedEvent e)
        {
            if (_letterTypedSfx == null) return;

            SoundManager.Instance.PlaySfx(_letterTypedSfx, _minPitch, _maxPitch);
        }

        private void OnGameStart(GameStartedEvent e)
        {
            if (_gameMusic == null) return;

            SoundManager.Instance.PlayMusic(_gameMusic, 0.2f);
        }
    }
}
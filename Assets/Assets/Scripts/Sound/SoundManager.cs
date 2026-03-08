using UnityEngine;

namespace Crossatro.Sound
{
    /// <summary>
    /// Central audio manager.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        // ============================================================
        // Singleton
        // ============================================================

        public static SoundManager Instance { get; private set; }

        // ============================================================
        // Configuration
        // ============================================================

        [Header("Music")]
        [SerializeField] private AudioSource _musicSource;

        [Header("SFX")]
        [Tooltip("Number of pooled SFX sources (handles overlapping sounds)")]
        [SerializeField] private int _sfxPoolSize = 8;

        [Tooltip("Master SFX volume")]
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 1f;

        // ============================================================
        // SFX Pool
        // ============================================================

        private AudioSource[] _sfxSources;
        private int _nextSfxIndex;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializeSfxPool();
            InitializeMusicSource();
        }

        // ============================================================
        // Initialization
        // ============================================================

        private void InitializeSfxPool()
        {
            _sfxSources = new AudioSource[_sfxPoolSize];

            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_Source_{i}");
                go.transform.SetParent(transform);

                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f; // 2D sound
                source.volume = _sfxVolume;

                _sfxSources[i] = source;
            }
        }

        private void InitializeMusicSource()
        {
            if (_musicSource != null) return;

            // Auto-create if not assigned
            var go = new GameObject("Music_Source");
            go.transform.SetParent(transform);

            _musicSource = go.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;
        }

        // ============================================================
        // SFX API
        // ============================================================

        /// <summary>
        /// Play a sound effect at normal pitch.
        /// </summary>
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;

            var source = GetNextSfxSource();
            source.pitch = 1f;
            source.volume = _sfxVolume;
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Play a sound effect with a random pitch between min and max.
        /// Each call uses its own AudioSource, so overlapping sounds
        /// won't interfere with each other's pitch.
        /// </summary>
        public void PlaySfx(AudioClip clip, float minPitch, float maxPitch)
        {
            if (clip == null) return;

            var source = GetNextSfxSource();
            source.pitch = Random.Range(minPitch, maxPitch);
            source.volume = _sfxVolume;
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Set master SFX volume.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            foreach (var source in _sfxSources)
                source.volume = _sfxVolume;
        }

        // ============================================================
        // Music API
        // ============================================================

        /// <summary>
        /// Play background music.
        /// </summary>
        public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (_musicSource == null || clip == null) return;

            _musicSource.clip = clip;
            _musicSource.volume = volume;
            _musicSource.loop = loop;
            _musicSource.pitch = 1f;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (_musicSource != null) _musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (_musicSource != null) _musicSource.UnPause();
        }

        public void SetMusicVolume(float volume)
        {
            if (_musicSource != null)
                _musicSource.volume = Mathf.Clamp01(volume);
        }

        // ============================================================
        // Pool Management
        // ============================================================

        /// <summary>
        /// Round-robin through the SFX source pool.
        /// If a source is still playing, it gets reused.
        /// </summary>
        private AudioSource GetNextSfxSource()
        {
            var source = _sfxSources[_nextSfxIndex];
            _nextSfxIndex = (_nextSfxIndex + 1) % _sfxSources.Length;
            return source;
        }
    }
}
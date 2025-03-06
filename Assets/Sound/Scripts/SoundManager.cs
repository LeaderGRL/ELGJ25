using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;

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
    }

    public void PlaySfx(AudioClip audioClip)
    {
        ResetPitch();
        musicSource.PlayOneShot(audioClip);
    }

    public void PlaySfxWithRandomPitch(AudioClip audioClip, float min, float max)
    {
        musicSource.pitch = Random.Range(min, max);
        musicSource.PlayOneShot(audioClip);
    }

    public void ResetPitch()
    {
        musicSource.pitch = 1;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void UnPauseMusic()
    {
        musicSource.UnPause();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetMusicPitch(float pitch)
    {
        musicSource.pitch = pitch;
    }

    public void SetMusicLoop(bool loop)
    {
        musicSource.loop = loop;
    }

    public void SetRandomPitch(float min, float max)
    {
        musicSource.pitch = Random.Range(min, max);
    }

}

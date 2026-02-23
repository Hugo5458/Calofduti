using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AudioManager simple sin dependencias externas
/// </summary>
public class SimpleAudioManager : MonoBehaviour
{
    public static SimpleAudioManager Instance;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;
    
    [Header("Sonidos UI")]
    public AudioClip buttonClickSound;
    public AudioClip pauseSound;
    public AudioClip unpauseSound;
    
    [Header("Sonidos de Juego")]
    public AudioClip[] footstepSounds;
    public AudioClip[] gunshotSounds;
    public AudioClip[] reloadSounds;
    public AudioClip[] hitSounds;
    
    // Configuración de volumen
    private float masterVolume = 1f;
    private float musicVolume = 0.7f;
    private float sfxVolume = 0.8f;
    private float uiVolume = 0.9f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAudioManager()
    {
        // Crear AudioSources si no existen
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.SetParent(transform);
            uiSource = uiSource.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
        }
        
        LoadSettings();
        ApplyVolumes();
    }
    
    void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 0.9f);
    }
    
    void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;
            
        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;
            
        if (uiSource != null)
            uiSource.volume = masterVolume * uiVolume;
    }
    
    #region Control de Volumen
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }
    
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Reproducción de Sonidos
    
    public void PlayMusic(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    
    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && uiSource != null)
        {
            uiSource.PlayOneShot(clip, volume);
        }
    }
    
    #endregion
    
    #region Métodos de Conveniencia
    
    public void PlayButtonClick()
    {
        PlayUISound(buttonClickSound);
    }
    
    public void PlayPauseSound()
    {
        PlayUISound(pauseSound);
    }
    
    public void PlayUnpauseSound()
    {
        PlayUISound(unpauseSound);
    }
    
    public void PlayRandomFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip randomStep = footstepSounds[Random.Range(0, footstepSounds.Length)];
            PlaySFX(randomStep, 0.5f);
        }
    }
    
    public void PlayRandomGunshot()
    {
        if (gunshotSounds != null && gunshotSounds.Length > 0)
        {
            AudioClip randomShot = gunshotSounds[Random.Range(0, gunshotSounds.Length)];
            PlaySFX(randomShot, 0.8f);
        }
    }
    
    public void PlayRandomReload()
    {
        if (reloadSounds != null && reloadSounds.Length > 0)
        {
            AudioClip randomReload = reloadSounds[Random.Range(0, reloadSounds.Length)];
            PlaySFX(randomReload, 0.7f);
        }
    }
    
    public void PlayRandomHit()
    {
        if (hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip randomHit = hitSounds[Random.Range(0, hitSounds.Length)];
            PlaySFX(randomHit, 0.9f);
        }
    }
    
    #endregion
    
    #region Métodos de Obtención
    
    public float GetMasterVolume()
    {
        return masterVolume;
    }
    
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    
    public float GetUIVolume()
    {
        return uiVolume;
    }
    
    #endregion
    
    public void StopAllAudio()
    {
        if (musicSource != null)
            musicSource.Stop();
        if (sfxSource != null)
            sfxSource.Stop();
        if (uiSource != null)
            uiSource.Stop();
    }
}

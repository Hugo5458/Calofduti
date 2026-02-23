using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Sistema de audio mejorado con control de volumen por categorías.
/// Maneja música, efectos de sonido y voz por separado.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Mixer de Audio")]
    public AudioMixer audioMixer;
    
    [Header("Canales de Audio")]
    public SoundGroup musicSounds;
    public SoundGroup sfxSounds;
    public SoundGroup voiceSounds;
    public SoundGroup uiSounds;
    
    [Header("Sonidos UI")]
    public AudioClip buttonClickSound;
    public AudioClip hoverSound;
    public AudioClip pauseSound;
    public AudioClip unpauseSound;
    public AudioClip notificationSound;
    
    [Header("Sonidos de Juego")]
    public AudioClip[] footstepSounds;
    public AudioClip[] gunshotSounds;
    public AudioClip[] reloadSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] zombieSounds;
    
    // Parámetros del mixer
    private const string MASTER_VOLUME_PARAM = "MasterVolume";
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";
    private const string VOICE_VOLUME_PARAM = "VoiceVolume";
    private const string UI_VOLUME_PARAM = "UIVolume";
    
    // PlayerPrefs keys
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string VOICE_VOLUME_KEY = "VoiceVolume";
    private const string UI_VOLUME_KEY = "UIVolume";
    private const string MUTE_ALL_KEY = "MuteAll";
    
    // Fuentes de audio por categoría
    private Dictionary<string, List<AudioSource>> audioSources = new Dictionary<string, List<AudioSource>>();
    
    // Singleton
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
        // Inicializar diccionario de fuentes de audio
        audioSources["Music"] = new List<AudioSource>();
        audioSources["SFX"] = new List<AudioSource>();
        audioSources["Voice"] = new List<AudioSource>();
        audioSources["UI"] = new List<AudioSource>();
        
        // Cargar configuración guardada
        LoadAudioSettings();
        
        // Crear fuentes de audio iniciales
        CreateInitialAudioSources();
    }
    
    void CreateInitialAudioSources()
    {
        // Crear algunas fuentes de audio para cada categoría
        for (int i = 0; i < 3; i++)
        {
            CreateAudioSource("Music");
            CreateAudioSource("SFX");
            CreateAudioSource("Voice");
            CreateAudioSource("UI");
        }
    }
    
    AudioSource CreateAudioSource(string category)
    {
        GameObject audioSourceObj = new GameObject($"{category}_AudioSource_{audioSources[category].Count}");
        audioSourceObj.transform.SetParent(transform);
        
        AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
        
        // Configurar según la categoría
        switch (category)
        {
            case "Music":
                audioSource.loop = true;
                audioSource.playOnAwake = false;
                break;
            case "SFX":
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                break;
            case "Voice":
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                break;
            case "UI":
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                break;
        }
        
        audioSources[category].Add(audioSource);
        return audioSource;
    }
    
    #region Control de Volumen
    
    void LoadAudioSettings()
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioMixer no asignado en AudioManager");
            return;
        }
        
        // Cargar volúmenes guardados
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0.8f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
        float voiceVol = PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 1f);
        float uiVol = PlayerPrefs.GetFloat(UI_VOLUME_KEY, 0.9f);
        bool isMuted = PlayerPrefs.GetInt(MUTE_ALL_KEY, 0) == 1;
        
        // Aplicar volúmenes al mixer
        SetMixerVolume(MASTER_VOLUME_PARAM, masterVol);
        SetMixerVolume(MUSIC_VOLUME_PARAM, musicVol);
        SetMixerVolume(SFX_VOLUME_PARAM, sfxVol);
        SetMixerVolume(VOICE_VOLUME_PARAM, voiceVol);
        SetMixerVolume(UI_VOLUME_PARAM, uiVol);
        
        // Aplicar mute global
        if (isMuted)
            MuteAll();
        else
            UnmuteAll();
    }
    
    void SetMixerVolume(string parameterName, float volume)
    {
        if (audioMixer != null)
        {
            float dbVolume = Mathf.Log10(volume) * 20;
            if (volume <= 0.001f)
                dbVolume = -80f; // Mute
            
            audioMixer.SetFloat(parameterName, dbVolume);
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
        SetMixerVolume(MASTER_VOLUME_PARAM, volume);
        PlayerPrefs.Save();
    }
    
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        SetMixerVolume(MUSIC_VOLUME_PARAM, volume);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        SetMixerVolume(SFX_VOLUME_PARAM, volume);
        PlayerPrefs.Save();
    }
    
    public void SetVoiceVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, volume);
        SetMixerVolume(VOICE_VOLUME_PARAM, volume);
        PlayerPrefs.Save();
    }
    
    public void SetUIVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(UI_VOLUME_KEY, volume);
        SetMixerVolume(UI_VOLUME_PARAM, volume);
        PlayerPrefs.Save();
    }
    
    public void MuteAll()
    {
        PlayerPrefs.SetInt(MUTE_ALL_KEY, 1);
        SetMixerVolume(MASTER_VOLUME_PARAM, 0.001f);
        PlayerPrefs.Save();
    }
    
    public void UnmuteAll()
    {
        PlayerPrefs.SetInt(MUTE_ALL_KEY, 0);
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0.8f);
        SetMixerVolume(MASTER_VOLUME_PARAM, masterVol);
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Reproducción de Sonidos
    
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource("Music");
        if (source != null)
        {
            source.clip = clip;
            source.loop = loop;
            source.Play();
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource("SFX");
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }
    }
    
    public void PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource("Voice");
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume;
            source.Play();
        }
    }
    
    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource("UI");
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume;
            source.Play();
        }
    }
    
    AudioSource GetAvailableAudioSource(string category)
    {
        if (!audioSources.ContainsKey(category))
        {
            CreateAudioSource(category);
        }
        
        // Buscar una fuente de audio que no esté reproduciendo
        foreach (AudioSource source in audioSources[category])
        {
            if (!source.isPlaying)
                return source;
        }
        
        // Si todas están ocupadas, crear una nueva
        return CreateAudioSource(category);
    }
    
    #endregion
    
    #region Métodos de Conveniencia
    
    public void PlayButtonClick()
    {
        PlayUISound(buttonClickSound);
    }
    
    public void PlayHoverSound()
    {
        PlayUISound(hoverSound);
    }
    
    public void PlayPauseSound()
    {
        PlayUISound(pauseSound);
    }
    
    public void PlayUnpauseSound()
    {
        PlayUISound(unpauseSound);
    }
    
    public void PlayNotificationSound()
    {
        PlayUISound(notificationSound);
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
    
    public void PlayRandomZombieSound()
    {
        if (zombieSounds != null && zombieSounds.Length > 0)
        {
            AudioClip randomZombie = zombieSounds[Random.Range(0, zombieSounds.Length)];
            PlaySFX(randomZombie, Random.Range(0.6f, 1.0f));
        }
    }
    
    #endregion
    
    #region Control de Audio
    
    public void StopAllMusic()
    {
        StopAllInCategory("Music");
    }
    
    public void StopAllSFX()
    {
        StopAllInCategory("SFX");
    }
    
    public void StopAllVoice()
    {
        StopAllInCategory("Voice");
    }
    
    public void StopAllUI()
    {
        StopAllInCategory("UI");
    }
    
    public void StopAllAudio()
    {
        foreach (var category in audioSources.Keys)
        {
            StopAllInCategory(category);
        }
    }
    
    void StopAllInCategory(string category)
    {
        if (audioSources.ContainsKey(category))
        {
            foreach (AudioSource source in audioSources[category])
            {
                source.Stop();
            }
        }
    }
    
    #endregion
    
    #region Métodos de Obtención
    
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0.8f);
    }
    
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
    }
    
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
    }
    
    public float GetVoiceVolume()
    {
        return PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 1f);
    }
    
    public float GetUIVolume()
    {
        return PlayerPrefs.GetFloat(UI_VOLUME_KEY, 0.9f);
    }
    
    public bool IsMuted()
    {
        return PlayerPrefs.GetInt(MUTE_ALL_KEY, 0) == 1;
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Limpiar todas las fuentes de audio
        foreach (var category in audioSources.Values)
        {
            category.Clear();
        }
        audioSources.Clear();
    }
}

[System.Serializable]
public class SoundGroup
{
    public string name;
    public AudioClip[] sounds;
}

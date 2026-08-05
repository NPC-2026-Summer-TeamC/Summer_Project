using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    public float MasterVolume { get; private set; } = 1.0f;
    public float BGMVolume { get; private set; } = 1.0f;
    public float SFXVolume { get; private set; } = 1.0f;

    private const string MasterVolKey = "MasterVolume";
    private const string BGMVolKey = "BGMVolume";
    private const string SFXVolKey = "SFXVolume";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolKey, 1.0f);
        BGMVolume = PlayerPrefs.GetFloat(BGMVolKey, 1.0f);
        SFXVolume = PlayerPrefs.GetFloat(SFXVolKey, 1.0f);

        ApplyVolumes();
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MasterVolKey, MasterVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BGMVolKey, BGMVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFXVolKey, SFXVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = BGMVolume * MasterVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = SFXVolume * MasterVolume;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        ApplyVolumes();
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        ApplyVolumes();
        sfxSource.PlayOneShot(clip);
    }
}

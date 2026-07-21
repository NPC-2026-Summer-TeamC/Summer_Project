using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider seSlider;

    void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVol", 1f);
        seSlider.value = PlayerPrefs.GetFloat("SEVol", 1f);
        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        audioMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Clamp(masterSlider.value, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("BGMVol", Mathf.Log10(Mathf.Clamp(bgmSlider.value, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("SEVol", Mathf.Log10(Mathf.Clamp(seSlider.value, 0.0001f, 1f)) * 20);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
    }

    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGMVol", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
    }

    public void SetSEVolume(float volume)
    {
        audioMixer.SetFloat("SEVol", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
    }

    public void OnConfirm()
    {
        PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
        PlayerPrefs.SetFloat("BGMVol", bgmSlider.value);
        PlayerPrefs.SetFloat("SEVol", seSlider.value);
        PlayerPrefs.Save();
        Debug.Log("설정이 저장되었습니다.");
    }
}
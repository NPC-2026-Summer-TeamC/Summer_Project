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
        // 저장된 값 불러오기
        masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVol", 1f);
        seSlider.value = PlayerPrefs.GetFloat("SEVol", 1f);

        ApplyVolumes();
    }

    // 슬라이더 값이 바뀔 때마다 호출됨
    public void ApplyVolumes()
    {
        // 0~1 값을 -80dB~0dB로 변환
        audioMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Clamp(masterSlider.value, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("BGMVol", Mathf.Log10(Mathf.Clamp(bgmSlider.value, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("SEVol", Mathf.Log10(Mathf.Clamp(seSlider.value, 0.0001f, 1f)) * 20);
    }

    // 확인 버튼 누를 때 호출됨
    public void OnConfirm()
    {
        PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
        PlayerPrefs.SetFloat("BGMVol", bgmSlider.value);
        PlayerPrefs.SetFloat("SEVol", seSlider.value);
        PlayerPrefs.Save();
        Debug.Log("설정이 저장되었습니다.");
    }
}
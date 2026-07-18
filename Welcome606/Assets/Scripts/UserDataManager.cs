using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance;

    public UserData userData = new UserData();

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 저장
    public void Save()
    {
        PlayerPrefs.SetInt("ClearedStage", userData.clearedStage);

        PlayerPrefs.SetInt("BGMOn", userData.bgmOn ? 1 : 0);
        PlayerPrefs.SetInt("SFXOn", userData.sfxOn ? 1 : 0);

        PlayerPrefs.SetFloat("BGMVolume", userData.bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", userData.sfxVolume);

        PlayerPrefs.Save();

        Debug.Log("UserData 저장 완료");
    }

    // 불러오기
    public void Load()
    {
        userData.clearedStage = PlayerPrefs.GetInt("ClearedStage", 0);

        userData.bgmOn = PlayerPrefs.GetInt("BGMOn", 1) == 1;
        userData.sfxOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;

        userData.bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        userData.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        Debug.Log("UserData 불러오기 완료");
    }

    // 스테이지 클리어
    public void ClearStage(int stage)
    {
        if (stage > userData.clearedStage)
        {
            userData.clearedStage = stage;
            Save();
        }
    }
}
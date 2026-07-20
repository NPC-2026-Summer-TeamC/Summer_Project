using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    private const int ChapterCount = 5;
    private const int StageCount = 3;

    // UserDataManager 싱글톤 인스턴스
    private static UserDataManager _instance;

    // Lazy Singleton
    // Instance를 처음 호출할 때 자동으로 생성
    public static UserDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("UserDataManager");
                _instance = go.AddComponent<UserDataManager>();

                // 씬이 변경되어도 유지
                DontDestroyOnLoad(go);

                // Load()는 Awake()에서 호출
            }

            return _instance;
        }
    }

    // 실제 유저 데이터
    public UserData userData = new UserData();

    private void Awake()
    {
        // 이미 생성된 인스턴스가 없으면 현재 객체를 사용
        if (_instance == null)
        {
            _instance = this;

            // 씬 전환 시 삭제되지 않도록 설정
            DontDestroyOnLoad(gameObject);

            // 저장된 데이터 불러오기
            Load();
        }
        // 이미 인스턴스가 있으면 중복 객체 제거
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // 저장
    public void Save()
    {
        // 챕터/스테이지 클리어 정보 저장
        for (int chapter = 0; chapter < ChapterCount; chapter++)
        {
            for (int stage = 0; stage < StageCount; stage++)
            {
                PlayerPrefs.SetInt($"Cleared_{chapter}_{stage}",
                    userData.clearedStages[chapter][stage] ? 1 : 0);
            }
        }

        // 사운드 설정 저장
        PlayerPrefs.SetInt("BGMOn", userData.bgmOn ? 1 : 0);
        PlayerPrefs.SetInt("SFXOn", userData.sfxOn ? 1 : 0);

        // 볼륨 저장
        PlayerPrefs.SetFloat("BGMVolume", userData.bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", userData.sfxVolume);

        // 스크립트 속도 저장
        PlayerPrefs.SetFloat("ScriptSpeed", userData.scriptSpeed);

        PlayerPrefs.Save();

        Debug.Log("UserData 저장 완료");
    }

    // 불러오기
    public void Load()
    {
        // 챕터/스테이지 클리어 정보 불러오기
        for (int chapter = 0; chapter < ChapterCount; chapter++)
        {
            for (int stage = 0; stage < StageCount; stage++)
            {
                userData.clearedStages[chapter][stage] =
                    PlayerPrefs.GetInt($"Cleared_{chapter}_{stage}", 0) == 1;
            }
        }

        // 사운드 설정 불러오기
        userData.bgmOn = PlayerPrefs.GetInt("BGMOn", 1) == 1;
        userData.sfxOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;

        // 볼륨 불러오기
        userData.bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        userData.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 스크립트 속도 불러오기
        userData.scriptSpeed = PlayerPrefs.GetFloat("ScriptSpeed", 1f);

        Debug.Log("UserData 불러오기 완료");
    }

    // 스테이지 클리어 처리
    public void ClearStage(int chapter, int stage)
    {
        userData.clearedStages[chapter][stage] = true;
        Save();
    }
}

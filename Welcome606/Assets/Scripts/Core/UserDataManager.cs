using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    // 싱글톤 객체
    private static UserDataManager _instance;
    private static bool _isQuitting = false;

    // Lazy 싱글톤, 객체를 처음 호출할 때 자동으로 생성
    public static UserDataManager Instance
    {
        get
        {
            if (_isQuitting) {
                Debug.LogWarning("애플리케이션 종료 중이라 UserDataManager.Instance를 반환하지 않습니다.");
                return null;
            }

            if (_instance == null) {
                var go = new GameObject("UserDataManager");
                _instance = go.AddComponent<UserDataManager>();
                DontDestroyOnLoad(go);  // 씬이 변경되어도 유지
                // Load()는 Awake()에서 호출
            }
            return _instance;
        }
    }

    // 실제 유저 데이터
    public UserData userData = new UserData();

    public void Save()
    {
        string json = JsonUtility.ToJson(userData);
        PlayerPrefs.SetString("UserDataJson", json);
        PlayerPrefs.Save();
        Debug.Log("UserData 저장 완료 (JSON)");
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("UserDataJson")) {
            string json = PlayerPrefs.GetString("UserDataJson");
            // 필드 누락 시 기본값 보존을 위해 FromJsonOverwrite 사용
            JsonUtility.FromJsonOverwrite(json, userData);
            Debug.Log("UserData 불러오기 완료 (JSON)");
        }
        else {
            Debug.Log("저장된 UserData가 없어 기본값을 사용합니다.");
        }
    }

    public void ClearStage(int chapter, int stage)
    {
        if (chapter >= 0 && chapter < UserDataConstants.ChapterCount &&
    stage >= 0 && stage < UserDataConstants.StageCount)
        {
            userData.chapters[chapter].stages[stage] = true;
            Save();
        }
    }

    public void CollectItem(int chapter)
    {
        if (chapter < 0 || chapter >= UserDataConstants.ChapterCount)
            return;

        if (userData.collectedItems[chapter])
            return;

        userData.collectedItems[chapter] = true;
        Save();
    }

    public bool HasCollectedItem(int chapter)
    {
        if (chapter < 0 || chapter >= UserDataConstants.ChapterCount)
            return false;

        return userData.collectedItems[chapter];
    }

    private void Awake()
    {
        // 이미 생성된 인스턴스가 없으면 현재 객체를 사용
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환 시 삭제되지 않도록 설정
            Load(); // 저장된 데이터 불러오기
        }

        // 이미 인스턴스가 있으면 중복 객체 제거
        else if (_instance != this) {
            Destroy(gameObject);
        }
    }

    // 강제 종료시 자동 저장 호출
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) {
            Save();
        }
    }

    // 정상적으로 앱을 종료할 때 호출
    private void OnApplicationQuit()
    {
        Save();
        _isQuitting = true;
    }
}

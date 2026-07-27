using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    // 싱글톤 객체
    private static UserDataManager instance;
    private static bool isQuitting = false;
    // 실제 유저 데이터
    private UserData userData = new UserData();

    // Lazy 싱글톤, 객체를 처음 호출할 때 자동으로 생성
    public static UserDataManager Instance
    {
        get {
            if (isQuitting) {
                Debug.LogWarning("애플리케이션 종료 중이라 UserDataManager.Instance를 반환하지 않습니다.");
                return null;
            }

            if (instance == null) {
                var go = new GameObject("UserDataManager");
                instance = go.AddComponent<UserDataManager>();
                DontDestroyOnLoad(go);  // 씬이 변경되어도 유지
                // Load()는 Awake()에서 호출
            }
            return instance;
        }
    }

    private void Awake() {
        // 이미 생성된 인스턴스가 없으면 현재 객체를 사용
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        // 이미 인스턴스가 있으면 중복 객체 제거
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    // 강제 종료시 자동 저장 호출
    private void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            Save();
        }
    }

    // 정상적으로 앱을 종료할 때 호출
    private void OnApplicationQuit() {
        Save();
        isQuitting = true;
    }

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

    public bool ClearStage(int chapter, int stage)
    {
        if (chapter < 0 || chapter >= UserDataConst.CHAPTER ||
            stage < 0 || stage >= UserDataConst.STAGE) {
            Debug.LogError("챕터 또는 스테이지 값 오류");
            return false;
        }

        userData.chapters[chapter] = stage;
        Save();
        return true;
    }

    // 3스테이지 클리어 시 아이템 획득
    public bool ClearChapter(int chapter)
    {
        if (chapter < 0 || chapter >= UserDataConst.CHAPTER) {
            Debug.LogError("챕터 값 오류");
            return false;
        }

        if (userData.collectedItems[chapter]) {
            Debug.Log("이미 획득한 아이템입니다.");
            return false;
        }

        userData.collectedItems[chapter] = true;
        Debug.Log($"{chapter}챕터의 아이템을 획득했습니다.");
        Save();
        return true;
    }

    public bool HasCollectedItem(int chapter)
    {
        if (chapter < 0 || chapter >= UserDataConst.CHAPTER) {
            Debug.LogError("챕터 값 오류");
            return false;
        }

        return userData.collectedItems[chapter];
    }

    // 해당 챕터(맵)가 해금되어 이동 가능한지 반환합니다.
    public bool IsChapterUnlocked(int chapter)
    {
        if (chapter < 0 || chapter > UserDataConst.CHAPTER) {
            // Debug.LogError("챕터 값 오류");
            return false;
        }

        if (chapter == 0) {
            // Debug.Log("메인 메뉴입니다.");
            return false;
        }
        return (userData.chapters[chapter - 1] >= UserDataConst.STAGE);
    }

    public bool CanMoveToNextChapter(int currentChapter)
    {
        return IsChapterUnlocked(currentChapter + 1);
    }

    public bool CanMoveToPrevChapter(int currentChapter)
    {
        return currentChapter > 1;
    }

}
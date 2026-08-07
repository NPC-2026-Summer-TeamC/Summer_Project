using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    // 중요!! 유저 데이터(해금 진행도 등)가 변경되었을 때 발송되는 이벤트.
    // UI 컨트롤러(예: ChapterMapController)에서 OnEnable 시점에 구독(+=)하여 RefreshUI()를 연결.
    public event System.Action OnUserDataChanged;
    
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

    public int MaxUnlockChapter => userData != null ? userData.maxUnlockChapter : 1;
    public int MaxUnlockStage => userData != null ? userData.maxUnlockStage : 1;
    public int MaxCollectionItem => userData != null ? userData.maxCollectionItem : 0;

    public bool HasCollectedItem(int chapter)
    {
        if (chapter < 1 || chapter > UserDataConst.CHAPTER) return false;
        return chapter <= MaxCollectionItem;
    }

    public void UserDataLog()
    {
        Debug.Log($"Chapter=[{userData.maxUnlockChapter}], Stage=[{userData.maxUnlockStage}], " +
            $"CollectedItem=[{userData.maxCollectionItem}], Ending=[{userData.isEndingClear}]");
    }

    public void Reset()
    {
        PlayerPrefs.DeleteKey("UserDataJson");
        userData = new UserData();
        Save();
        OnUserDataChanged?.Invoke();
    }

    public bool ClearStage(int chapter, int stage)
    {
        if (chapter < 1 || chapter > UserDataConst.CHAPTER || stage < 1 || stage > UserDataConst.STAGE) {
            return false;
        }

        // 현재 클리어한 스테이지가 진행도 상 최대가 아닌 경우 clear 판정만
        if (chapter < userData.maxUnlockChapter) {
            return true;
        }
        else if (chapter == userData.maxUnlockChapter && stage < userData.maxUnlockStage) {
            return true;
        }

        // 해당 챕터의 마지막 스테이지가 아닌 경우 다음 스테이지 해금
        if (stage < UserDataConst.STAGE) {
            userData.maxUnlockStage = stage + 1;
        }
        // 해당 챕터의 마지막 스테이지를 클리어한 경우 (다음 챕터 해금)
        else if (chapter <= UserDataConst.CHAPTER) {
            CollectItem(chapter);
            userData.maxUnlockChapter = chapter + 1; // 5챕터 클리어 시 6챕터(엔딩 퀘스트) 해금
            userData.maxUnlockStage = 1;
        }

        Save();
        OnUserDataChanged?.Invoke();
        return true;
    }

    // 3스테이지 클리어 시 아이템 획득
    public bool CollectItem(int chapter)
    {
        if (chapter < 1 || chapter > UserDataConst.CHAPTER) {
            Debug.LogError("챕터 값 오류");
            return false;
        }
        userData.maxCollectionItem = chapter;
        Debug.Log($"{chapter}챕터의 아이템을 획득했습니다.");
        return true;
    }
    
    // 해당 챕터(맵)가 해금되어 이동 가능한지 반환합니다.
    public bool IsChapterUnlocked(int chapter)
    {
        if (chapter < 1 || chapter > UserDataConst.CHAPTER) {
            // Debug.LogError("챕터 값 오류");
            return false;
        }
        return chapter <= userData.maxUnlockChapter;
    }

    public bool IsStageUnlocked(int chapter, int stage)
    {
        if (chapter < 1 || chapter > UserDataConst.CHAPTER || stage < 1 || stage > UserDataConst.STAGE) {
            // Debug.LogError("챕터 또는 스테이지 값 오류");
            return false;
        }
        if (chapter < userData.maxUnlockChapter) {  // 이전 챕터는 모두 해금
            return true;
        }
        if (chapter == userData.maxUnlockChapter) { // 현재 챕터는 maxUnlockStage까지 해금
            return stage <= userData.maxUnlockStage;
        }
        return false;   // 미래 챕터는 잠금
    }
}

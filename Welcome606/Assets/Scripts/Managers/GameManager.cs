using UnityEngine;

namespace Welcome606.Managers
{
    /// <summary>
    /// 게임 전역 상태 및 최상위 선택 컨텍스트(현재 챕터/스테이지)를 관리하는 매니저 싱글톤.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        private static bool isQuitting = false;

        [Header("Current Selected Context")]
        [SerializeField] private int selectedChapter = 1;
        [SerializeField] private int selectedStage = 1;

        public static GameManager Instance
        {
            get
            {
                if (isQuitting)
                {
                    Debug.LogWarning("애플리케이션 종료 중이라 GameManager.Instance를 반환하지 않습니다.");
                    return null;
                }

                if (instance == null)
                {
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public int SelectedChapter => selectedChapter;
        public int SelectedStage => selectedStage;

        private void Awake()
        {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);

                // 부팅 시 1회만 저장 진행도와 동기화 (이후 챕터 선택은 Map 씬/UI가 결정)
                SyncContextFromUserData();
                if (UserDataManager.Instance != null) {
                    UserDataManager.Instance.OnUserDataChanged += HandleUserDataChanged;
                }
            }
            else if (instance != this) {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDestroy()
        {
            // 중복 인스턴스가 파괴될 때 구독 해제하면 원본 인스턴스의 구독이 끊기지 않도록 자신일 때만 해제
            if (instance == this && UserDataManager.Instance != null) {
                UserDataManager.Instance.OnUserDataChanged -= HandleUserDataChanged;
            }
        }

        /// <summary>
        /// 데이터 변경 시 선택 챕터가 유효(해금)하면 유지하고, 진행도 초기화 등으로 잠금 상태가 되면 재동기화합니다.
        /// 클리어로 다음 챕터가 해금될 때마다 컨텍스트를 덮어쓰면 이전 챕터 맵에서 잘못된 챕터가 열리기 때문입니다.
        /// </summary>
        private void HandleUserDataChanged()
        {
            if (UserDataManager.Instance == null) {
                return;
            }

            if (!UserDataManager.Instance.IsChapterUnlocked(selectedChapter)) {
                SyncContextFromUserData();
            }
        }

        /// <summary>
        /// UserDataManager의 최근 해금/진행 챕터 및 스테이지 정보를 읽어와 선택 컨텍스트를 동기화합니다.
        /// </summary>
        public void SyncContextFromUserData()
        {
            if (UserDataManager.Instance != null) {
                int chapter = UserDataManager.Instance.MaxUnlockChapter;
                int stage = UserDataManager.Instance.MaxUnlockStage;
                SetSelectedContext(chapter, stage);
                Debug.Log($"[GameManager] SyncContextFromUserData - Chapter: {chapter}, Stage: {stage}");
            }
        }

        /// <summary>
        /// 선택된 챕터(맵) 번호를 설정합니다. (1~5)
        /// </summary>
        public void SetSelectedChapter(int chapter)
        {
            selectedChapter = Mathf.Clamp(chapter, 1, UserDataConst.CHAPTER);
            Debug.Log($"[GameManager] Selected Chapter updated: {selectedChapter}");
        }

        /// <summary>
        /// 선택된 스테이지(퍼즐 단계) 번호를 설정합니다. (1~3)
        /// </summary>
        public void SetSelectedStage(int stage)
        {
            selectedStage = Mathf.Clamp(stage, 1, UserDataConst.STAGE);
            Debug.Log($"[GameManager] Selected Stage updated: {selectedStage}");
        }

        /// <summary>
        /// 챕터 및 스테이지 컨텍스트를 동시에 설정합니다.
        /// </summary>
        public void SetSelectedContext(int chapter, int stage)
        {
            SetSelectedChapter(chapter);
            SetSelectedStage(stage);
        }
    }
}

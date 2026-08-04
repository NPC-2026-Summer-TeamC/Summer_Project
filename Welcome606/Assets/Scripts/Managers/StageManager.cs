using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Welcome606.Managers
{
    /// <summary>
    /// 인게임 퍼즐 씬(StageScene)의 라이프사이클 및 클리어 상태를 총괄 관리하는 매니저 클래스입니다.
    /// 퍼즐 클리어 시 UserDataManager.Instance.ClearStage()를 호출하여 진행도를 저장하고,
    /// SceneFlowManager를 통해 맵 선택 씬(StageEntryScene)으로의 복귀를 제어합니다.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        private static StageManager instance;
        private static bool isQuitting = false;

        public static StageManager Instance
        {
            get
            {
                if (isQuitting)
                {
                    Debug.LogWarning("[StageManager] 애플리케이션 종료 중이라 인스턴스를 반환하지 않습니다.");
                    return null;
                }

                if (instance == null)
                {
#if UNITY_6000_0_OR_NEWER || UNITY_2023_1_OR_NEWER
                    instance = Object.FindFirstObjectByType<StageManager>();
#else
                    instance = Object.FindObjectOfType<StageManager>();
#endif
                    if (instance == null)
                    {
                        var go = new GameObject("StageManager");
                        instance = go.AddComponent<StageManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Stage Runtime Config")]
        [Tooltip("GameManager 컨텍스트가 없을 때 fallback으로 사용할 기본 챕터")]
        [SerializeField] private int fallbackChapter = 1;

        [Tooltip("GameManager 컨텍스트가 없을 때 fallback으로 사용할 기본 스테이지")]
        [SerializeField] private int fallbackStage = 1;

        [Header("Clear UI & Transition")]
        [Tooltip("퍼즐 클리어 시 활성화할 연출/UI 팝업 오브젝트 (선택 사항)")]
        [SerializeField] private GameObject clearPopupObject;

        [Tooltip("클리어 팝업 연출 후 씬 전환까지의 대기 시간(초)")]
        [SerializeField] private float autoReturnDelaySeconds = 2.0f;

        [Tooltip("클리어 후 자동 복귀 여부")]
        [SerializeField] private bool autoReturnToMap = true;

        // Stage Events
        public event System.Action OnStageStart;
        public event System.Action OnStageClear;
        public event System.Action OnStageFail;

        public int CurrentChapter { get; private set; }
        public int CurrentStage { get; private set; }
        public bool IsStageCleared { get; private set; }
        public bool IsStageFailed { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitializeStageContext();
        }

        private void Start()
        {
            OnStageStart?.Invoke();
            Debug.Log($"[StageManager] 스테이지 시작! Chapter: {CurrentChapter}, Stage: {CurrentStage}");
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        /// <summary>
        /// GameManager 싱글톤에서 선택된 챕터 및 스테이지 정보를 가져옵니다.
        /// GameManager가 없으면 fallbackInspector 설정을 사용합니다.
        /// </summary>
        private void InitializeStageContext()
        {
            if (GameManager.Instance != null)
            {
                CurrentChapter = GameManager.Instance.SelectedChapter;
                CurrentStage = GameManager.Instance.SelectedStage;
            }
            else
            {
                CurrentChapter = Mathf.Clamp(fallbackChapter, 1, UserDataConst.CHAPTER);
                CurrentStage = Mathf.Clamp(fallbackStage, 1, UserDataConst.STAGE);
                Debug.LogWarning($"[StageManager] GameManager 인스턴스를 찾지 못해 fallback 값을 사용합니다. (Chapter: {CurrentChapter}, Stage: {CurrentStage})");
            }

            IsStageCleared = false;
            IsStageFailed = false;

            if (clearPopupObject != null)
            {
                clearPopupObject.SetActive(false);
            }
        }

        /// <summary>
        /// 퍼즐 완수 시 호출됩니다.
        /// 진행도를 저장하고, 클리어 이벤트를 발생시키며 맵 복귀를 처리합니다.
        /// </summary>
        public void CompleteStage()
        {
            if (IsStageCleared || IsStageFailed)
            {
                return;
            }

            IsStageCleared = true;
            Debug.Log($"[StageManager] 스테이지 클리어 완수! Chapter: {CurrentChapter}, Stage: {CurrentStage}");

            // 1. UserDataManager 진행도 저장 및 이벤트 발송
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.ClearStage(CurrentChapter, CurrentStage);
            }
            else
            {
                Debug.LogError("[StageManager] UserDataManager.Instance가 null입니다.");
            }

            // 2. 클리어 이벤트 트리거
            OnStageClear?.Invoke();

            // 3. 클리어 팝업 연출 출력
            if (clearPopupObject != null)
            {
                clearPopupObject.SetActive(true);
            }

            // 4. 복귀 처리
            if (autoReturnToMap)
            {
                StartCoroutine(CoReturnToMapAfterDelay(autoReturnDelaySeconds));
            }
        }

        /// <summary>
        /// 기존 스크립트 호환용 퍼즐 클리어 신호 수신 래퍼 메서드입니다.
        /// </summary>
        public void OnPuzzleClear()
        {
            CompleteStage();
        }

        /// <summary>
        /// 퍼즐 실패 시 호출됩니다.
        /// </summary>
        public void FailStage()
        {
            if (IsStageCleared || IsStageFailed)
            {
                return;
            }

            IsStageFailed = true;
            Debug.Log($"[StageManager] 스테이지 실패! Chapter: {CurrentChapter}, Stage: {CurrentStage}");
            OnStageFail?.Invoke();
        }

        /// <summary>
        /// 일정 시간 대기 후 맵 선택 씬(StageEntryScene)으로 돌아갑니다.
        /// </summary>
        private IEnumerator CoReturnToMapAfterDelay(float delaySeconds)
        {
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            ReturnToMapLobby();
        }

        /// <summary>
        /// 맵 선택 씬(StageEntryScene)으로 부드럽게 씬전환 복귀합니다.
        /// </summary>
        public void ReturnToMapLobby()
        {
            string targetScene = "StageEntryScene";

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(targetScene);
            }
            else
            {
                Debug.LogWarning($"[StageManager] SceneFlowManager.Instance가 존재하지 않아 기본 SceneManager로 {targetScene}을 로드합니다.");
                SceneManager.LoadScene(targetScene);
            }
        }

        /// <summary>
        /// 기존 스크립트 호환용 맵 복귀 래퍼 메서드입니다.
        /// </summary>
        public void ReturnToMap()
        {
            ReturnToMapLobby();
        }

        #region Debug Helper
        [ContextMenu("Debug - Complete Stage")]
        public void DebugCompleteStage()
        {
            CompleteStage();
        }
        #endregion
    }
}

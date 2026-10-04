using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Welcome606.Managers;

namespace Welcome606.Ending
{
    /// <summary>
    /// 씬 내의 6챕터 엔딩 퀘스트 흐름 및 메시지/EndingEvent 연출을 제어하는 씬 배치 컨트롤러 컴포넌트.
    /// 퀘스트 안내 메시지는 스토리 대사가 아닌 짧은 시스템 문구이므로, 대사창 씬(DialogueManager)에 의존하지 않고
    /// Ending_PF 내부의 자체 메시지 패널에 표시합니다.
    /// </summary>
    public class EndingQuestController : MonoBehaviour
    {
        [Header("UI & 컴포넌트 연결")]
        public EndingTodoUIController todoUIController;

        [Header("퀘스트 메시지 패널")]
        [Tooltip("퀘스트 안내 메시지를 띄울 패널 (Ending_PF/MessagePanel)")]
        [SerializeField] private GameObject messagePanel;

        [Tooltip("메시지 본문 텍스트 (MessagePanel/MessageText)")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("엔딩 연출 에셋")]
        [Tooltip("엔딩 거울 일러스트 연출 컷씬 패널")]
        public GameObject mirrorCutscenePanel;

        [Tooltip("거울 일러스트 이미지")]
        public Image mirrorImage;

        [Tooltip("엔딩 BGM 클립")]
        public AudioClip endingBgmClip;

        private int currentStep = 0; // 0: 옷(Map4), 1: 떡(Map1), 2: 향수(Map2), 3: 신발(Map3), 4: 종료
        private bool isEventProcessing = false;
        private EndingCollectibleController[] collectibles;

        private readonly string[] stepSuccessMessages = new string[]
        {
            "옷을 입었다.",
            "떡을 먹었다.",
            "향수를 뿌렸다.",
            "신발을 신었다."
        };

        public int CurrentStep => currentStep;

        private void OnEnable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged += RefreshQuestState;
            }
        }

        private void OnDisable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged -= RefreshQuestState;
            }
        }

        private void Start()
        {
            InitializeEndingQuestState();
        }

        public void InitializeEndingQuestState()
        {
            if (todoUIController == null)
            {
                todoUIController = FindFirstObjectByType<EndingTodoUIController>(FindObjectsInactive.Include);
            }

            collectibles = FindObjectsByType<EndingCollectibleController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            RefreshQuestState();

            if (mirrorCutscenePanel != null) mirrorCutscenePanel.SetActive(false);
            if (messagePanel != null) messagePanel.SetActive(false);
        }

        /// <summary>
        /// 진행도가 바뀌었을 때(엔딩 퀘스트 해금 등) 수집품 노출과 Todo 문구를 갱신합니다.
        /// 수집품은 잠금 상태에서 스스로 비활성화되며 이벤트 구독도 해제되므로, 다시 켜는 일은 이 컨트롤러가 맡습니다.
        /// </summary>
        private void RefreshQuestState()
        {
            if (collectibles != null)
            {
                foreach (var item in collectibles)
                {
                    if (item != null) item.RefreshVisibility();
                }
            }

            if (IsChapter6Unlocked() && todoUIController != null)
            {
                todoUIController.UpdateTodoProgress(currentStep);
            }
        }

        public bool IsChapter6Unlocked()
        {
            return UserDataManager.Instance != null && UserDataManager.Instance.IsEndingQuestUnlocked;
        }

        /// <summary>
        /// 수집품 오브젝트 클릭 핸들러
        /// </summary>
        public void OnCollectibleItemClicked(int mapIndex, EndingItemType itemType)
        {
            if (isEventProcessing) return;

            bool isChapter6 = IsChapter6Unlocked();
            if (!isChapter6)
            {
                ShowMessage("아직 6챕터 엔딩 퀘스트 단계가 아닙니다.");
                return;
            }

            int targetItemStep = (int)itemType;

            if (targetItemStep < currentStep)
            {
                ShowMessage("이미 사용했다.");
            }
            else if (targetItemStep > currentStep)
            {
                ShowMessage("아직은 사용할 때가 아니다.");
            }
            else
            {
                // 올바른 순서의 수집품 클릭
                StartCoroutine(CoProcessStepSuccess(targetItemStep));
            }
        }

        private IEnumerator CoProcessStepSuccess(int step)
        {
            isEventProcessing = true;

            // 1. 성공 대사 연출 출력
            yield return StartCoroutine(CoShowMessageAsync(stepSuccessMessages[step], 2.0f));

            // 2. 단계 상승
            currentStep++;

            if (todoUIController != null)
            {
                todoUIController.UpdateTodoProgress(currentStep);
            }

            // 3. 마지막 신발 단계 완료 시 자동으로 EndingEvent (고정 속도 Cutscene 연출) 실행
            if (currentStep >= 4)
            {
                yield return StartCoroutine(CoTriggerEndingCutscene());
            }

            isEventProcessing = false;
        }

        /// <summary>
        /// 모든 Todo 퀘스트 완료 시 자동으로 실행되는 EndingEvent 연출 코루틴.
        /// (BGM 재생, 스크립트 속도/Auto 모드 고정 연출 후 CreditsScene 전환)
        /// </summary>
        private IEnumerator CoTriggerEndingCutscene()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.SetEnding(true);
            }

            // 1. 엔딩 BGM 전환
            if (SoundManager.Instance != null && endingBgmClip != null)
            {
                SoundManager.Instance.PlayBGM(endingBgmClip);
            }

            // 2. 거울 일러스트 컷씬 패널 페이드 인
            if (mirrorCutscenePanel != null)
            {
                mirrorCutscenePanel.SetActive(true);

                CanvasGroup cg = mirrorCutscenePanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    float elapsed = 0f;
                    while (elapsed < 1.5f)
                    {
                        elapsed += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(0f, 1f, elapsed / 1.5f);
                        yield return null;
                    }
                    cg.alpha = 1f;
                }
            }

            yield return new WaitForSeconds(1.0f);

            // 3. 음악과 타이밍에 맞춘 고정 속도 메시지 연출
            yield return StartCoroutine(CoShowMessageAsync("거울 속 나의 모습을 바라보았다.", 2.5f));
            yield return StartCoroutine(CoShowMessageAsync("모든 준비가 끝났다.", 2.5f));
            yield return StartCoroutine(CoShowMessageAsync("미희는 현관문을 열고 나갔다.", 3.0f));

            // 4. CreditsScene 비동기 씬 페이드 전환
            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene("CreditsScene", 1.5f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("CreditsScene");
            }
        }

        public void ShowMessage(string msg)
        {
            StopAllCoroutines();
            StartCoroutine(CoShowMessageAsync(msg, 2.0f));
        }

        /// <summary>
        /// 메시지 패널에 문구를 duration초 동안 표시합니다. 패널이 연결되지 않은 씬에서도 퀘스트 흐름이 멈추지 않도록
        /// Console 로그로 대체하고 같은 시간만큼 대기합니다.
        /// </summary>
        private IEnumerator CoShowMessageAsync(string msg, float duration)
        {
            if (messagePanel == null || messageText == null)
            {
                Debug.Log($"[EndingQuestController] {msg}");
                yield return new WaitForSeconds(duration);
                yield break;
            }

            messageText.text = msg;
            messagePanel.SetActive(true);

            yield return new WaitForSeconds(duration);

            messagePanel.SetActive(false);
        }

        /// <summary>
        /// 테스트용 강제 Step 변경
        /// </summary>
        public void SetStepForDebug(int step)
        {
            currentStep = Mathf.Clamp(step, 0, 4);
            if (todoUIController != null)
            {
                todoUIController.UpdateTodoProgress(currentStep);
            }
            if (currentStep >= 4 && !isEventProcessing)
            {
                StartCoroutine(CoTriggerEndingCutscene());
            }
        }

#if UNITY_EDITOR
        // 디버그 진입점: Play 모드에서 Inspector의 EndingQuestController 우측 ⋮ 메뉴로 실행합니다.
        // 예전 forceEnableEndingQuest 플래그처럼 판정을 우회하지 않고 실제 진행도(UserDataManager)를 바꾸므로,
        // 수집품·Todo·맵 상태가 실제 플레이와 같은 경로로 갱신됩니다. 테스트 후 'Reset Progress'로 되돌리세요.
        [ContextMenu("Debug/Unlock Ending Quest (1~5챕터 클리어)")]
        private void DebugUnlockEndingQuest()
        {
            if (!Application.isPlaying || UserDataManager.Instance == null) {
                Debug.LogWarning("[EndingQuestController] Play 모드에서만 실행할 수 있습니다.");
                return;
            }

            for (int chapter = 1; chapter <= UserDataConst.CHAPTER; chapter++) {
                for (int stage = 1; stage <= UserDataConst.STAGE; stage++) {
                    UserDataManager.Instance.ClearStage(chapter, stage);
                }
            }
            UserDataManager.Instance.UserDataLog();
        }

        [ContextMenu("Debug/Next Quest Step")]
        private void DebugNextQuestStep()
        {
            if (!Application.isPlaying) {
                return;
            }
            SetStepForDebug(currentStep + 1);
        }

        [ContextMenu("Debug/Reset Progress")]
        private void DebugResetProgress()
        {
            if (!Application.isPlaying || UserDataManager.Instance == null) {
                return;
            }
            currentStep = 0;
            UserDataManager.Instance.Reset();
        }
#endif
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Welcome606.Managers;

namespace Welcome606.Ending
{
    /// <summary>
    /// 씬 내의 6챕터 엔딩 퀘스트 흐름 및 메시지/EndingEvent 연출을 제어하는 씬 배치 컨트롤러 컴포넌트.
    /// (DialogModal_PF 프리팹 / DialogueManager 전용 연동)
    /// </summary>
    public class EndingQuestController : MonoBehaviour
    {
        [Header("UI & 컴포넌트 연결")]
        public EndingTodoUIController todoUIController;

        [Header("DialogModal 모달 연동 (DialogModal_PF 프리팹)")]
        [Tooltip("대사 스토리 연출 DialogModal 모달창 GameObject (DialogModal_PF)")]
        public GameObject dialogModal;

        [HideInInspector]
        public DialogueManager dialogueManager;

        [Header("엔딩 연출 에셋")]
        [Tooltip("엔딩 거울 일러스트 연출 컷씬 패널")]
        public GameObject mirrorCutscenePanel;

        [Tooltip("거울 일러스트 이미지")]
        public Image mirrorImage;

        [Tooltip("엔딩 BGM 클립")]
        public AudioClip endingBgmClip;

        [Header("테스트/디버그")]
        [Tooltip("true일 경우 UserDataManager 챕터 상태와 상관없이 퀘스트 활성화")]
        public bool forceEnableEndingQuest = false;

        private int currentStep = 0; // 0: 옷(Map4), 1: 떡(Map1), 2: 향수(Map2), 3: 신발(Map3), 4: 종료
        private bool isEventProcessing = false;

        private readonly string[] stepSuccessMessages = new string[]
        {
            "옷을 입었다.",
            "떡을 먹었다.",
            "향수를 뿌렸다.",
            "신발을 신었다."
        };

        private void Start()
        {
            InitializeEndingQuestState();
        }

        public void InitializeEndingQuestState()
        {
            bool isChapter6 = forceEnableEndingQuest || IsChapter6Unlocked();

            if (todoUIController == null)
            {
                todoUIController = FindFirstObjectByType<EndingTodoUIController>(FindObjectsInactive.Include);
            }

            if (dialogueManager == null)
            {
                dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
            }

            if (isChapter6 && todoUIController != null)
            {
                todoUIController.UpdateTodoProgress(currentStep);
            }

            // 씬 내 배치된 수집품 오브젝트들의 상태 갱신 알림
            EndingCollectibleItem[] items = FindObjectsByType<EndingCollectibleItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item != null) item.RefreshVisibility();
            }

            if (mirrorCutscenePanel != null) mirrorCutscenePanel.SetActive(false);
            if (dialogModal != null && dialogModal.activeSelf) dialogModal.SetActive(false);
        }

        public bool IsChapter6Unlocked()
        {
            if (forceEnableEndingQuest) return true;

            if (UserDataManager.Instance != null)
            {
                return UserDataManager.Instance.IsEnding || UserDataManager.Instance.MaxUnlockChapter >= 6;
            }
            return false;
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

            // 3. 음악과 타이밍에 맞춘 대사 연출 (DialogModal / DialogueManager 자동 고정속도 연출)
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

        private IEnumerator CoShowMessageAsync(string msg, float duration)
        {
            var dm = dialogueManager != null ? dialogueManager : DialogueManager.Instance;
            if (dm == null)
            {
                dm = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
            }

            if (dm != null && dm.dialogText != null)
            {
                if (dialogModal != null) dialogModal.SetActive(true);
                else dm.gameObject.SetActive(true);

                if (dm.nameText != null) dm.nameText.text = "";
                dm.dialogText.text = msg;

                yield return new WaitForSeconds(duration);

                if (dialogModal != null) dialogModal.SetActive(false);
                else dm.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[EndingQuestController] {msg}");
                yield return new WaitForSeconds(duration);
            }
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
    }
}

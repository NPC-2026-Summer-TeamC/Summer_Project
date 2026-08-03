using UnityEngine;
using UnityEngine.UI;
using Welcome606.Managers;

namespace Welcome606.UI
{
    /// <summary>
    /// StageEntryScene에서 1~3번 스테이지(퍼즐 단계) 진입 버튼의 해금/숨김(SetActive) 상태를 제어하고 퍼즐 씬 진입을 처리하는 컨트롤러.
    /// 챕터 정보는 GameManager.Instance.SelectedChapter로 통일하여 관리합니다.
    /// </summary>
    public class StageSelectController : MonoBehaviour
    {
        [Header("Target Scene Config")]
        [Tooltip("스테이지 진입 시 전이할 퍼즐 씬 이름 (기본값: StageScene)")]
        [SerializeField] private string targetStageSceneName = "StageScene";

        [Header("Fixed Stage UI Buttons (Exactly 3)")]
        [SerializeField] private Button stage1Button;
        [SerializeField] private Button stage2Button;
        [SerializeField] private Button stage3Button;

        private Button[] StageButtons => new Button[] { stage1Button, stage2Button, stage3Button };

        private UnityEngine.Events.UnityAction[] stageButtonActions;

        private void Awake()
        {
            stageButtonActions = new UnityEngine.Events.UnityAction[]
            {
                OnClickStage1,
                OnClickStage2,
                OnClickStage3
            };
            SetupButtonListeners();
        }

        private void OnDestroy()
        {
            RemoveButtonListeners();
        }

        private void OnEnable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged += RefreshUI;
            }
            RefreshUI();
        }

        private void OnDisable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged -= RefreshUI;
            }
        }

        private void SetupButtonListeners()
        {
            Button[] buttons = StageButtons;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && stageButtonActions != null && i < stageButtonActions.Length)
                {
                    buttons[i].onClick.RemoveListener(stageButtonActions[i]);
                    buttons[i].onClick.AddListener(stageButtonActions[i]);
                }
            }
        }

        private void RemoveButtonListeners()
        {
            Button[] buttons = StageButtons;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && stageButtonActions != null && i < stageButtonActions.Length)
                {
                    buttons[i].onClick.RemoveListener(stageButtonActions[i]);
                }
            }
        }

        private void OnClickStage1() => OnClickStageButton(1);
        private void OnClickStage2() => OnClickStageButton(2);
        private void OnClickStage3() => OnClickStageButton(3);

        /// <summary>
        /// GameManager.Instance.SelectedChapter와 UserData의 진행도(IsStageUnlocked)를 확인하여
        /// 1~3번 스테이지 버튼 노출 및 활성화 상태를 갱신합니다.
        /// </summary>
        public void RefreshUI()
        {
            int chapter = GameManager.Instance != null ? GameManager.Instance.SelectedChapter : 1;
            Button[] buttons = StageButtons;

            for (int i = 0; i < buttons.Length; i++)
            {
                int stageNum = i + 1;
                if (buttons[i] == null) continue;

                bool isUnlocked = UserDataManager.Instance != null &&
                                  UserDataManager.Instance.IsStageUnlocked(chapter, stageNum);

                // 해금 여부에 따라 버튼 노출/숨김 및 인터랙션 설정
                buttons[i].gameObject.SetActive(isUnlocked);
                buttons[i].interactable = isUnlocked;
            }
        }

        private void OnClickStageButton(int stage)
        {
            int chapter = GameManager.Instance != null ? GameManager.Instance.SelectedChapter : 1;
            Debug.Log($"[StageSelectController] Chapter {chapter} - Stage {stage} Selected!");

            // 1. 선택된 스테이지 컨텍스트 저장
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetSelectedStage(stage);
            }

            // 2. 비동기 페이드 씬 전이 실행
            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(targetStageSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetStageSceneName);
            }
        }
    }
}

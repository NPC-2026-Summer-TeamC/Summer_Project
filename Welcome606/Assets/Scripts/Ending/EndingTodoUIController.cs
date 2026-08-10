using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Welcome606.Managers;

namespace Welcome606.Ending
{
    /// <summary>
    /// .unity 씬의 UI Canvas에 미리 배치해 두고 6챕터 해금 시 활성화되어
    /// 현재 진행 중인 Todo 퀘스트 1개만 순차적으로 보여주는 UI 컨트롤러.
    /// </summary>
    public class EndingTodoUIController : MonoBehaviour
    {
        [Header("UI 구성요소")]
        [Tooltip("Todo 리스트 전체 패널 (미지정 시 현재 GameObject)")]
        public GameObject todoPanel;

        [Tooltip("단일 진행중 Todo 텍스트 UI 컴포넌트 (추천)")]
        public TextMeshProUGUI singleTodoText;

        [Tooltip("기존 4개 Todo 텍스트 UI 목록 (하위 호환 지원, 현재 Step 항목만 활성화)")]
        public TextMeshProUGUI[] todoTexts;

        [Tooltip("완료 체크 표시 아이콘 목록")]
        public GameObject[] completedCheckIcons;

        [Header("스타일 설정")]
        public Color normalColor = Color.white;
        public Color activeColor = new Color(1f, 0.85f, 0.4f, 1f); // 진행 중 하이라이트
        public string bulletSymbol = "• ";

        private readonly string[] questDescriptions = new string[]
        {
            "옷을 입자.",
            "떡을 먹자.",
            "향수를 뿌리자.",
            "신발을 신자."
        };

        private void OnEnable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged += RefreshVisibility;
            }
            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged -= RefreshVisibility;
            }
        }

        private void Start()
        {
            if (todoPanel == null) todoPanel = gameObject;
            if (singleTodoText == null && (todoTexts == null || todoTexts.Length == 0))
            {
                singleTodoText = GetComponentInChildren<TextMeshProUGUI>(true);
            }
            RefreshVisibility();
        }

        public void RefreshVisibility()
        {
            bool isUnlocked = IsChapter6Unlocked();
            ShowTodoPanel(isUnlocked);
        }

        private bool IsChapter6Unlocked()
        {
            var questController = FindFirstObjectByType<EndingQuestController>(FindObjectsInactive.Include);
            if (questController != null && questController.forceEnableEndingQuest)
            {
                return true;
            }

            if (UserDataManager.Instance != null)
            {
                return UserDataManager.Instance.IsEnding || UserDataManager.Instance.MaxUnlockChapter >= 6;
            }
            return false;
        }

        public void ShowTodoPanel(bool show)
        {
            GameObject target = todoPanel != null ? todoPanel : gameObject;
            if (target != null && target.activeSelf != show)
            {
                target.SetActive(show);
            }
        }

        /// <summary>
        /// 현재 퀘스트 진행 단계(0~4)에 맞춰 순차적으로 1개의 Todo UI만 노출합니다.
        /// </summary>
        /// <param name="currentStep">0: 옷, 1: 떡, 2: 향수, 3: 신발, 4: 전체 완료</param>
        public void UpdateTodoProgress(int currentStep)
        {
            if (currentStep >= questDescriptions.Length)
            {
                // 모든 퀘스트 완료 시 EndingEvent가 실행되므로 패널을 감추거나 완료 표시
                if (singleTodoText != null)
                {
                    singleTodoText.text = $"{bulletSymbol}준비 완료";
                    singleTodoText.color = activeColor;
                }
                ShowTodoPanel(false);
                return;
            }

            ShowTodoPanel(true);

            string currentQuestText = $"{bulletSymbol}{questDescriptions[currentStep]}";

            // 1. 단일 Text 컴포넌트가 바인딩되어 있는 경우
            if (singleTodoText != null)
            {
                singleTodoText.text = currentQuestText;
                singleTodoText.color = activeColor;
                singleTodoText.gameObject.SetActive(true);
            }

            // 2. 배열형 Text UI 컴포넌트가 할당된 경우 (현재 Step 1개만 활성화)
            if (todoTexts != null && todoTexts.Length > 0)
            {
                for (int i = 0; i < todoTexts.Length; i++)
                {
                    if (todoTexts[i] != null)
                    {
                        if (i == currentStep)
                        {
                            todoTexts[i].text = currentQuestText;
                            todoTexts[i].color = activeColor;
                            todoTexts[i].gameObject.SetActive(true);
                        }
                        else
                        {
                            todoTexts[i].gameObject.SetActive(false);
                        }
                    }
                }
            }

            // 3. 완료 체크 아이콘 갱신
            if (completedCheckIcons != null && completedCheckIcons.Length > 0)
            {
                for (int i = 0; i < completedCheckIcons.Length; i++)
                {
                    if (completedCheckIcons[i] != null)
                    {
                        completedCheckIcons[i].SetActive(i < currentStep);
                    }
                }
            }
        }
    }
}

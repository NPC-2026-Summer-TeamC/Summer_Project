using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Welcome606.Managers;

namespace Welcome606.Ending
{
    /// <summary>
    /// .unity 씬의 UI Canvas에 미리 배치해 두고 6챕터 해금 시 활성화되어 Todo 퀘스트 4단계 상태를 보여주는 UI 컨트롤러.
    /// </summary>
    public class EndingTodoUIController : MonoBehaviour
    {
        [Header("UI 구성요소")]
        [Tooltip("Todo 리스트 전체 패널 (미지정 시 현재 GameObject)")]
        public GameObject todoPanel;

        [Tooltip("4개 Todo 텍스트 UI 목록 (0: 옷, 1: 떡, 2: 향수, 3: 신발)")]
        public TextMeshProUGUI[] todoTexts;

        [Tooltip("완료 체크 표시/스트라이크스루 아이콘 목록")]
        public GameObject[] completedCheckIcons;

        [Header("스타일 설정")]
        public Color normalColor = Color.white;
        public Color activeColor = new Color(1f, 0.85f, 0.4f, 1f); // 진행 중 하이라이트
        public Color completedColor = new Color(0.6f, 0.6f, 0.6f, 1f); // 완료 시 연하게

        private readonly string[] questDescriptions = new string[]
        {
            "1. 옷을 입자.",
            "2. 떡을 먹자.",
            "3. 향수를 뿌리자.",
            "4. 신발을 신자."
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
            if (todoTexts == null || todoTexts.Length == 0)
            {
                todoTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
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
                return UserDataManager.Instance.MaxUnlockChapter >= 6;
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
        /// 현재 퀘스트 진행 단계(0~4)에 맞춰 Todo UI 상태를 갱신합니다.
        /// </summary>
        /// <param name="currentStep">0: 옷, 1: 떡, 2: 향수, 3: 신발, 4: 전체 완료</param>
        public void UpdateTodoProgress(int currentStep)
        {
            ShowTodoPanel(true);

            for (int i = 0; i < 4; i++)
            {
                if (todoTexts != null && i < todoTexts.Length && todoTexts[i] != null)
                {
                    if (i < currentStep)
                    {
                        todoTexts[i].text = $"<s>{questDescriptions[i]}</s>";
                        todoTexts[i].color = completedColor;
                    }
                    else if (i == currentStep)
                    {
                        todoTexts[i].text = questDescriptions[i];
                        todoTexts[i].color = activeColor;
                    }
                    else
                    {
                        todoTexts[i].text = questDescriptions[i];
                        todoTexts[i].color = normalColor;
                    }
                }

                if (completedCheckIcons != null && i < completedCheckIcons.Length && completedCheckIcons[i] != null)
                {
                    completedCheckIcons[i].SetActive(i < currentStep);
                }
            }
        }
    }
}

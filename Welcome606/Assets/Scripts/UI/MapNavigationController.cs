using UnityEngine;
using UnityEngine.UI;
using Welcome606.Managers;

namespace Welcome606.UI
{
    /// <summary>
    /// 맵 간 이동 (좌/우 이전 맵 / 다음 맵 이동 화살표 버튼 제어) 컨트롤러.
    /// 맵 씬 진입 시 GameManager의 선택 챕터를 해당 맵의 챕터로 설정합니다.
    /// </summary>
    public class MapNavigationController : MonoBehaviour
    {
        [Header("Current Map Info")]
        [Tooltip("현재 맵(씬)의 챕터 번호 (1~5)")]
        [SerializeField] private int currentChapter = 1;

        [Header("UI Buttons")]
        [SerializeField] private Button prevChapterButton;
        [SerializeField] private Button nextChapterButton;

        private void Awake()
        {
            // 진입한 맵 기준으로 컨텍스트를 맞춰야 StageEntryScene에서 해당 챕터의 스테이지가 열림
            if (GameManager.Instance != null) {
                GameManager.Instance.SetSelectedChapter(currentChapter);
            }

            // 버튼 클릭 이벤트 연결
            if (prevChapterButton != null)
                prevChapterButton.onClick.AddListener(OnClickPrevChapter);

            if (nextChapterButton != null)
                nextChapterButton.onClick.AddListener(OnClickNextChapter);
        }

        private void OnEnable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged += RefreshNavigationButtons;
            }
            RefreshNavigationButtons();
        }

        private void OnDisable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged -= RefreshNavigationButtons;
            }
        }

        /// <summary>
        /// 현재 챕터 위치와 UserData 해금 상태를 기반으로 이전/다음 버튼 노출 갱신
        /// </summary>
        public void RefreshNavigationButtons()
        {
            // 1. 이전 챕터 버튼: 1챕터가 아니면 항상 노출
            bool canGoPrev = currentChapter > 1;
            if (prevChapterButton != null)
            {
                prevChapterButton.gameObject.SetActive(canGoPrev);
            }

            // 2. 다음 챕터 버튼: 다음 챕터가 해금되었을 때만 노출
            bool canGoNext = UserDataManager.Instance != null && UserDataManager.Instance.IsChapterUnlocked(currentChapter + 1);
            if (nextChapterButton != null)
            {
                nextChapterButton.gameObject.SetActive(canGoNext);
            }
        }

        private void OnClickPrevChapter()
        {
            int targetChapter = currentChapter - 1;
            Debug.Log($"[MapNavigationController] 이전 맵으로 이동: Map0{targetChapter}Scene");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetSelectedChapter(targetChapter);
            }

            string sceneName = $"Map0{targetChapter}Scene";
            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(sceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }

        private void OnClickNextChapter()
        {
            int targetChapter = currentChapter + 1;
            Debug.Log($"[MapNavigationController] 다음 맵으로 이동: Map0{targetChapter}Scene");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetSelectedChapter(targetChapter);
            }

            string sceneName = $"Map0{targetChapter}Scene";
            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(sceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using Welcome606.Managers;

namespace Welcome606.UI
{
    /// <summary>
    /// MainMenuScene의 '시작' 버튼을 처리하는 컨트롤러.
    /// 저장된 최대 해금 챕터의 맵 씬으로 이동합니다. (진행도 초기화 직후에는 자연스럽게 Map01Scene)
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        private void Awake()
        {
            if (startButton != null) {
                startButton.onClick.AddListener(OnClickStart);
            }
        }

        private void OnDestroy()
        {
            if (startButton != null) {
                startButton.onClick.RemoveListener(OnClickStart);
            }
        }

        private void OnClickStart()
        {
            // 엔딩 이후(MaxUnlockChapter == 6)에도 존재하는 맵은 5챕터까지이므로 Clamp
            int chapter = 1;
            if (UserDataManager.Instance != null) {
                chapter = Mathf.Clamp(UserDataManager.Instance.MaxUnlockChapter, 1, UserDataConst.CHAPTER);
            }

            if (GameManager.Instance != null) {
                GameManager.Instance.SetSelectedChapter(chapter);
            }

            string sceneName = $"Map0{chapter}Scene";
            if (SceneFlowManager.Instance != null) {
                SceneFlowManager.Instance.LoadScene(sceneName);
            }
            else {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }
    }
}

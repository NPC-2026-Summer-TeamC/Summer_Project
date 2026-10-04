using UnityEngine;
using Welcome606.Managers;

namespace Welcome606.UI
{
    /// <summary>
    /// 지정한 씬으로 전환하는 범용 핸들러. Collider가 있는 오브젝트는 클릭(OnMouseDown)으로,
    /// UI Button은 onClick에 LoadTargetScene을 연결해 사용합니다. 전환은 항상 SceneFlowManager(페이드/광클 방지)를 경유합니다.
    /// </summary>
    public class SceneTransitionHandler : MonoBehaviour
    {
        [Tooltip("전환할 대상 씬 이름 (Build Settings에 등록된 이름)")]
        [SerializeField] private string targetSceneName;

        private void OnMouseDown()
        {
            LoadTargetScene();
        }

        /// <summary>
        /// 대상 씬으로 전환합니다. 씬 이름이 비어 있으면 경고만 남기고 무시합니다.
        /// </summary>
        public void LoadTargetScene()
        {
            if (string.IsNullOrEmpty(targetSceneName)) {
                Debug.LogWarning($"[SceneTransitionHandler] targetSceneName이 비어 있습니다: {name}");
                return;
            }

            if (SceneFlowManager.Instance != null) {
                SceneFlowManager.Instance.LoadScene(targetSceneName);
            }
            else {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}

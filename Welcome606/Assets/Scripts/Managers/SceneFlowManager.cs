using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Welcome606.Managers
{
    /// <summary>
    /// CanvasGroup 기반 화면 Fade In/Out 비동기 씬 전환 및 광클 입력 방지를 담당하는 싱글톤 매니저.
    /// </summary>
    public class SceneFlowManager : MonoBehaviour
    {
        private static SceneFlowManager instance;
        private static bool isQuitting = false;

        [Header("Fade UI Config")]
        [SerializeField] private Canvas fadeCanvas;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private Image fadeImage;
        [SerializeField] private float defaultFadeDuration = 0.5f;

        private bool isTransitioning = false;

        public static SceneFlowManager Instance
        {
            get
            {
                if (isQuitting)
                {
                    Debug.LogWarning("애플리케이션 종료 중이라 SceneFlowManager.Instance를 반환하지 않습니다.");
                    return null;
                }

                if (instance == null)
                {
                    var go = new GameObject("SceneFlowManager");
                    instance = go.AddComponent<SceneFlowManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureFadeCanvasSetup();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        /// <summary>
        /// 페이드 연출용 Canvas / CanvasGroup / Image가 인스펙터에 할당되어 있지 않을 경우 동적으로 생성합니다.
        /// </summary>
        private void EnsureFadeCanvasSetup()
        {
            if (fadeCanvasGroup != null) return;

            // 1. Overlay Canvas 생성
            GameObject canvasGo = new GameObject("SceneFlow_FadeCanvas");
            canvasGo.transform.SetParent(transform);

            fadeCanvas = canvasGo.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999; // 최상단 노출

            canvasGo.AddComponent<CanvasScaler>();
            fadeCanvasGroup = canvasGo.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;

            // 2. Fullscreen Image 생성
            GameObject imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(canvasGo.transform, false);

            fadeImage = imageGo.AddComponent<Image>();
            fadeImage.color = Color.black;

            RectTransform rectTransform = fadeImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 지정한 씬 이름으로 비동기 로딩 및 페이드 전환을 실행합니다.
        /// </summary>
        public void LoadScene(string sceneName, float fadeDuration = -1f)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"[SceneFlowManager] 이미 씬 전환 진행 중입니다. 요청 무시: {sceneName}");
                return;
            }

            float duration = fadeDuration > 0f ? fadeDuration : defaultFadeDuration;
            StartCoroutine(CoTransitionToScene(sceneName, duration));
        }

        private IEnumerator CoTransitionToScene(string sceneName, float duration)
        {
            isTransitioning = true;
            EnsureFadeCanvasSetup();

            // 광클 방지 및 Fade Out (화면 검게)
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = true;
            yield return CoFade(0f, 1f, duration);

            // 비동기 씬 로딩
            AsyncOperation asyncOp = null;
            try
            {
                asyncOp = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneFlowManager] 씬 로드 예외 발생: {sceneName}, Error: {ex.Message}");
            }

            if (asyncOp != null)
            {
                asyncOp.allowSceneActivation = false;

                // 씬 로딩 완료 대기 (0.9에서 대기)
                while (asyncOp.progress < 0.9f)
                {
                    yield return null;
                }

                // 씬 활성화
                asyncOp.allowSceneActivation = true;
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                Debug.LogError($"[SceneFlowManager] 씬을 찾을 수 없거나 로드에 실패했습니다: {sceneName}");
            }

            // 로드 실패 시에도 화면이 검게 마비되는 현상을 방지하기 위해 Fade In 및 입력 상태 복구 보장
            yield return CoFade(1f, 0f, duration);

            RestoreFadeCanvasState();
        }

        private void RestoreFadeCanvasState()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
            }
            isTransitioning = false;
        }

        private IEnumerator CoFade(float startAlpha, float targetAlpha, float duration)
        {
            float elapsed = 0f;
            fadeCanvasGroup.alpha = startAlpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}

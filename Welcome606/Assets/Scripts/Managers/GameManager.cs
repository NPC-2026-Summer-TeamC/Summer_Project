using UnityEngine;

namespace Welcome606.Managers
{
    /// <summary>
    /// 게임 전역 상태 및 최상위 선택 컨텍스트(현재 챕터/스테이지)를 관리하는 매니저 싱글톤.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        private static bool isQuitting = false;

        [Header("Current Selected Context")]
        [SerializeField] private int selectedChapter = 1;
        [SerializeField] private int selectedStage = 1;

        public static GameManager Instance
        {
            get
            {
                if (isQuitting)
                {
                    Debug.LogWarning("애플리케이션 종료 중이라 GameManager.Instance를 반환하지 않습니다.");
                    return null;
                }

                if (instance == null)
                {
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public int SelectedChapter => selectedChapter;
        public int SelectedStage => selectedStage;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
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
        /// 선택된 챕터(맵) 번호를 설정합니다. (1~5)
        /// </summary>
        public void SetSelectedChapter(int chapter)
        {
            selectedChapter = Mathf.Clamp(chapter, 1, UserDataConst.CHAPTER);
            Debug.Log($"[GameManager] Selected Chapter updated: {selectedChapter}");
        }

        /// <summary>
        /// 선택된 스테이지(퍼즐 단계) 번호를 설정합니다. (1~3)
        /// </summary>
        public void SetSelectedStage(int stage)
        {
            selectedStage = Mathf.Clamp(stage, 1, UserDataConst.STAGE);
            Debug.Log($"[GameManager] Selected Stage updated: {selectedStage}");
        }

        /// <summary>
        /// 챕터 및 스테이지 컨텍스트를 동시에 설정합니다.
        /// </summary>
        public void SetSelectedContext(int chapter, int stage)
        {
            SetSelectedChapter(chapter);
            SetSelectedStage(stage);
        }
    }
}

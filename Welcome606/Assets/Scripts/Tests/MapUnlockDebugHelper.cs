using UnityEngine;

namespace Welcome606.Tests
{
    /// <summary>
    /// 에디터 및 런타임 상에서 디버그 키 입력을 통해 맵/스테이지 해금 상태 및 UserData 변경 이벤트를 실시간 검증하는 헬퍼.
    /// </summary>
    public class MapUnlockDebugHelper : MonoBehaviour
    {
        [Header("Debug Controls")]
        [Tooltip("Space 키 입력 시 현재 진행 중인 스테이지 클리어 테스트")]
        [SerializeField] private bool enableKeyControls = true;

        [SerializeField] private int testChapter = 1;
        [SerializeField] private int testStage = 1;

        private void Update()
        {
            if (!enableKeyControls) return;

            // Space: 지정한 testChapter, testStage 클리어 실행
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"[DebugHelper] Space pressed -> ClearStage({testChapter}, {testStage})");
                if (UserDataManager.Instance != null)
                {
                    UserDataManager.Instance.ClearStage(testChapter, testStage);
                    UserDataManager.Instance.UserDataLog();
                }
            }

            // Keypad 6 / Alpha 6: 챕터 6 강제 진입 (1~5 챕터 전 스테이지 클리어)
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                Debug.Log("[DebugHelper] Alpha6 pressed -> Unlock Chapter 6");
                if (UserDataManager.Instance != null)
                {
                    for (int c = 1; c <= 5; c++)
                    {
                        for (int s = 1; s <= 3; s++)
                        {
                            UserDataManager.Instance.ClearStage(c, s);
                        }
                    }
                    UserDataManager.Instance.UserDataLog();
                }
            }
        }

        [ContextMenu("Clear Test Stage")]
        public void ClearTestStage()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.ClearStage(testChapter, testStage);
                UserDataManager.Instance.UserDataLog();
            }
        }

        [ContextMenu("Unlock All Chapters (Chapter 6 Ready)")]
        public void UnlockAllChapters()
        {
            if (UserDataManager.Instance != null)
            {
                for (int c = 1; c <= 5; c++)
                {
                    for (int s = 1; s <= 3; s++)
                    {
                        UserDataManager.Instance.ClearStage(c, s);
                    }
                }
                UserDataManager.Instance.UserDataLog();
            }
        }

        [ContextMenu("Reset User Data")]
        public void ResetUserData()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.Reset();
                UserDataManager.Instance.UserDataLog();
            }
        }
    }
}

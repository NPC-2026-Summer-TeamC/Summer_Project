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

            // Keypad 0 / Alpha 0: 데이터 초기화
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                Debug.Log("[DebugHelper] Alpha0 pressed -> Reset Data");
                if (UserDataManager.Instance != null)
                {
                    UserDataManager.Instance.Reset();
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

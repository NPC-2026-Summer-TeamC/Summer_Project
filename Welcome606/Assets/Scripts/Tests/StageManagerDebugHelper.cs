using UnityEngine;
using Welcome606.Managers;

namespace Welcome606.Tests
{
    /// <summary>
    /// StageManager 퍼즐 클리어 및 UserDataManager 저장 동작을 검증하기 위한 런타임 테스트 헬퍼입니다.
    /// 'C' 키 누름 시 퍼즐 클리어를 트리거하며, 'R' 키 누름 시 맵 선택 씬으로 복귀합니다.
    /// </summary>
    public class StageManagerDebugHelper : MonoBehaviour
    {
        [Header("Test Key Bindings")]
        [SerializeField] private KeyCode clearStageKey = KeyCode.C;
        [SerializeField] private KeyCode returnToMapKey = KeyCode.R;

        private void Update()
        {
            if (Input.GetKeyDown(clearStageKey))
            {
                if (StageManager.Instance != null)
                {
                    Debug.Log("[StageManagerDebugHelper] Debug Key 'C' Pressed: Triggering CompleteStage()");
                    StageManager.Instance.CompleteStage();
                }
                else
                {
                    Debug.LogWarning("[StageManagerDebugHelper] StageManager.Instance가 존재하지 않습니다.");
                }
            }

            if (Input.GetKeyDown(returnToMapKey))
            {
                if (StageManager.Instance != null)
                {
                    Debug.Log("[StageManagerDebugHelper] Debug Key 'R' Pressed: Triggering ReturnToMapLobby()");
                    StageManager.Instance.ReturnToMapLobby();
                }
                else
                {
                    Debug.LogWarning("[StageManagerDebugHelper] StageManager.Instance가 존재하지 않습니다.");
                }
            }
        }
    }
}

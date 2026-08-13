using UnityEngine;
using UnityEngine.EventSystems; // 🔴 UI 위 클릭인지 확인하기 위해 추가

// 🔴 클릭하면 지정된 EventID의 대사를 실행하는 범용 컴포넌트.
// 콜라이더(Collider/Collider2D)가 붙어있는 오브젝트에 이 스크립트를 추가하면 됨.
// (OnMouseDown은 콜라이더가 있어야 작동함! 없으면 클릭해도 반응 없음)
public class InteractionController : MonoBehaviour
{
    [Header("실행할 대사")]
    [Tooltip("클릭 시 실행할 대사의 EventID (엑셀 A열 값과 정확히 일치해야 함)")]
    public string eventID;

    private void OnMouseDown()
    {
        // 🔴 UI(로그창/설정창/버튼 등) 위를 클릭한 거라면, 그 뒤에 깔린 이 오브젝트까지
        // 같이 클릭 판정이 나지 않도록 여기서 걸러냄
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning($"[InteractionController] {gameObject.name}: 씬에서 DialogueManager.Instance를 찾을 수 없습니다.");
            return;
        }

        // 🔴 이미 다른 대사가 재생 중이면 무시 (연속 클릭으로 대사가 캔슬/재시작되는 것 방지)
        if (DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        if (string.IsNullOrEmpty(eventID))
        {
            Debug.LogWarning($"[InteractionController] {gameObject.name}: Event ID가 비어있습니다.");
            return;
        }

        DialogueManager.Instance.StartDialogue(eventID);
    }
}
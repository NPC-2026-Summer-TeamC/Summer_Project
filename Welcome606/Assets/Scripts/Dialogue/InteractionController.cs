using UnityEngine;

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
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning($"[InteractionController] {gameObject.name}: 씬에서 DialogueManager.Instance를 찾을 수 없습니다.");
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
using UnityEngine;
using UnityEngine.UI;

// 🔴 LogModal_PF(로그창) 안의 LogScrollView를 관리하는 스크립트.
// DialogueManager가 "새 대사 로그로 남겨줘" 하고 부르면, 이 스크립트가 실제로
// 프리팹을 만들어서 Content에 쌓고, 스크롤을 맨 아래로 내려줌.
public class LogModalController : MonoBehaviour
{
    [Header("연결")]
    public GameObject logItemPrefab; // 로그 한 줄짜리 프리팹 (LogItemController 붙어있는 것)
    public Transform contentParent;  // LogScrollView 안의 "Content" 오브젝트
    public ScrollRect scrollRect;    // LogScrollView의 ScrollRect 컴포넌트

    // 🔴 로그창이 켜질 때마다(비활성 상태였다가 다시 활성화될 때) 맨 아래(최신 대사)로 스크롤.
    // 로그창이 닫혀있는 동안 쌓인 대사들은 레이아웃 계산이 제대로 안 먹혔을 수 있어서,
    // 열리는 시점에 한 번 더 맞춰줌.
    private void OnEnable()
    {
        RefreshLayoutAndScrollToBottom();
    }

    public void AddLogEntry(string characterName, string dialogue)
    {
        if (logItemPrefab == null || contentParent == null)
        {
            Debug.LogWarning("[LogModalController] Log Item Prefab 또는 Content Parent가 연결 안 되어 있습니다.");
            return;
        }

        GameObject item = Instantiate(logItemPrefab, contentParent);
        // 🔴 GetComponentInChildren로 바꿔서, 스크립트가 프리팹의 자식 오브젝트에 붙어있어도 찾아낼 수 있도록 함
        LogItemController controller = item.GetComponentInChildren<LogItemController>();

        if (controller != null)
        {
            controller.SetLogData(characterName, dialogue);
        }
        else
        {
            Debug.LogWarning("[LogModalController] 로그 프리팹에 LogItemController 스크립트가 없습니다.");
        }

        // 🔴 코루틴(한 프레임 대기) 없이 바로 처리.
        // 로그창이 꺼져있는(비활성) 상태에서도 대사가 계속 로그에 쌓여야 하는데,
        // 코루틴은 오브젝트가 켜져있어야만 실행되기 때문에 즉시 실행 방식으로 변경.
        RefreshLayoutAndScrollToBottom();
    }

    private void RefreshLayoutAndScrollToBottom()
    {
        if (contentParent != null)
        {
            RectTransform contentRect = contentParent.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
        }

        if (scrollRect != null)
        {
            // 0 = 스크롤 맨 아래, 1 = 스크롤 맨 위
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
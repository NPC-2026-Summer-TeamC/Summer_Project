using UnityEngine;
using TMPro;

// 🔴 로그 아이템 프리팹(LogItem_PF)에 붙일 스크립트.
// 이름 + 대사 텍스트를 채워주는 역할만 함.
public class LogItemController : MonoBehaviour
{
    [Header("텍스트 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public void SetLogData(string characterName, string dialogue)
    {
        if (nameText != null)
        {
            // 이름이 없는 줄(예: 시스템/지문)이면 이름 칸은 비워둠
            nameText.text = string.IsNullOrEmpty(characterName) ? "" : characterName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = dialogue;
        }
    }
}
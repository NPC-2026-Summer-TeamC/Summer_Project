using UnityEngine;
using UnityEngine.UI;

public class SkipButtonController : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public Button skipButton;

    private void Awake()
    {
        if (skipButton != null)
        {
            // 클릭 시 단 1회 스킵 로직 실행 
            skipButton.onClick.AddListener(OnSkipButtonClicked);
        }
    }

    private void OnSkipButtonClicked()
    {
        if (dialogueManager != null)
        {
            dialogueManager.ExecuteSkip();
        }
    }
}
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    
    [Space(15)]
    [Header("데이터 설정")]
    public TextAsset dialogueFile;
    
    private DialogueParser parser;
    private DialogueData[] dialogues;
    private int currentIndex = 0;

    private void Awake()
    {
        parser = GetComponent<DialogueParser>();
        if (parser == null)
            parser = gameObject.AddComponent<DialogueParser>();

        LoadDialogueData();
    }

    // Update문에서 마우스를 직접 감지하던 코드를 통째로 제거했습니다.
    // 이제 오직 투명 버튼의 OnClick 이벤트나 명시적인 함수 호출로만 대사가 넘어갑니다.

    private void LoadDialogueData()
    {
        if (dialogueFile == null) return;

        dialogues = parser.ParseTextAsset(dialogueFile);
        
        if (dialogues != null && dialogues.Length > 0)
        {
            currentIndex = 0;
            DisplayCurrentDialogue();
        }
    }

    private void DisplayCurrentDialogue()
    {
        if (currentIndex < dialogues.Length)
        {
            DialogueData currentData = dialogues[currentIndex];
            
            nameText.text = string.IsNullOrEmpty(currentData.characterName) ? "" : currentData.characterName;
            
            if (!string.IsNullOrEmpty(currentData.dialogue))
            {
                dialogText.text = currentData.dialogue;
            }
            else if (!string.IsNullOrEmpty(currentData.soundEffect))
            {
                dialogText.text = currentData.soundEffect;
            }
            else
            {
                dialogText.text = "";
            }
        }
    }

    // 화면 클릭 시 호출되는 함수 (이 함수를 투명 버튼의 OnClick에 연결합니다)
    public void OnScreenClicked()
    {
        if (dialogues == null) return;

        if (currentIndex < dialogues.Length - 1)
        {
            currentIndex++;
            DisplayCurrentDialogue();
        }
    }
}
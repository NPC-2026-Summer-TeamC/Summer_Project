using UnityEngine;
using TMPro;
using System.Collections.Generic; // Dictionary, List 사용에 필수

public class DialogueManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    
    [Space(15)]
    [Header("데이터 설정")]
    public TextAsset dialogueFile;
    [Tooltip("게임 시작 시 자동 실행할 EventID (예: Prologue_01)")]
    public string defaultEventID = "Prologue_01";
    
    private DialogueParser parser;
    private Dictionary<string, List<DialogueData>> dialogueDatabase;
    private List<DialogueData> currentDialogueList;
    private int currentIndex = 0;

    private void Awake()
    {
        // 순수 C# 클래스이므로 new로 생성
        parser = new DialogueParser();
        LoadDialogueDatabase();
    }

    private void Start()
    {
        // 씬 시작 시 기본 EventID 대사 재생
        if (!string.IsNullOrEmpty(defaultEventID))
        {
            StartDialogue(defaultEventID);
        }
    }

    private void LoadDialogueDatabase()
    {
        if (dialogueFile == null) return;
        dialogueDatabase = parser.ParseTextAsset(dialogueFile);
    }

    // 외부(또는 내부)에서 EventID 키값으로 원하는 대화 묶음을 불러오는 핵심 함수
    public void StartDialogue(string eventID)
    {
        if (dialogueDatabase != null && dialogueDatabase.ContainsKey(eventID))
        {
            currentDialogueList = dialogueDatabase[eventID];
            currentIndex = 0;
            DisplayCurrentDialogue();
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] 데이터베이스에 '{eventID}' 키값이 존재하지 않습니다. 엑셀 A열을 확인하세요.");
        }
    }

    private void DisplayCurrentDialogue()
    {
        if (currentDialogueList != null && currentIndex < currentDialogueList.Count)
        {
            DialogueData currentData = currentDialogueList[currentIndex];
            
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

    public void OnScreenClicked()
    {
        if (currentDialogueList == null) return;

        if (currentIndex < currentDialogueList.Count - 1)
        {
            currentIndex++;
            DisplayCurrentDialogue();
        }
        else
        {
            // 대사 종료 시 초기화
            dialogText.text = "";
            nameText.text = "";
        }
    }
}
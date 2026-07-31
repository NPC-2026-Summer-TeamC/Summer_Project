using UnityEngine;
using UnityEngine.UI; // 🔴 배경 Image 컴포넌트 조작을 위해 추가
using TMPro;
using System.Collections; // 🔴 코루틴(타이핑 효과) 사용을 위해 추가
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Image dialogBackgroundImage; // 🔴 대화창 "배경"만 투명하게 만들기 위한 Image (CanvasGroup 대신 사용)

    [Space(15)]
    [Header("데이터 설정")]
    public TextAsset dialogueFile;
    [Tooltip("게임 시작 시 자동 실행할 EventID (예: Prologue_01)")]
    public string defaultEventID = "Prologue_01";

    // 🔴 로그창 연동. 비워두면 로그 기능 없이도 정상 작동함(선택 사항).
    [Header("로그창 연동")]
    public LogModalController logController;

    // 🔴 설정창에서 조절할 속도 변수
    [HideInInspector] public float typingSpeed = 0.05f; 
    [HideInInspector] public float autoSpeed = 3f;

    // 🔴 자동 진행(Auto Play) 사용 여부. 설정창의 토글(스위치)이 이 값을 켜고 끔.
    [HideInInspector] public bool isAutoPlay = false;

    private DialogueParser parser;
    private Dictionary<string, List<DialogueData>> dialogueDatabase;
    private List<DialogueData> currentDialogueList;
    private int currentIndex = 0;
    private string currentEventID; // 🔴 지금 재생 중인 이벤트ID (로그 중복 판별에 사용)

    // 🔴 "이벤트ID_인덱스" 조합으로 이미 로그에 남긴 대사인지 기억해두는 목록.
    // 이전 버튼으로 되돌아갔다가 다시 앞으로 가도, 이미 본 대사는 여기 걸려서 중복으로 안 쌓임.
    private HashSet<string> loggedDialogueKeys = new HashSet<string>();
    
    // 🔴 타이핑 제어용 변수
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    // 🔴 자동 진행 대기(다음 대사로 넘어가기 전 대기)용 코루틴
    private Coroutine autoPlayCoroutine;

    private void Awake()
    {
        parser = new DialogueParser();
        LoadDialogueDatabase();
    }

    private void Start()
    {
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

    public void StartDialogue(string eventID)
    {
        if (dialogueDatabase != null && dialogueDatabase.ContainsKey(eventID))
        {
            currentEventID = eventID; // 🔴 로그 중복 판별에 쓰기 위해 저장
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

            TryLogDialogue(currentData); // 🔴 아직 로그에 안 남긴 대사면 로그창에 추가

            // 🔴 기존 진행 중인 타이핑 멈춤
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            // 🔴 새 대사가 나오기 전, 이전에 예약돼 있던 자동 진행 대기는 취소
            if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);

            if (!string.IsNullOrEmpty(currentData.dialogue))
            {
                // 🔴 대사일 경우 타이핑 코루틴 실행
                typingCoroutine = StartCoroutine(TypeText(currentData.dialogue));
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

    // 🔴 현재 대사가 처음 보는 대사면 로그창에 추가하고, 이미 본 적 있으면 건너뜀.
    // (이전 버튼으로 되돌아갔다가 다시 앞으로 가는 경우 중복 방지)
    private void TryLogDialogue(DialogueData data)
    {
        if (logController == null) return;
        if (string.IsNullOrEmpty(data.dialogue)) return; // 실제 대사가 있는 줄만 로그에 남김 (음향효과/지문 전용 줄은 제외)

        string logKey = currentEventID + "_" + currentIndex;
        if (loggedDialogueKeys.Contains(logKey)) return; // 이미 로그에 남긴 대사 -> 중복 방지

        loggedDialogueKeys.Add(logKey);
        logController.AddLogEntry(data.characterName, data.dialogue);
    }

    // 🔴 텍스트 타이핑 효과 코루틴
    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogText.text = "";

        // 속도가 0.01 이하(즉시 출력)일 경우 타이핑 생략
        if (typingSpeed <= 0.011f)
        {
            dialogText.text = line;
            isTyping = false;
            TryStartAutoPlay(); // 🔴 타이핑 끝났으니 자동 진행 모드면 예약
            yield break;
        }

        foreach (char c in line.ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        TryStartAutoPlay(); // 🔴 타이핑 끝났으니 자동 진행 모드면 예약
    }

    // 🔴 타이핑이 끝난 직후 호출됨. 자동 진행 모드일 때만 실제로 대기 코루틴을 시작함.
    private void TryStartAutoPlay()
    {
        if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);

        if (isAutoPlay)
        {
            autoPlayCoroutine = StartCoroutine(AutoProceed());
        }
    }

    // 🔴 autoSpeed(초)만큼 기다렸다가 자동으로 다음 대사로 넘어감
    private IEnumerator AutoProceed()
    {
        yield return new WaitForSeconds(autoSpeed);
        OnScreenClicked();
    }

    // 🔴 자동 진행 기능을 켜고 끄는 함수. 설정창의 토글(스위치)이 이 함수를 호출함.
    public void SetAutoPlay(bool value)
    {
        isAutoPlay = value;

        if (!value && autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
    }

    // 🔴 "이전" 버튼용 함수. currentIndex를 1 줄이고 그 대사를 다시 보여줌.
    // 현재 이벤트의 첫 번째 대사(index 0)에서는 더 이상 갈 곳이 없으니 그냥 아무 반응 없이 막음.
    public void PrevDialogue()
    {
        if (currentDialogueList == null) return;

        if (currentIndex <= 0)
        {
            // 이 이벤트의 첫 대사임 -> 더 이전으로 못 감
            return;
        }

        // 🔴 자동 진행 대기 중이었다면 취소 (뒤로 가는 도중에 갑자기 앞으로 넘어가면 안 되니까)
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        currentIndex--;
        DisplayCurrentDialogue();
    }

    public void OnScreenClicked()
    {
        if (currentDialogueList == null) return;

        // 🔴 화면을 직접 클릭했다면, 예약돼 있던 자동 진행 대기는 일단 취소
        // (아래에서 타이핑 상태에 따라 다시 필요하면 예약함)
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        // 🔴 타이핑 중 클릭 시 전체 문장 즉시 출력
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogText.text = currentDialogueList[currentIndex].dialogue;
            isTyping = false;
            TryStartAutoPlay(); // 🔴 즉시 출력 후에도 자동 진행 모드면 다시 예약
            return;
        }

        if (currentIndex < currentDialogueList.Count - 1)
        {
            currentIndex++;
            DisplayCurrentDialogue();
        }
        else
        {
            dialogText.text = "";
            nameText.text = "";
        }
    }

    // 🔴 외부(설정창)에서 "배경만" 투명도를 조절할 수 있도록 열어둔 함수.
    // CanvasGroup이 아니라 배경 Image의 색상 중 알파(투명도)만 바꿔서,
    // 같은 부모 밑에 있는 대사 텍스트(NameText/DialogText)는 영향을 받지 않음.
    public void SetOpacity(float alpha)
    {
        if (dialogBackgroundImage != null)
        {
            Color color = dialogBackgroundImage.color;
            color.a = alpha;
            dialogBackgroundImage.color = color;
        }
    }
}
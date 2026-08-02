using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public enum SkipMode { ReadOnly, AllText }

    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Image dialogBackgroundImage;

    [Space(15)]
    [Header("데이터 설정")]
    public TextAsset dialogueFile;
    [Tooltip("게임 시작 시 자동 실행할 EventID (예: Prologue_01)")]
    public string defaultEventID = "Prologue_01";

    [Header("로그창 연동")]
    public LogModalController logController;

    [HideInInspector] public float typingSpeed = 0.05f;
    [HideInInspector] public float autoSpeed = 3f;
    [HideInInspector] public bool isAutoPlay = false;

    // 스킵 관련 상태값
    [HideInInspector] public SkipMode skipMode = SkipMode.ReadOnly;
    private bool forceInstantReveal = false;

    private const string ReadKeysPrefKey = "DialogueReadKeys";
    private HashSet<string> readDialogueKeys = new HashSet<string>();
    // 🔴 마지막으로 디스크에 저장(Save)한 이후로 새로 추가된 읽음 기록이 있는지 여부.
    // 이게 false면 SaveReadProgress()가 불려도 불필요한 디스크 접근을 하지 않도록 함.
    private bool hasUnsavedReadProgress = false;

    private DialogueParser parser;
    private Dictionary<string, List<DialogueData>> dialogueDatabase;
    private List<DialogueData> currentDialogueList;
    private int currentIndex = 0;
    private string currentEventID;
    private HashSet<string> loggedDialogueKeys = new HashSet<string>();

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private Coroutine autoPlayCoroutine;

    private void Awake()
    {
        parser = new DialogueParser();
        LoadDialogueDatabase();
        LoadReadKeys();
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(defaultEventID))
        {
            StartDialogue(defaultEventID);
        }
    }

    // 🔴 앱이 백그라운드로 전환될 때(일시정지) 저장 - 모바일에서 특히 중요
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveReadProgress();
    }

    // 🔴 앱이 종료될 때 저장
    private void OnApplicationQuit()
    {
        SaveReadProgress();
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
            currentEventID = eventID;
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

            TryLogDialogue(currentData);
            MarkAsRead(currentEventID + "_" + currentIndex);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);

            if (!string.IsNullOrEmpty(currentData.dialogue))
            {
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

    // 🔴 대화 종료 처리(텍스트 비우기)를 한 곳에서 관리 + 이 시점에 읽음 기록을 디스크에 저장
    private void EndDialogueDisplay()
    {
        dialogText.text = "";
        nameText.text = "";
        SaveReadProgress(); // 🔴 대화가 끝나는 시점 = 디스크 저장 트리거 포인트
    }

    private void TryLogDialogue(DialogueData data)
    {
        if (logController == null) return;
        if (string.IsNullOrEmpty(data.dialogue)) return;

        string logKey = currentEventID + "_" + currentIndex;
        if (loggedDialogueKeys.Contains(logKey)) return;

        loggedDialogueKeys.Add(logKey);
        logController.AddLogEntry(data.characterName, data.dialogue);
    }

    private void LoadReadKeys()
    {
        string saved = PlayerPrefs.GetString(ReadKeysPrefKey, "");
        if (string.IsNullOrEmpty(saved)) return;

        string[] keys = saved.Split(',');
        foreach (string key in keys)
        {
            if (!string.IsNullOrEmpty(key)) readDialogueKeys.Add(key);
        }
    }

    // 🔴 "읽었다"는 사실은 메모리(HashSet)에만 즉시 반영. 디스크 저장(Save)은 여기서 하지 않음.
    // PlayerPrefs.SetString 자체는 메모리 상의 PlayerPrefs 캐시에 쓰는 거라 비교적 가벼움 -
    // 비용이 큰 건 실제 디스크에 내려쓰는 Save() 쪽이라, 그걸 매번 호출하지 않도록 분리함.
    private void MarkAsRead(string key)
    {
        if (readDialogueKeys.Contains(key)) return;

        readDialogueKeys.Add(key);
        PlayerPrefs.SetString(ReadKeysPrefKey, string.Join(",", readDialogueKeys));
        hasUnsavedReadProgress = true;
    }

    // 🔴 실제 디스크 저장(PlayerPrefs.Save())은 이 함수를 통해서만, 특정 트리거 시점에만 호출함.
    // (대화 종료 / 씬 전환 / 앱 일시정지 / 앱 종료)
    public void SaveReadProgress()
    {
        if (!hasUnsavedReadProgress) return; // 저장할 새 내용이 없으면 디스크 접근 자체를 생략

        PlayerPrefs.Save();
        hasUnsavedReadProgress = false;
    }

    private bool IsAlreadyRead(string eventID, int index)
    {
        return readDialogueKeys.Contains(eventID + "_" + index);
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogText.text = "";

        if (typingSpeed <= 0.011f || forceInstantReveal)
        {
            dialogText.text = line;
            isTyping = false;
            TryStartAutoPlay();
            yield break;
        }

        foreach (char c in line.ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        TryStartAutoPlay();
    }

    private void TryStartAutoPlay()
    {
        if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);

        if (isAutoPlay)
        {
            autoPlayCoroutine = StartCoroutine(AutoProceed());
        }
    }

    private IEnumerator AutoProceed()
    {
        yield return new WaitForSeconds(autoSpeed);
        OnScreenClicked();
    }

    public void SetAutoPlay(bool value)
    {
        isAutoPlay = value;

        if (!value && autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
    }

    public void SetSkipMode(SkipMode mode)
    {
        skipMode = mode;
    }

    public void PrevDialogue()
    {
        if (currentDialogueList == null) return;

        if (currentIndex <= 0)
        {
            return;
        }

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

        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogText.text = currentDialogueList[currentIndex].dialogue;
            isTyping = false;
            TryStartAutoPlay();
            return;
        }

        if (currentIndex < currentDialogueList.Count - 1)
        {
            currentIndex++;
            DisplayCurrentDialogue();
        }
        else
        {
            EndDialogueDisplay(); // 🔴 대화 종료 지점 -> 여기서 읽음 기록 디스크 저장
        }
    }

    public void SetOpacity(float alpha)
    {
        if (dialogBackgroundImage != null)
        {
            Color color = dialogBackgroundImage.color;
            color.a = alpha;
            dialogBackgroundImage.color = color;
        }
    }

    // 단발성 스킵 버튼 클릭 시 호출되는 함수
    public void ExecuteSkip()
    {
        if (currentDialogueList == null) return;

        // 1. 진행 중인 오토플레이 정지
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        // 2. 타이핑 중이었다면 대사를 즉시 띄우고 상태 해제 (이번 클릭은 여기서 끝)
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogText.text = currentDialogueList[currentIndex].dialogue;
            isTyping = false;
            return;
        }

        // 마지막 대사였다면 대화창 비우기 + 저장
        if (currentIndex >= currentDialogueList.Count - 1)
        {
            EndDialogueDisplay();
            return;
        }

        // 3. 설정된 스킵 모드에 따라 분기
        if (skipMode == SkipMode.ReadOnly)
        {
            // 🔴 [읽은 텍스트만] 모드: 이미 읽은 대사는 쭉 건너뛰고, 처음 보는 대사에서 멈춤
            while (currentIndex < currentDialogueList.Count - 1)
            {
                if (IsAlreadyRead(currentEventID, currentIndex + 1))
                {
                    // 다음 대사가 이미 읽은 대사면 즉시 띄우고 계속 다음 줄로 루프
                    currentIndex++;
                    forceInstantReveal = true;
                    DisplayCurrentDialogue();
                    forceInstantReveal = false;
                }
                else
                {
                    // 다음 대사가 안 읽은 대사면 거기까지만 가고 멈춤 (정상 타이핑)
                    currentIndex++;
                    DisplayCurrentDialogue();
                    break;
                }
            }
        }
        else if (skipMode == SkipMode.AllText)
        {
            // 🔴 [모든 텍스트] 모드: 읽음 여부와 관계없이 끝까지 전부 진행한 뒤 대화 종료 처리
            while (currentIndex < currentDialogueList.Count - 1)
            {
                currentIndex++;
                forceInstantReveal = true;
                DisplayCurrentDialogue();
                forceInstantReveal = false;
            }

            EndDialogueDisplay(); // 끝까지 다 진행했으니 대화 종료 처리
        }
    }
}
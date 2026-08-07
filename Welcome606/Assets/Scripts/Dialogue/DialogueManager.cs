using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // 🔴 대화 종료 이벤트(UnityEvent) 사용을 위해 추가
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    // 🔴 씬 단위 싱글턴. 같은 씬 안에서는 DialogueManager.Instance로 어디서든 바로 접근 가능.
    // (씬마다 프리팹을 개별 배치하는 구조라서 DontDestroyOnLoad는 쓰지 않음 - 씬이 바뀌면
    // 그 씬의 새 DialogueManager가 Awake에서 자기 자신을 Instance로 등록함)
    public static DialogueManager Instance { get; private set; }

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

    // 🔴 대화가 끝났을 때 외부(퀘스트 시스템, 상호작용 오브젝트 등)로 신호를 보내는 이벤트.
    // 인스펙터에서 On Dialogue Ended (+) 눌러서 원하는 함수를 등록해서 쓸 수 있음.
    [Header("대화 종료 이벤트")]
    public UnityEvent OnDialogueFinished;

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
        // 🔴 같은 씬에 DialogueManager가 실수로 두 개 이상 있는 경우를 대비한 안전장치
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[DialogueManager] 씬에 DialogueManager가 이미 존재합니다. ({gameObject.name})은(는) 중복이라 비활성화합니다.");
            enabled = false;
            return;
        }

        Instance = this;

        parser = new DialogueParser();
        LoadDialogueDatabase();
        LoadReadKeys();
    }

    private void OnDestroy()
    {
        // 🔴 씬이 바뀌거나 이 오브젝트가 파괴될 때, 내가 등록해둔 Instance였다면 정리
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // 🔴 이전 실행에서 대화 도중 비정상 종료(크래시/강제 종료)된 기록이 있으면,
        // defaultEventID보다 우선해서 그 대사를 강제로 다시 실행함
        string inProgressEventID = PlayerPrefs.GetString(InProgressEventIDKey, "");
        if (!string.IsNullOrEmpty(inProgressEventID))
        {
            Debug.Log($"[DialogueManager] 이전에 비정상 종료된 대화를 감지하여 재실행합니다: {inProgressEventID}");
            StartDialogue(inProgressEventID);
            return;
        }

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

    // 🔴 지금 대사가 재생 중인지 외부에서 확인할 수 있는 프로퍼티 (중복 실행 방지용)
    public bool IsDialogueActive { get; private set; } = false;

    // 🔴 "재생 중이던 대사" 크래시 복구용 PlayerPrefs 키
    private const string InProgressEventIDKey = "DialogueInProgressEventID";

    public void StartDialogue(string eventID)
    {
        // 🔴 대화 시작 시 스스로 자기 오브젝트를 켬 (모달을 여는 쪽에서 따로 SetActive(true) 안 해줘도 되게)
        gameObject.SetActive(true);

        if (dialogueDatabase != null && dialogueDatabase.ContainsKey(eventID))
        {
            IsDialogueActive = true; // 🔴 대사 재생 시작
            currentEventID = eventID;
            currentDialogueList = dialogueDatabase[eventID];
            currentIndex = 0;

            // 🔴 재생 시작한 EventID를 즉시 디스크에 저장 (크래시 복구용이라 지연 저장하면 의미 없음.
            // 대사 "시작" 시점에만 한 번 호출되는 거라 자주 발생하는 이벤트가 아니라 성능 부담도 적음)
            PlayerPrefs.SetString(InProgressEventIDKey, eventID);
            PlayerPrefs.Save();

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

    // 🔴 대화 종료 처리(텍스트 비우기)를 한 곳에서 관리 + 이 시점에 읽음 기록을 디스크에 저장 + 외부에 종료 신호 + 모달 자동 비활성화
    private void EndDialogueDisplay()
    {
        dialogText.text = "";
        nameText.text = "";
        IsDialogueActive = false; // 🔴 대사 재생 종료

        // 🔴 정상적으로 끝났으니 "재생 중이던 대사" 기록을 지움 (다음 실행 때 강제 재실행 안 되도록)
        PlayerPrefs.DeleteKey(InProgressEventIDKey);
        PlayerPrefs.Save();

        SaveReadProgress(); // 🔴 대화가 끝나는 시점 = 디스크 저장 트리거 포인트
        OnDialogueFinished?.Invoke(); // 🔴 외부 시스템에 "대화 끝났다" 신호 전달
        gameObject.SetActive(false); // 🔴 대화창 자동으로 끄기 (다음 StartDialogue 호출 시 스스로 다시 켜짐)
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
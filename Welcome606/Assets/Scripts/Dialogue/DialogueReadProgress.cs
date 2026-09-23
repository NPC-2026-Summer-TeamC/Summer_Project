using UnityEngine;
using System.Collections.Generic;

// 🔴 DialogueManager에서 분리한 클래스.
// "읽은 대사 기록"이랑 "강제종료 복구용 저장"과 관련된 PlayerPrefs 접근을 전부 여기서만 담당함.
// DialogueManager는 이 클래스를 통해서만 관련 데이터를 읽고 쓰고, 직접 PlayerPrefs를 건드리지 않음.
// (MonoBehaviour가 아닌 순수 C# 클래스 - DialogueParser랑 같은 패턴)
public class DialogueReadProgress
{
    private const string ReadKeysPrefKey = "DialogueReadKeys";
    private const string InProgressEventIDKey = "DialogueInProgressEventID";

    private HashSet<string> readDialogueKeys = new HashSet<string>();

    // 🔴 마지막으로 디스크에 저장(Save)한 이후로 새로 추가된 읽음 기록이 있는지 여부.
    // 이게 false면 SaveReadProgress()가 불려도 불필요한 디스크 접근을 하지 않도록 함.
    private bool hasUnsavedReadProgress = false;

    public DialogueReadProgress()
    {
        LoadReadKeys();
    }

    // 🔴 게임 시작 시 PlayerPrefs에서 "이미 읽은 대사" 목록을 불러옴
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

    public bool IsAlreadyRead(string eventID, int index)
    {
        return readDialogueKeys.Contains(eventID + "_" + index);
    }

    // 🔴 "읽었다"는 사실은 메모리(HashSet)에만 즉시 반영. 디스크 저장(Save)은 여기서 하지 않음.
    // PlayerPrefs.SetString 자체는 메모리 상의 PlayerPrefs 캐시에 쓰는 거라 비교적 가벼움 -
    // 비용이 큰 건 실제 디스크에 내려쓰는 Save() 쪽이라, 그걸 매번 호출하지 않도록 분리함.
    public void MarkAsRead(string key)
    {
        if (readDialogueKeys.Contains(key)) return;

        readDialogueKeys.Add(key);
        PlayerPrefs.SetString(ReadKeysPrefKey, string.Join(",", readDialogueKeys));
        hasUnsavedReadProgress = true;
    }

    // 🔴 실제 디스크 저장(PlayerPrefs.Save())은 이 함수를 통해서만, 특정 트리거 시점에만 호출함.
    // (대화 종료 / 앱 일시정지 / 앱 종료)
    public void SaveReadProgress()
    {
        if (!hasUnsavedReadProgress) return; // 저장할 새 내용이 없으면 디스크 접근 자체를 생략

        PlayerPrefs.Save();
        hasUnsavedReadProgress = false;
    }

    // 🔴 강제종료 복구용: 지금 재생 중인 EventID를 즉시 디스크에 저장
    // (크래시 복구용이라 지연 저장하면 의미 없음. 대사 "시작" 시점에만 호출되니 성능 부담도 적음)
    public void SaveInProgressEventID(string eventID)
    {
        PlayerPrefs.SetString(InProgressEventIDKey, eventID);
        PlayerPrefs.Save();
    }

    // 🔴 정상적으로 대화가 끝났을 때, 강제종료 복구 기록을 지움
    public void ClearInProgressEventID()
    {
        PlayerPrefs.DeleteKey(InProgressEventIDKey);
        PlayerPrefs.Save();
    }

    // 🔴 게임 시작 시, 이전에 못 끝낸 대사가 있는지 확인
    public string GetInProgressEventID()
    {
        return PlayerPrefs.GetString(InProgressEventIDKey, "");
    }
}
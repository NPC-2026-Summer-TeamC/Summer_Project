using System.Collections.Generic;
using UnityEngine;

// 1. MonoBehaviour 상속 제거 (순수 C# 클래스)
public class DialogueParser
{
    // 2. Dictionary<string, List<DialogueData>> 구조로 반환
    public Dictionary<string, List<DialogueData>> ParseTextAsset(TextAsset tsvFile)
    {
        Dictionary<string, List<DialogueData>> dialogueDict = new Dictionary<string, List<DialogueData>>();

        if (tsvFile == null) return dialogueDict;

        // 필수 반영: 개행 끝 \r 문자 제거
        string[] lines = tsvFile.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

        // 🔴 현재 처리 중인 이벤트 ID를 기억해두는 변수.
        // A열(EventID)이 비어있는 줄은 이 값을 그대로 이어받아서 같은 이벤트로 취급함.
        string currentEventID = null;

        for (int i = 1; i < lines.Length; i++)
        {
            // 🔴 주의: 줄 전체를 Trim()하면 앞/뒤에 있는 빈 칸(탭)까지 같이 지워져서
            // "EventID 빈칸, 캐릭터 빈칸, 대사만 있음" 같은 줄의 칸 순서가 밀려버림.
            // 그래서 줄바꿈 여부 확인만 공백 체크로 하고, 실제 자르기는 원본 그대로 사용.
            string rawLine = lines[i];
            if (string.IsNullOrWhiteSpace(rawLine)) continue; // 완전히 빈 줄(이벤트 사이 여백)은 건너뜀

            string[] columns = rawLine.Split('\t');
            if (columns.Length < 1) continue;

            string firstColumn = columns[0].Trim();

            // 🔴 A열이 "end"면 현재 이벤트를 마감하고, 이 줄 자체는 대사로 취급하지 않음
            if (firstColumn.Equals("end", System.StringComparison.OrdinalIgnoreCase))
            {
                currentEventID = null;
                continue;
            }

            string eventID;
            if (!string.IsNullOrEmpty(firstColumn))
            {
                // 🔴 A열에 값이 있으면 새 이벤트 시작 (기존 방식처럼 매 줄 반복되어도 동일하게 동작)
                eventID = firstColumn;
                currentEventID = eventID;
            }
            else
            {
                // 🔴 A열이 비어있으면 바로 위에서 진행 중이던 이벤트에 이어붙임
                if (string.IsNullOrEmpty(currentEventID))
                {
                    // 아직 어떤 이벤트도 시작되지 않았는데 A열이 비어있으면 잘못된 데이터이므로 건너뜀
                    Debug.LogWarning($"[DialogueParser] {i + 1}번째 줄: EventID 없이 대사가 시작돼서 건너뜁니다.");
                    continue;
                }
                eventID = currentEventID;
            }

            DialogueData data = new DialogueData();

            // 필수 반영: 하드코딩 제거 및 TSV 컬럼 순서 바인딩 (1:캐릭터, 2:대사, 3:음향, 4:연출)
            data.characterName = columns.Length > 1 ? columns[1].Trim() : "";
            data.dialogue = columns.Length > 2 ? columns[2].Trim() : "";
            data.soundEffect = columns.Length > 3 ? columns[3].Trim() : "";
            data.screenEffect = columns.Length > 4 ? columns[4].Trim() : "";

            // EventID별로 그룹화
            if (!dialogueDict.ContainsKey(eventID))
            {
                dialogueDict[eventID] = new List<DialogueData>();
            }

            dialogueDict[eventID].Add(data);
        }

        return dialogueDict;
    }
}
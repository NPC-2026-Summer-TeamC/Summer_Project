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
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split('\t');
            if (columns.Length < 1) continue;

            // 0번째 열은 EventID
            string eventID = columns[0].Trim();
            if (string.IsNullOrEmpty(eventID)) continue;

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
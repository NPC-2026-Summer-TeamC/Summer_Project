using System.Collections.Generic;
using UnityEngine;

public class DialogueParser : MonoBehaviour
{
    public DialogueData[] ParseTextAsset(TextAsset tsvFile)
    {
        List<DialogueData> dialogueList = new List<DialogueData>();
        
        if (tsvFile == null) return null;

        string[] lines = tsvFile.text.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split('\t');
            DialogueData data = new DialogueData();

            // 탭으로 나뉘었을 때 첫 번째 칸에 들어간 텍스트가 캐릭터 이름인지,
            // 아니면 탭이 생략되어 효과음/대사가 첫 번째 칸으로 밀려 들어온 것인지 판별합니다.
            string firstCol = columns.Length > 0 ? columns[0].Trim() : "";

            // 만약 첫 번째 칸에 적힌 내용이 대사나 효과음 형태이거나(예: '쿵', '드르륵' 등),
            // 혹은 두 번째 칸이 비어있지 않다면 밀림 현상이 발생한 것입니다.
            // 여기서는 엑셀 구조상 캐릭터 이름으로 지정된 정해진 이름이 아니라면 
            // 캐릭터 이름 칸이 비어있던(탭이 생략된) 줄로 판단하여 데이터를 올바르게 재배치합니다.
            
            bool isCharacterName = (firstCol == "주인공" || firstCol == "플레이어" || firstCol == "npc" /* 필요한 캐릭터 이름 추가 가능 */);

            if (!isCharacterName && columns.Length == 1)
            {
                // 탭 없이 텍스트만 한 개 있는 경우 (효과음/연출)
                data.characterName = "";
                data.dialogue = "";
                data.soundEffect = firstCol;
                data.screenEffect = "";
            }
            else if (!isCharacterName && columns.Length >= 2)
            {
                // 캐릭터 칸이 비어있어서 두 번째 칸이 첫 번째 칸으로 밀려온 경우
                data.characterName = "";
                data.dialogue = "";
                data.soundEffect = columns[0].Trim();
                data.screenEffect = columns[1].Trim();
            }
            else
            {
                // 정상적인 대사 줄인 경우
                data.characterName = firstCol;
                data.dialogue = columns.Length > 1 ? columns[1].Trim() : "";
                data.soundEffect = columns.Length > 2 ? columns[2].Trim() : "";
                data.screenEffect = columns.Length > 3 ? columns[3].Trim() : "";
            }

            dialogueList.Add(data);
        }

        return dialogueList.ToArray();
    }
}
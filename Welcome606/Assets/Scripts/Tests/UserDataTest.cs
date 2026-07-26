using UnityEngine;

public class UserDataTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== 1단계: 초기 상태 로드 및 출력 ===");
        PrintUserData();

        Debug.Log("=== 2단계: 스테이지 클리어 처리 및 자동 저장 ===");
        // 챕터 0의 스테이지 0, 챕터 1의 스테이지 2 클리어
        UserDataManager.Instance.ClearStage(0, 0);
        UserDataManager.Instance.ClearStage(1, 2);

        // 아이템 획득 테스트
        UserDataManager.Instance.CollectItem(0);
        UserDataManager.Instance.CollectItem(3);
        Debug.Log("=== 3단계: PlayerPrefs에 저장된 생 JSON 데이터 확인 ===");
        if (PlayerPrefs.HasKey("UserDataJson"))
        {
            string rawJson = PlayerPrefs.GetString("UserDataJson");
            Debug.Log($"[저장된 JSON]: {rawJson}");
        }
        else
        {
            Debug.LogError("PlayerPrefs에 UserDataJson 키가 존재하지 않습니다!");
        }

        Debug.Log("=== 4단계: 메모리 데이터를 초기화한 후 디스크에서 재로드 ===");
        // 현재 인스턴스의 메모리 데이터를 임의로 초기화
        UserDataManager.Instance.userData = new UserData();
        Debug.Log("메모리 데이터 초기화 직후:");
        PrintUserData();

        // 디스크로부터 다시 불러오기
        UserDataManager.Instance.Load();
        Debug.Log("다시 로드한 후:");
        PrintUserData();
    }

    private void PrintUserData()
    {
        var data = UserDataManager.Instance.userData;
        for (int c = 0; c < data.chapters.Length; c++)
        {
            for (int s = 0; s < data.chapters[c].stages.Length; s++)
            {
                if (data.chapters[c].stages[s])
                {
                    Debug.Log($"👉 [클리어 확인] Chapter {c}, Stage {s}: {data.chapters[c].stages[s]}");
                }
            }
        }
        for (int c = 0; c < data.collectedItems.Length; c++)
        {
            Debug.Log($"👉 [아이템 획득] Chapter {c}: {data.collectedItems[c]}");
        }
    }
}

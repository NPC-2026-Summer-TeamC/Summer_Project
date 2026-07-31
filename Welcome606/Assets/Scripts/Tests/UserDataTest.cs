using UnityEngine;

public class UserDataTest : MonoBehaviour
{
    private void Start()
    {
        UserDataManager.Instance.Reset();
        UserDataManager.Instance.UserDataLog();
        
        Debug.Log("=== 1챕터 2스테이지 클리어 ===");
        UserDataManager.Instance.ClearStage(1, 2);
        UserDataManager.Instance.UserDataLog();

        Debug.Log("=== 1챕터 3스테이지 클리어 ===");
        UserDataManager.Instance.ClearStage(1, 3);
        UserDataManager.Instance.UserDataLog();
        
        Debug.Log("=== 2챕터 3스테이지 클리어 ===");
        UserDataManager.Instance.ClearStage(2, 3);
        UserDataManager.Instance.UserDataLog();

        Debug.Log("=== 3챕터 2스테이지 클리어 ===");
        UserDataManager.Instance.ClearStage(3, 2);
        UserDataManager.Instance.UserDataLog();
    }
}

using UnityEngine;

public class DialogUIManager : MonoBehaviour
{
    [Header("모달창 연결")]
    [SerializeField] private GameObject logModal;      // 로그창 오브젝트
    [SerializeField] private GameObject settingModal;  // 설정창 오브젝트

    private void Awake()
    {
        // 씬 시작 시 모달창들은 기본적으로 비활성화
        CloseAllModals();
    }

    // [로그] 버튼 클릭 시 호출
    public void OpenLogModal()
    {
        if (logModal != null)
        {
            logModal.SetActive(true);
        }
    }

    // [설정] 버튼 클릭 시 호출
    public void OpenSettingModal()
    {
        if (settingModal != null)
        {
            settingModal.SetActive(true);
        }
    }

    // 모든 모달창 닫기 (배경 터치, 닫기/확인 버튼 클릭 시 호출)
    public void CloseAllModals()
    {
        if (logModal != null) logModal.SetActive(false);
        if (settingModal != null) settingModal.SetActive(false);
    }
}
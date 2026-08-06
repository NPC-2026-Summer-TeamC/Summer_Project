using System;
using UnityEngine;
using UnityEngine.UI;
using Welcome606.Managers;

namespace Welcome606.UI
{
    /// <summary>
    /// ItemPhotoScene의 수집품(포토) 5종 정적 슬롯 상태 갱신, 수집 아이템 클릭 시 Dialog 대사 모달 연동 및 씬 전환 컨트롤러.
    /// </summary>
    public class ItemPhotoController : MonoBehaviour
    {
        [Header("Close / Back Button")]
        [SerializeField] private Button closeButton;

        [Header("Item Slots (1~5 Chapter Items)")]
        [SerializeField] private Button[] itemButtons = new Button[5];             // 씬에 정적 배치된 5개 수집 아이템 버튼
        [SerializeField] private GameObject[] itemLockedOverlays = new GameObject[5]; // 씬에 정적 배치된 미해금 잠금 오버레이

        [Header("Dialog Modal")]
        [SerializeField] private GameObject dialogModal;        // 수집품 선택 시 띄울 스토리 대사 Dialog 모달창
        [SerializeField] private DialogueManager dialogueManager; // DialogueManager (대사 데이터 연동 시 사용)

        [Header("Audio Clips (Optional)")]
        [SerializeField] private AudioClip selectSfxClip;           // 아이템 클릭 SFX
        [SerializeField] private AudioClip closeSfxClip;            // 닫기 버튼 클릭 SFX

        private void OnEnable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged += RefreshUI;
            }
            RefreshUI();
        }

        private void OnDisable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged -= RefreshUI;
            }
        }

        private void Start()
        {
            // 씬 시작 시 Dialog 모달 기본 비활성화(숨김)
            if (dialogModal != null)
            {
                dialogModal.SetActive(false);
            }

            BindEventListeners();
            RefreshUI();
        }


        private void BindEventListeners()
        {
            // 닫기/뒤로가기 버튼
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // 5개 수집 아이템 버튼 클릭 리스너 등록
            if (itemButtons != null)
            {
                for (int i = 0; i < itemButtons.Length; i++)
                {
                    int chapterIndex = i + 1; // 1~5 챕터
                    if (itemButtons[i] != null)
                    {
                        itemButtons[i].onClick.RemoveAllListeners();
                        itemButtons[i].onClick.AddListener(() => OnItemClicked(chapterIndex));
                    }
                }
            }
        }

        /// <summary>
        /// UserDataManager 진행도(MaxCollectionItem)에 따라 정적으로 배치된 5개 포토 슬롯의 해금/잠금 상태를 갱신합니다.
        /// </summary>
        public void RefreshUI()
        {
            int maxCollected = UserDataManager.Instance != null ? UserDataManager.Instance.MaxCollectionItem : 0;

            if (itemButtons != null)
            {
                for (int i = 0; i < itemButtons.Length; i++)
                {
                    int chapter = i + 1;
                    bool isUnlocked = chapter <= maxCollected;

                    if (itemButtons[i] != null)
                    {
                        itemButtons[i].interactable = isUnlocked;
                    }

                    if (itemLockedOverlays != null && i < itemLockedOverlays.Length && itemLockedOverlays[i] != null)
                    {
                        itemLockedOverlays[i].SetActive(!isUnlocked);
                    }
                }
            }
        }

        /// <summary>
        /// 수집 아이템 슬롯 클릭 시 스토리 대사 Dialog 모달 연동
        /// </summary>
        public void OnItemClicked(int chapterIndex)
        {
            Debug.Log($"[ItemPhotoController] 수집 아이템 선택 - {chapterIndex}챕터 (Dialog 모달 출력)");

            // 1. Dialog 모달 GameObject 활성화
            if (dialogModal != null)
            {
                dialogModal.SetActive(true);
            }

            // 2. DialogueManager 연동 시 해당 수집품 대사 EventID 실행
            if (dialogueManager != null)
            {
                string eventId = $"ItemPhoto_Chapter{chapterIndex}";
                dialogueManager.StartDialogue(eventId);
            }


        }

        /// <summary>
        /// 닫기 버튼 클릭 시 이전 맵으로 페이드 비동기 이동
        /// </summary>
        public void OnCloseButtonClicked()
        {
            int targetChapter = 1;
            if (GameManager.Instance != null)
            {
                targetChapter = GameManager.Instance.SelectedChapter;
            }

            string targetSceneName = $"Map0{targetChapter}Scene";
            Debug.Log($"[ItemPhotoController] ItemPhotoScene 종료 -> 이동 대상: {targetSceneName}");

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(targetSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}




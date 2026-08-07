using UnityEngine;
using UnityEngine.EventSystems;
using Welcome606.Managers;

namespace Welcome606.Ending
{
    public enum EndingItemType
    {
        Clothes = 0,   // 옷 (Map4)
        RiceCake = 1,  // 떡 (Map1)
        Perfume = 2,   // 향수 (Map2)
        Shoes = 3      // 신발 (Map3)
    }

    /// <summary>
    /// .unity 씬에 사전에 직접 배치되어, 1~5 챕터를 모두 클리어하여 6챕터가 해금되면 
    /// 자동으로 화면에 노출(SetActive)되고 클릭 상호작용을 처리하는 컴포넌트.
    /// </summary>
    public class EndingCollectibleItem : MonoBehaviour, IPointerClickHandler
    {
        [Header("수집품 설정")]
        [Tooltip("해당 오브젝트의 수집품 유형")]
        public EndingItemType itemType;

        [Tooltip("해당 오브젝트가 존재하는 맵 번호 (1~4)")]
        public int mapIndex = 1;

        [Tooltip("true일 경우 진행도와 상관없이 무조건 씬에 노출")]
        public bool forceShowInScene = false;

        private void OnEnable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged += RefreshVisibility;
            }
            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.OnUserDataChanged -= RefreshVisibility;
            }
        }

        private void Start()
        {
            RefreshVisibility();
        }

        /// <summary>
        /// 1~5 챕터 완료 시(6챕터 진입 상태) 오브젝트 노출 상태를 즉시 갱신합니다.
        /// </summary>
        public void RefreshVisibility()
        {
            bool isUnlocked = forceShowInScene || IsChapter6Unlocked();

            // 6챕터 진입 전(1~5 챕터 플레이 중)에는 안 보임, 6챕터 해금 시 즉시 노출
            if (gameObject.activeSelf != isUnlocked)
            {
                gameObject.SetActive(isUnlocked);
            }
        }

        private bool IsChapter6Unlocked()
        {
            var controller = FindFirstObjectByType<EndingQuestController>(FindObjectsInactive.Include);
            if (controller != null && controller.forceEnableEndingQuest)
            {
                return true;
            }

            if (UserDataManager.Instance != null)
            {
                return UserDataManager.Instance.MaxUnlockChapter >= 6;
            }
            return false;
        }

        /// <summary>
        /// 2D Collider 클릭 시 (OnMouseDown)
        /// </summary>
        private void OnMouseDown()
        {
            TriggerClick();
        }

        /// <summary>
        /// UI Graphic/EventSystem 클릭 시 (IPointerClickHandler)
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerClick();
        }

        private void TriggerClick()
        {
            var controller = FindFirstObjectByType<EndingQuestController>(FindObjectsInactive.Include);
            if (controller != null)
            {
                controller.OnCollectibleItemClicked(mapIndex, itemType);
            }
            else
            {
                Debug.LogWarning($"[EndingCollectibleItem] 씬에서 EndingQuestController를 찾을 수 없습니다. (Item: {itemType})");
            }
        }
    }
}

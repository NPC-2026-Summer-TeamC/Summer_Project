using UnityEngine;

namespace Welcome606.UI
{
    /// <summary>
    /// Map0{N}Scene에서 해당 챕터의 클리어 여부에 따라 '더러움(Before)' / '깨끗함(After)' 오브젝트 그룹을 토글하는 컨트롤러.
    /// 챕터 클리어 판정은 3스테이지 클리어 시 획득하는 수집품(UserDataManager.HasCollectedItem)을 기준으로 합니다.
    /// 수집품 오브젝트(ItemPhotoScene 진입용)는 afterObjects에 넣어 클리어 후에만 노출합니다.
    /// </summary>
    public class MapStateController : MonoBehaviour
    {
        [Header("Map Info")]
        [Tooltip("이 맵(씬)의 챕터 번호 (1~5)")]
        [SerializeField] private int chapter = 1;

        [Header("State Object Groups")]
        [Tooltip("클리어 전에만 보이는 오브젝트 (더러운 배경/스테이지 오브젝트 등)")]
        [SerializeField] private GameObject[] beforeObjects;

        [Tooltip("클리어 후에만 보이는 오브젝트 (깨끗한 배경/스테이지 오브젝트/수집품 등)")]
        [SerializeField] private GameObject[] afterObjects;

        private void OnEnable()
        {
            if (UserDataManager.Instance != null) {
                UserDataManager.Instance.OnUserDataChanged += RefreshMapState;
            }
            RefreshMapState();
        }

        private void OnDisable()
        {
            if (UserDataManager.Instance != null) {
                UserDataManager.Instance.OnUserDataChanged -= RefreshMapState;
            }
        }

        /// <summary>
        /// 저장된 진행도를 읽어 Before/After 그룹의 활성 상태를 갱신합니다. UserDataManager가 없으면 Before 상태로 둡니다.
        /// </summary>
        public void RefreshMapState()
        {
            bool isCleared = UserDataManager.Instance != null && UserDataManager.Instance.HasCollectedItem(chapter);

            SetGroupActive(beforeObjects, !isCleared);
            SetGroupActive(afterObjects, isCleared);
        }

        private void SetGroupActive(GameObject[] group, bool active)
        {
            if (group == null) {
                return;
            }

            for (int i = 0; i < group.Length; i++) {
                if (group[i] != null) {
                    group[i].SetActive(active);
                }
            }
        }
    }
}

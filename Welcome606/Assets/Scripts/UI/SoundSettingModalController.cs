using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Welcome606.UI
{
    /// <summary>
    /// 사운드 환경설정 전용 모달 컨트롤러 (SoundSettingModal_PF.prefab 연동)
    /// 마스터, 효과음, 배경음악 슬라이더 및 퍼센트 표시, 설정 초기화, 확인/닫기 기능을 제어합니다.
    /// </summary>
    public class SoundSettingModalController : MonoBehaviour
    {
        [Header("오디오 슬라이더 연결")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider bgmSlider;

        [Header("볼륨 퍼센트 텍스트 연결 (TMPro)")]
        [SerializeField] private TextMeshProUGUI masterPercentText;
        [SerializeField] private TextMeshProUGUI sfxPercentText;
        [SerializeField] private TextMeshProUGUI bgmPercentText;

        [Header("버튼 연결")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        private float cachedMasterVolume = 1.0f;
        private float cachedSFXVolume = 1.0f;
        private float cachedBGMVolume = 1.0f;

        public Slider MasterSlider => masterSlider;
        public Slider SFXSlider => sfxSlider;
        public Slider BGMSlider => bgmSlider;

        public TextMeshProUGUI MasterPercentText => masterPercentText;
        public TextMeshProUGUI SFXPercentText => sfxPercentText;
        public TextMeshProUGUI BGMPercentText => bgmPercentText;

        public Button ResetButton => resetButton;
        public Button ConfirmButton => confirmButton;
        public Button CloseButton => closeButton;

        private void Awake()
        {
            AutoFindUIComponents();
            BindListeners();
        }

        private void OnEnable()
        {
            AutoFindUIComponents();
            CacheCurrentSettings();
            SyncUIWithSettings();
        }

        /// <summary>
        /// 모달이 열릴 때의 현재 사운드 설정을 캐싱합니다.
        /// </summary>
        private void CacheCurrentSettings()
        {
            if (SoundManager.Instance != null)
            {
                cachedMasterVolume = SoundManager.Instance.MasterVolume;
                cachedSFXVolume = SoundManager.Instance.SFXVolume;
                cachedBGMVolume = SoundManager.Instance.BGMVolume;
            }
        }

        /// <summary>
        /// 자식 오브젝트 이름을 기반으로 누락된 UI 컴포넌트를 자동 할당합니다.
        /// </summary>
        public void AutoFindUIComponents()
        {
            var sliders = GetComponentsInChildren<Slider>(true);
            foreach (var slider in sliders)
            {
                string name = slider.name.ToLower();
                if (masterSlider == null && (name.Contains("master") || name.Contains("마스터")))
                {
                    masterSlider = slider;
                }
                else if (sfxSlider == null && (name.Contains("sfx") || name.Contains("se") || name.Contains("효과음")))
                {
                    sfxSlider = slider;
                }
                else if (bgmSlider == null && (name.Contains("bgm") || name.Contains("배경")))
                {
                    bgmSlider = slider;
                }
            }

            // 폴백 (슬라이더 순서)
            if (masterSlider == null && sliders.Length > 0) masterSlider = sliders[0];
            if (sfxSlider == null && sliders.Length > 1) sfxSlider = sliders[1];
            if (bgmSlider == null && sliders.Length > 2) bgmSlider = sliders[2];

            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var textComp in texts)
            {
                string name = textComp.name.ToLower();
                if (masterPercentText == null && (name.Contains("master") || name.Contains("마스터")))
                {
                    masterPercentText = textComp;
                }
                else if (sfxPercentText == null && (name.Contains("sfx") || name.Contains("se") || name.Contains("효과음")))
                {
                    sfxPercentText = textComp;
                }
                else if (bgmPercentText == null && (name.Contains("bgm") || name.Contains("배경")))
                {
                    bgmPercentText = textComp;
                }
            }

            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                string name = button.name.ToLower();
                if (resetButton == null && (name.Contains("reset") || name.Contains("초기화")))
                {
                    resetButton = button;
                }
                else if (confirmButton == null && (name.Contains("confirm") || name.Contains("확인") || name.Contains("ok")))
                {
                    confirmButton = button;
                }
                else if (closeButton == null && (name.Contains("close") || name.Contains("닫기") || name == "x" || name.Contains("btn_close")))
                {
                    closeButton = button;
                }
            }
        }

        private void BindListeners()
        {
            if (masterSlider != null)
            {
                masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(OnResetSettingClicked);
                resetButton.onClick.AddListener(OnResetSettingClicked);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        /// <summary>
        /// SoundManager의 현재 볼륨 값으로 UI 슬라이더 및 퍼센트 텍스트를 업데이트합니다.
        /// </summary>
        public void SyncUIWithSettings()
        {
            if (SoundManager.Instance != null)
            {
                if (masterSlider != null)
                {
                    masterSlider.SetValueWithoutNotify(SoundManager.Instance.MasterVolume);
                    UpdatePercentText(masterPercentText, SoundManager.Instance.MasterVolume);
                }

                if (sfxSlider != null)
                {
                    sfxSlider.SetValueWithoutNotify(SoundManager.Instance.SFXVolume);
                    UpdatePercentText(sfxPercentText, SoundManager.Instance.SFXVolume);
                }

                if (bgmSlider != null)
                {
                    bgmSlider.SetValueWithoutNotify(SoundManager.Instance.BGMVolume);
                    UpdatePercentText(bgmPercentText, SoundManager.Instance.BGMVolume);
                }
            }
        }

        public void OnMasterVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetMasterVolume(value);
            }
            UpdatePercentText(masterPercentText, value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetSFXVolume(value);
            }
            UpdatePercentText(sfxPercentText, value);
        }

        public void OnBGMVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetBGMVolume(value);
            }
            UpdatePercentText(bgmPercentText, value);
        }

        private void UpdatePercentText(TextMeshProUGUI textComp, float value)
        {
            if (textComp != null)
            {
                int percentage = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
                textComp.text = $"{percentage}%";
            }
        }

        /// <summary>
        /// 사운드 환경설정 및 유저 세이브 데이터 초기화
        /// </summary>
        public void OnResetSettingClicked()
        {
            if (UserDataManager.Instance != null)
            {
                UserDataManager.Instance.Reset();
                Debug.Log("[SoundSettingModalController] 유저 세이브 데이터가 초기화되었습니다.");
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetMasterVolume(1.0f);
                SoundManager.Instance.SetSFXVolume(1.0f);
                SoundManager.Instance.SetBGMVolume(1.0f);
                SyncUIWithSettings();
                Debug.Log("[SoundSettingModalController] 사운드 환경설정이 기본값(100%)으로 초기화되었습니다.");
            }
        }

        /// <summary>
        /// 확인 버튼 클릭 시: 변경된 설정을 저장 및 적용 확정하고 모달을 닫습니다.
        /// </summary>
        public void OnConfirmButtonClicked()
        {
            CacheCurrentSettings();
            Debug.Log("[SoundSettingModalController] 변경된 사운드 설정이 저장 및 적용되었습니다.");
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 닫기(취소) 버튼 클릭 시: 모달이 열릴 당시의 설정으로 원상복구(저장하지 않고 되돌림)하고 모달을 닫습니다.
        /// </summary>
        public void OnCloseButtonClicked()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetMasterVolume(cachedMasterVolume);
                SoundManager.Instance.SetSFXVolume(cachedSFXVolume);
                SoundManager.Instance.SetBGMVolume(cachedBGMVolume);
            }
            SyncUIWithSettings();
            Debug.Log("[SoundSettingModalController] 사운드 설정 변경사항이 취소되고 원래 값으로 복구되었습니다.");
            gameObject.SetActive(false);
        }
    }
}

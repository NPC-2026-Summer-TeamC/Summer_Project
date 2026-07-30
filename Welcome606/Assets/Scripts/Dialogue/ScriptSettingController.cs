using UnityEngine;
using UnityEngine.UI;
using TMPro; // 🔴 스킵 모드 버튼 글씨 색 변경을 위해 추가

public class ScriptSettingController : MonoBehaviour
{
    [Header("대사 매니저 연결")]
    public DialogueManager dialogueManager;

    [Header("슬라이더 UI 연결")]
    public Slider textSpeedSlider;
    public Slider autoSpeedSlider;
    public Slider dialogOpacitySlider;

    // 🔴 스킵 범위 설정 버튼 2개 (읽은 텍스트만 / 모든 텍스트). 둘 중 하나만 선택되는 라디오 버튼처럼 동작함.
    [Header("스킵 범위 설정 버튼 연결")]
    public Button readOnlySkipButton;
    public Button allTextSkipButton;
    public TextMeshProUGUI readOnlySkipButtonLabel; // 🔴 "읽은 텍스트만" 글씨
    public TextMeshProUGUI allTextSkipButtonLabel;  // 🔴 "모든 텍스트" 글씨

    [Header("스킵 범위 버튼 배경 색")]
    public Color skipModeSelectedColor = new Color(0.75f, 0.15f, 0.15f);
    public Color skipModeUnselectedColor = Color.white;

    [Header("스킵 범위 버튼 글씨 색")]
    public Color skipModeLabelSelectedColor = Color.white;
    public Color skipModeLabelUnselectedColor = Color.black;

    [Header("기본 설정값")]
    public float defaultTextSpeed = 0.05f;
    public float defaultAutoSpeed = 3f;
    public float defaultOpacity = 1f;

    private void Awake()
    {
        if (textSpeedSlider != null) textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
        if (autoSpeedSlider != null) autoSpeedSlider.onValueChanged.AddListener(OnAutoSpeedChanged);
        if (dialogOpacitySlider != null) dialogOpacitySlider.onValueChanged.AddListener(OnOpacityChanged);

        // 🔴 스킵 범위 버튼 클릭 연결
        if (readOnlySkipButton != null) readOnlySkipButton.onClick.AddListener(() => OnSkipModeSelected(DialogueManager.SkipMode.ReadOnly));
        if (allTextSkipButton != null) allTextSkipButton.onClick.AddListener(() => OnSkipModeSelected(DialogueManager.SkipMode.AllText));
    }

    // 🔴 설정창이 화면에 켜질 때마다(활성화될 때마다) 슬라이더/토글을 현재 적용된 값으로 맞춰줌
    private void OnEnable()
    {
        if (dialogueManager == null) return;

        if (textSpeedSlider != null)
            textSpeedSlider.SetValueWithoutNotify(InvertSliderValue(textSpeedSlider, dialogueManager.typingSpeed));

        if (autoSpeedSlider != null)
            autoSpeedSlider.SetValueWithoutNotify(InvertSliderValue(autoSpeedSlider, dialogueManager.autoSpeed));

        if (dialogOpacitySlider != null && dialogueManager.dialogBackgroundImage != null)
            dialogOpacitySlider.SetValueWithoutNotify(dialogueManager.dialogBackgroundImage.color.a);

        UpdateSkipModeVisual(); // 🔴 설정창 열릴 때 현재 스킵 모드에 맞춰 버튼 색 갱신
    }

    // 🔴 슬라이더 왼쪽(느림)/오른쪽(빠름) 라벨 방향과 실제 값의 의미(작을수록 빠름)가 반대라서
    // 슬라이더 값을 뒤집어주는 함수. min+max-value 를 하면 왼쪽 <-> 오른쪽이 서로 바뀜.
    private float InvertSliderValue(Slider slider, float value)
    {
        return slider.minValue + slider.maxValue - value;
    }

    public void OnTextSpeedChanged(float value)
    {
        if (dialogueManager != null) dialogueManager.typingSpeed = InvertSliderValue(textSpeedSlider, value);
    }

    public void OnAutoSpeedChanged(float value)
    {
        if (dialogueManager != null) dialogueManager.autoSpeed = InvertSliderValue(autoSpeedSlider, value);
    }

    public void OnOpacityChanged(float value)
    {
        if (dialogueManager != null) dialogueManager.SetOpacity(value);
    }

    // 🔴 "읽은 텍스트만" / "모든 텍스트" 버튼 클릭 시 호출됨
    private void OnSkipModeSelected(DialogueManager.SkipMode mode)
    {
        if (dialogueManager != null) dialogueManager.SetSkipMode(mode);
        UpdateSkipModeVisual();
    }

    // 🔴 현재 선택된 스킵 모드에 맞춰 두 버튼 중 하나만 강조색으로 표시 (배경 + 글씨 색 둘 다)
    private void UpdateSkipModeVisual()
    {
        if (dialogueManager == null) return;

        bool isReadOnly = dialogueManager.skipMode == DialogueManager.SkipMode.ReadOnly;

        SetSkipButtonVisual(readOnlySkipButton, readOnlySkipButtonLabel, isReadOnly);
        SetSkipButtonVisual(allTextSkipButton, allTextSkipButtonLabel, !isReadOnly);
    }

    private void SetSkipButtonVisual(Button button, TextMeshProUGUI label, bool isSelected)
    {
        if (button != null)
        {
            Image image = button.GetComponent<Image>();
            if (image != null) image.color = isSelected ? skipModeSelectedColor : skipModeUnselectedColor;
        }

        if (label != null)
        {
            label.color = isSelected ? skipModeLabelSelectedColor : skipModeLabelUnselectedColor;
        }
    }

    public void OnResetButtonClicked()
    {
        if (textSpeedSlider != null) textSpeedSlider.value = InvertSliderValue(textSpeedSlider, defaultTextSpeed);
        if (autoSpeedSlider != null) autoSpeedSlider.value = InvertSliderValue(autoSpeedSlider, defaultAutoSpeed);
        if (dialogOpacitySlider != null) dialogOpacitySlider.value = defaultOpacity;

        OnSkipModeSelected(DialogueManager.SkipMode.ReadOnly); // 🔴 스킵 모드도 기본값(읽은 텍스트만)으로
    }

    public void OnConfirmButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
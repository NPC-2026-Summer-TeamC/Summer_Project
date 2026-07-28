using UnityEngine;
using UnityEngine.UI;

public class ScriptSettingController : MonoBehaviour
{
    [Header("대사 매니저 연결")]
    public DialogueManager dialogueManager;

    [Header("슬라이더 UI 연결")]
    public Slider textSpeedSlider;
    public Slider autoSpeedSlider;
    public Slider dialogOpacitySlider;

    [Header("기본 설정값")]
    public float defaultTextSpeed = 0.05f;
    public float defaultAutoSpeed = 3f;
    public float defaultOpacity = 1f;

    private void Awake()
    {
        if (textSpeedSlider != null) textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
        if (autoSpeedSlider != null) autoSpeedSlider.onValueChanged.AddListener(OnAutoSpeedChanged);
        if (dialogOpacitySlider != null) dialogOpacitySlider.onValueChanged.AddListener(OnOpacityChanged);
    }

    // 🔴 설정창이 화면에 켜질 때마다(활성화될 때마다) 슬라이더/토글을 현재 적용된 값으로 맞춰줌
    private void OnEnable()
    {
        if (dialogueManager == null) return;

        if (textSpeedSlider != null)
            textSpeedSlider.SetValueWithoutNotify(InvertSliderValue(textSpeedSlider, dialogueManager.typingSpeed));

        if (autoSpeedSlider != null)
            autoSpeedSlider.SetValueWithoutNotify(InvertSliderValue(autoSpeedSlider, dialogueManager.autoSpeed));

        if (dialogOpacitySlider != null && dialogueManager.dialogCanvasGroup != null)
            dialogOpacitySlider.SetValueWithoutNotify(dialogueManager.dialogCanvasGroup.alpha);
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

    public void OnResetButtonClicked()
    {
        if (textSpeedSlider != null) textSpeedSlider.value = InvertSliderValue(textSpeedSlider, defaultTextSpeed);
        if (autoSpeedSlider != null) autoSpeedSlider.value = InvertSliderValue(autoSpeedSlider, defaultAutoSpeed);
        if (dialogOpacitySlider != null) dialogOpacitySlider.value = defaultOpacity;
    }

    public void OnConfirmButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
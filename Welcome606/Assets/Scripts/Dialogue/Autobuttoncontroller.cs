using UnityEngine;
using UnityEngine.UI;
using TMPro; // 🔴 AUTO 글씨(TextMeshPro) 색을 바꾸기 위해 추가

// 🔴 하단 바에 있는 AUTO 버튼 전용 스크립트

public class AutoButtonController : MonoBehaviour
{
    [Header("연결")]
    public DialogueManager dialogueManager;
    public Button autoButton;
    public TextMeshProUGUI autoButtonLabel; // 🔴 버튼 안의 "AUTO" 글씨 (하이어라키의 AutoText)

    [Header("켜짐 / 꺼짐 배경 색")]
    [Tooltip("자동 진행이 켜졌을 때 버튼 색")]
    public Color onColor = new Color(0.75f, 0.15f, 0.15f);
    [Tooltip("자동 진행이 꺼졌을 때 버튼 색 (원래 색)")]
    public Color offColor = Color.white;

    [Header("켜짐 / 꺼짐 글씨 색")]
    [Tooltip("자동 진행이 켜졌을 때 글씨 색")]
    public Color labelOnColor = Color.white;
    [Tooltip("자동 진행이 꺼졌을 때 글씨 색 (원래 색)")]
    public Color labelOffColor = Color.black;

    private bool isOn = false;
    private Image buttonImage; // 버튼의 배경 이미지(색을 바꿀 대상)

    private void Awake()
    {
        if (autoButton != null)
        {
            // 버튼 자체에 붙어있는 Image 컴포넌트를 가져와서 색을 바꿀 거임
            buttonImage = autoButton.GetComponent<Image>();
            autoButton.onClick.AddListener(OnAutoButtonClicked);
        }

        UpdateVisual();
    }

    // 🔴 AUTO 버튼을 누르면 호출됨
    private void OnAutoButtonClicked()
    {
        isOn = !isOn;

        if (dialogueManager != null)
        {
            dialogueManager.SetAutoPlay(isOn);
        }

        UpdateVisual();
    }

    // 🔴 현재 상태에 맞춰 버튼 배경색 + 글씨색을 바꿔줌
    private void UpdateVisual()
    {
        if (buttonImage != null)
        {
            buttonImage.color = isOn ? onColor : offColor;
        }

        if (autoButtonLabel != null)
        {
            autoButtonLabel.color = isOn ? labelOnColor : labelOffColor;
        }
    }
}
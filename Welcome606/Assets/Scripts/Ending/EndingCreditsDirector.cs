using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Welcome606.Managers;

namespace Welcome606.Ending
{
    /// <summary>
    /// CreditsScene 진입 시 Cut1~Cut6 엔딩 크레딧 연출 전체 흐름을 순차 지휘하는 씬 배치 컨트롤러 컴포넌트.
    /// (씬 배치는 Unity 에디터에서 수동으로 진행되며, 모든 참조는 Inspector에서 연결한다)
    /// </summary>
    public class EndingCreditsDirector : MonoBehaviour
    {
        [Header("Cut1 - 문/신발, 대사 자동 진행")]
        [SerializeField] private GameObject cut1Panel;
        [SerializeField] private CanvasGroup cut1CanvasGroup;
        [SerializeField] private TextMeshProUGUI cut1ScriptText;

        [Header("Cut2 - 거울, Tilt-Down, White-Out")]
        [SerializeField] private GameObject cut2Panel;
        [SerializeField] private CanvasGroup cut2CanvasGroup;
        [SerializeField] private RectTransform cut2TiltTarget;

        [Header("Cut3 - 코믹 5컷 스크롤 시퀀스")]
        [SerializeField] private GameObject cut3Panel;
        [SerializeField] private CanvasGroup cut3CanvasGroup;
        [Tooltip("EndingCredit01~05.png를 순서대로 배치한 5개의 Image 자식을 담은 스크롤 컨테이너(빈 오브젝트)의 RectTransform")]
        [SerializeField] private RectTransform cut3TiltTarget;

        [Header("Cut4 - 스테이지별 dirty↔clean 크로스페이드")]
        [SerializeField] private GameObject cut4Panel;
        [SerializeField] private CanvasGroup cut4CanvasGroup;
        [SerializeField] private Image cut4CrossfadeImageA;
        [SerializeField] private Image cut4CrossfadeImageB;
        [SerializeField] private Sprite[] dirtyStageSprites;
        [SerializeField] private Sprite[] cleanStageSprites;

        [Header("Cut5 - 소품 교차 삽입 + 팀 크레딧 스크롤")]
        [SerializeField] private GameObject cut5Panel;
        [SerializeField] private CanvasGroup cut5CanvasGroup;
        [SerializeField] private Image cut5PropImage;
        [SerializeField] private Sprite[] cut5PropSprites;
        [SerializeField] private TextMeshProUGUI[] cut5CreditTexts;
        [Tooltip("cut5CreditTexts 4블록을 세로로 담은 스크롤 컨테이너(빈 오브젝트)의 RectTransform")]
        [SerializeField] private RectTransform cut5CreditsScrollTarget;

        [Header("Cut6 - 엔딩 문구, 클릭 대기 후 복귀")]
        [SerializeField] private GameObject cut6Panel;
        [SerializeField] private CanvasGroup cut6CanvasGroup;
        [SerializeField] private TextMeshProUGUI cut6DedicationText;
        [SerializeField] private Button cut6FinalClickButton;

        [Header("공통 - 스킵 / 사운드")]
        [SerializeField] private Button skipButton;
        [SerializeField] private AudioClip doorOpenSfx;
        [SerializeField] private AudioClip endingBgmClip;

        // Cut1 임시 대사(스토리 담당자 확정 전). 추후 문구 교체 시 이 배열 값만 수정하면 된다.
        private readonly string[] cut1ScriptLines = new string[]
        {
            "정신없이 뛰어다니던 나날들이었다.",
            "이제 이 방도 제법 깨끗해졌다.",
            "문 앞에 가지런히 놓인 신발을 바라보았다.",
            "미희는 조용히 신발을 신었다."
        };

        // Cut5 팀 크레딧 4블록(기획/개발/아트/음악). 기획서 원문 그대로 하드코딩.
        private readonly string[] cut5TeamCreditBlocks = new string[]
        {
            "기획\n박재희, 신려진, 한지언",
            "개발\n이지민, 윤정하, 진서",
            "아트\n송채언, 윤채빈",
            "음악\n박시온"
        };

        private const float CutFadeDuration = 1.0f;
        private const float Cut1LineDisplayDuration = 3.0f;
        private const float Cut2TiltDuration = 2.5f;
        private const float Cut2TiltDistance = 200f;
        private const float Cut2HoldAfterTilt = 1.0f;
        private const int Cut3ComicPanelCount = 5;
        private const float Cut3PanelSpacing = 400f;
        private const float Cut3TiltDuration = 1.5f;
        private const float Cut3HoldPerImage = 5.0f;
        private const float Cut4StageCrossfadeDuration = 1.2f;
        private const float Cut4HoldPerStage = 3.5f;
        private const int Cut5CreditBlockCount = 4;
        private const float Cut5CreditBlockSpacing = 200f;
        private const float Cut5ScrollDuration = 1.5f;
        private const float Cut5HoldPerBeat = 5.5f;
        private const float Cut6TextFadeDuration = 2.0f;
        private const string MainMenuSceneName = "MainMenuScene";

        private bool isWaitingForFinalClick = false;

        private void Start()
        {
            InitializeCutPanels();

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }

            if (cut6FinalClickButton != null)
            {
                cut6FinalClickButton.onClick.AddListener(OnFinalClickToMainMenu);
            }

            StartCoroutine(CoPlayEndingSequence());
        }

        /// <summary>
        /// 모든 Cut 패널을 alpha 0 / 입력 차단 상태로 강제 초기화한다.
        /// (씬에서 초기값 세팅을 놓쳐도 방어되도록 코드에서 한 번 더 보장)
        /// </summary>
        private void InitializeCutPanels()
        {
            ActivatePanel(cut1Panel);
            ActivatePanel(cut2Panel);
            ActivatePanel(cut3Panel);
            ActivatePanel(cut4Panel);
            ActivatePanel(cut5Panel);
            ActivatePanel(cut6Panel);

            SetCanvasGroupState(cut1CanvasGroup, 0f, false, false);
            SetCanvasGroupState(cut2CanvasGroup, 0f, false, false);
            SetCanvasGroupState(cut3CanvasGroup, 0f, false, false);
            SetCanvasGroupState(cut4CanvasGroup, 0f, false, false);
            SetCanvasGroupState(cut5CanvasGroup, 0f, false, false);
            SetCanvasGroupState(cut6CanvasGroup, 0f, false, false);
        }

        private void ActivatePanel(GameObject panel)
        {
            if (panel != null && !panel.activeSelf)
            {
                panel.SetActive(true);
            }
        }

        private void SetCanvasGroupState(CanvasGroup cg, float alpha, bool interactable, bool blocksRaycasts)
        {
            if (cg == null) return;

            cg.alpha = alpha;
            cg.interactable = interactable;
            cg.blocksRaycasts = blocksRaycasts;
        }

        private IEnumerator CoPlayEndingSequence()
        {
            yield return StartCoroutine(CoCut1_DoorAndShoes());
            yield return StartCoroutine(CoCut2_MirrorTiltAndWhiteOut());
            yield return StartCoroutine(CoCut3_ComicSequence());
            yield return StartCoroutine(CoCut4_StageCrossfade());
            yield return StartCoroutine(CoCut5_PropsAndCredits());
            yield return StartCoroutine(CoCut6_Dedication());
        }

        private IEnumerator CoCut1_DoorAndShoes()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(doorOpenSfx);
                SoundManager.Instance.PlayBGM(endingBgmClip);
            }

            yield return StartCoroutine(CoFadeCanvasGroup(cut1CanvasGroup, 0f, 1f, CutFadeDuration));

            for (int i = 0; i < cut1ScriptLines.Length; i++)
            {
                if (cut1ScriptText != null)
                {
                    cut1ScriptText.text = cut1ScriptLines[i];
                }

                yield return new WaitForSeconds(Cut1LineDisplayDuration);
            }

            yield return StartCoroutine(CoFadeCanvasGroup(cut1CanvasGroup, 1f, 0f, CutFadeDuration));
        }

        private IEnumerator CoCut2_MirrorTiltAndWhiteOut()
        {
            yield return StartCoroutine(CoFadeCanvasGroup(cut2CanvasGroup, 0f, 1f, CutFadeDuration));

            yield return StartCoroutine(CoPanTiltDown(cut2TiltTarget, Cut2TiltDistance, Cut2TiltDuration));

            yield return new WaitForSeconds(Cut2HoldAfterTilt);

            // White-Out: 다음 Cut3의 흰 배경으로 자연스럽게 이어지도록 페이드 아웃만 수행한다.
            yield return StartCoroutine(CoFadeCanvasGroup(cut2CanvasGroup, 1f, 0f, CutFadeDuration));
        }

        private IEnumerator CoCut3_ComicSequence()
        {
            yield return StartCoroutine(CoFadeCanvasGroup(cut3CanvasGroup, 0f, 1f, CutFadeDuration));

            // 씬에 세로로 쌓아둔 5장의 코믹 패널(EndingCredit01~05.png)을 스크롤 컨테이너(cut3TiltTarget)를
            // 한 칸씩 위로 Pan하며 순서대로 훑고 지나가는 연출. 첫 장은 이미 화면에 보이므로 대기부터 시작한다.
            // (아래쪽에 쌓인 다음 패널을 끌어올려야 하므로 CoPanTiltDown에 음수 거리를 넘겨 방향을 반대로 쓴다)
            yield return new WaitForSeconds(Cut3HoldPerImage);

            for (int i = 1; i < Cut3ComicPanelCount; i++)
            {
                yield return StartCoroutine(CoPanTiltDown(cut3TiltTarget, -Cut3PanelSpacing, Cut3TiltDuration));

                yield return new WaitForSeconds(Cut3HoldPerImage);
            }

            yield return StartCoroutine(CoFadeCanvasGroup(cut3CanvasGroup, 1f, 0f, CutFadeDuration));
        }

        private IEnumerator CoCut4_StageCrossfade()
        {
            yield return StartCoroutine(CoFadeCanvasGroup(cut4CanvasGroup, 0f, 1f, CutFadeDuration));

            int stageCount = dirtyStageSprites != null ? dirtyStageSprites.Length : 0;

            for (int i = 0; i < stageCount; i++)
            {
                if (cut4CrossfadeImageA != null)
                {
                    cut4CrossfadeImageA.sprite = dirtyStageSprites[i];
                    SetGraphicAlpha(cut4CrossfadeImageA, 1f);
                }

                bool hasCleanSprite = cleanStageSprites != null && i < cleanStageSprites.Length && cleanStageSprites[i] != null;

                if (cut4CrossfadeImageB != null)
                {
                    // clean 아트가 아직 없으면 sprite를 비워 단색 placeholder로 표시한다.
                    cut4CrossfadeImageB.sprite = hasCleanSprite ? cleanStageSprites[i] : null;
                    SetGraphicAlpha(cut4CrossfadeImageB, 0f);
                }

                yield return StartCoroutine(CoFadeGraphicAlpha(cut4CrossfadeImageB, 0f, 1f, Cut4StageCrossfadeDuration));

                yield return new WaitForSeconds(Cut4HoldPerStage);

                SetGraphicAlpha(cut4CrossfadeImageB, 0f);
            }

            yield return StartCoroutine(CoFadeCanvasGroup(cut4CanvasGroup, 1f, 0f, CutFadeDuration));
        }

        private IEnumerator CoCut5_PropsAndCredits()
        {
            yield return StartCoroutine(CoFadeCanvasGroup(cut5CanvasGroup, 0f, 1f, CutFadeDuration));

            if (cut5CreditTexts != null)
            {
                for (int i = 0; i < cut5CreditTexts.Length; i++)
                {
                    if (cut5CreditTexts[i] != null)
                    {
                        cut5CreditTexts[i].text = i < cut5TeamCreditBlocks.Length ? cut5TeamCreditBlocks[i] : string.Empty;
                    }
                }
            }

            // Cut3와 동일하게, 세로로 쌓아둔 4블록을 스크롤 컨테이너(cut5CreditsScrollTarget)로
            // 한 칸씩 아래에서 위로 Pan하며 훑고 지나가는 크레딧 롤 연출. 소품 이미지는 매 블록 전환에 맞춰 교체한다.
            // (아래쪽에 쌓인 다음 블록을 끌어올려야 하므로 CoPanTiltDown에 음수 거리를 넘겨 방향을 반대로 쓴다)
            UpdateCut5PropSprite(0);

            yield return new WaitForSeconds(Cut5HoldPerBeat);

            for (int i = 1; i < Cut5CreditBlockCount; i++)
            {
                UpdateCut5PropSprite(i);

                yield return StartCoroutine(CoPanTiltDown(cut5CreditsScrollTarget, -Cut5CreditBlockSpacing, Cut5ScrollDuration));

                yield return new WaitForSeconds(Cut5HoldPerBeat);
            }
        }

        private void UpdateCut5PropSprite(int index)
        {
            if (cut5PropImage == null || cut5PropSprites == null || cut5PropSprites.Length == 0) return;

            Sprite sprite = cut5PropSprites[index % cut5PropSprites.Length];
            if (sprite != null)
            {
                cut5PropImage.sprite = sprite;
            }
        }

        private IEnumerator CoCut6_Dedication()
        {
            yield return StartCoroutine(CoFadeCanvasGroup(cut5CanvasGroup, 1f, 0f, CutFadeDuration));

            if (cut6DedicationText != null)
            {
                cut6DedicationText.text = "우리 곁의 미화들에게.";
                SetGraphicAlpha(cut6DedicationText, 0f);
            }

            yield return StartCoroutine(CoFadeCanvasGroup(cut6CanvasGroup, 0f, 1f, CutFadeDuration));

            if (cut6DedicationText != null)
            {
                yield return StartCoroutine(CoFadeGraphicAlpha(cut6DedicationText, 0f, 1f, Cut6TextFadeDuration));
            }

            isWaitingForFinalClick = true;
            SetCanvasGroupState(cut6CanvasGroup, 1f, true, true);

            while (isWaitingForFinalClick)
            {
                yield return null;
            }
        }

        private void OnFinalClickToMainMenu()
        {
            if (!isWaitingForFinalClick) return;

            isWaitingForFinalClick = false;

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(MainMenuSceneName);
            }
        }

        /// <summary>
        /// Skip 버튼 클릭 시 즉시 MainMenuScene으로 복귀한다.
        /// (기존 Botton_Back의 OnMouseDown_SwitchScene을 대체)
        /// </summary>
        public void OnSkipClicked()
        {
            StopAllCoroutines();

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(MainMenuSceneName);
            }
        }

        /// <summary>
        /// SceneFlowManager.CoFade의 Lerp 패턴을 차용한 Director 내부 전용 CanvasGroup 페이드 코루틴.
        /// </summary>
        private IEnumerator CoFadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            if (cg == null) yield break;

            float elapsed = 0f;
            cg.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            cg.alpha = to;

            bool isVisible = to > 0f;
            cg.interactable = isVisible;
            cg.blocksRaycasts = isVisible;
        }

        /// <summary>
        /// 대상 RectTransform의 anchoredPosition.y를 아래로 Lerp 이동시키는 카메라 Tilt-Down 연출용 코루틴.
        /// (뷰포트는 씬에서 RectMask2D로 감싸져 있다고 가정)
        /// </summary>
        private IEnumerator CoPanTiltDown(RectTransform target, float distance, float duration)
        {
            if (target == null) yield break;

            Vector2 startPos = target.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, -distance);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }

            target.anchoredPosition = endPos;
        }

        /// <summary>
        /// Image/TextMeshProUGUI 등 Graphic 공통 알파값을 Lerp로 페이드하는 내부 유틸 코루틴.
        /// (Cut4 크로스페이드, Cut5/Cut6 텍스트 페이드에서 공용으로 사용)
        /// </summary>
        private IEnumerator CoFadeGraphicAlpha(Graphic graphic, float from, float to, float duration)
        {
            if (graphic == null) yield break;

            float elapsed = 0f;
            SetGraphicAlpha(graphic, from);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetGraphicAlpha(graphic, Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            SetGraphicAlpha(graphic, to);
        }

        private void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null) return;

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }
}

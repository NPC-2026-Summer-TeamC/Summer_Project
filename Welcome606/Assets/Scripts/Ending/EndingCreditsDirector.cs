using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("cut3TiltTarget")]
        [SerializeField] private RectTransform cut3ScrollTarget;

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
        [FormerlySerializedAs("cut5CreditsScrollTarget")]
        [SerializeField] private RectTransform cut5ScrollTarget;

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

        // 스토리보드(Agents/documents/ending_storybd1.png, 2.png) 타임코드(총 2:59=179초) 기준 재조정.
        // Cut1: 0:00-0:20(20s) / Cut2: 0:20-0:40(20s=틸트15s+화이트아웃5s) / Cut3: 0:40-1:20(40s)
        // Cut4: 1:20-2:00(40s) / Cut5: 2:00-2:46(46s) / Cut6: 2:46-2:59(13s, 클릭대기라 소프트 예산)
        private const float CutFadeDuration = 1.0f;
        private const float Cut1LineDisplayDuration = 4.5f;
        private const float Cut2TiltDuration = 14.0f;
        private const float Cut2TiltDistance = 200f;
        private const float Cut2WhiteOutFadeDuration = 5.0f;
        private const float Cut3ScrollDuration = 38.0f;
        private const float Cut4StageCrossfadeDuration = 1.2f;
        private const float Cut4LoopBudgetSeconds = 38.0f;
        private const int Cut5CreditBlockCount = 4;
        private const float Cut5CreditBlockSpacing = 200f;
        private const float Cut5ScrollDuration = 45.0f;
        // Cut6 진입 전 페이드 합(fade-in 1s + Cut6TextFadeDuration 2s = 3s, Cut5→Cut6 전환 fade 1s 포함 총 4s)이
        // 13초 소프트 예산 안에 들어간다. 클릭 대기 구조이므로 이 상수들은 변경하지 않는다.
        private const float Cut6TextFadeDuration = 2.0f;
        private const string MainMenuSceneName = "MainMenuScene";

        private bool isWaitingForFinalClick = false;
        private Vector2 cut3ScrollTargetInitialPos;

        private void Awake()
        {
            if (cut3ScrollTarget != null)
            {
                cut3ScrollTargetInitialPos = cut3ScrollTarget.anchoredPosition;
            }
        }

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

            if (cut3ScrollTarget != null)
            {
                cut3ScrollTarget.anchoredPosition = cut3ScrollTargetInitialPos;
            }
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

            // White-Out: 다음 Cut3의 흰 배경으로 자연스럽게 이어지도록 페이드 아웃만 수행한다.
            yield return StartCoroutine(CoFadeCanvasGroup(cut2CanvasGroup, 1f, 0f, Cut2WhiteOutFadeDuration));
        }

        private IEnumerator CoCut3_ComicSequence()
        {
            yield return StartCoroutine(CoFadeCanvasGroup(cut3CanvasGroup, 0f, 1f, CutFadeDuration));

            PrepareCut3PanelPositionsForDownwardScroll();

            // 씬에 배치된 5장의 코믹 패널(EndingCredit01~05.png)을 스크롤 컨테이너(cut3ScrollTarget)가
            // 화면 위에서 아래로 내려오는 방향(Tilt-Down 연출)으로 1번➔5번 순서대로 한 번의 연속 등속 스크롤.
            float totalDistance = ComputeCut3ScrollDistance();

            yield return StartCoroutine(CoScrollDownContinuous(cut3ScrollTarget, totalDistance, Cut3ScrollDuration));

            yield return StartCoroutine(CoFadeCanvasGroup(cut3CanvasGroup, 1f, 0f, CutFadeDuration));
        }

        /// <summary>
        /// 씬에서 1번(상단)부터 5번(하단)으로 배치된 코믹 패널들을,
        /// 화면 위에서 아래로 내려오는 방향(Tilt-Down)으로 1번➔5번 순서대로 보여주기 위해
        /// 런타임 시작 시 2~5번 패널의 Y 오프셋을 1번 패널 상단(+Y)으로 재정렬합니다.
        /// (사용자의 기존 씬 수동 배치를 유지하면서 위에서 아래로 스크롤 가능하도록 자동 보정)
        /// </summary>
        private void PrepareCut3PanelPositionsForDownwardScroll()
        {
            if (cut3ScrollTarget == null || cut3ScrollTarget.childCount < 2) return;

            RectTransform first = cut3ScrollTarget.GetChild(0) as RectTransform;
            RectTransform last = cut3ScrollTarget.GetChild(cut3ScrollTarget.childCount - 1) as RectTransform;
            if (first == null || last == null) return;

            // 이미 자식들이 상단(+Y) 방향으로 배치되어 있다면 재정렬 불필요
            if (last.anchoredPosition.y > first.anchoredPosition.y) return;

            float firstY = first.anchoredPosition.y;

            for (int i = 1; i < cut3ScrollTarget.childCount; i++)
            {
                RectTransform child = cut3ScrollTarget.GetChild(i) as RectTransform;
                if (child == null) continue;

                float distanceFromFirst = Mathf.Abs(child.anchoredPosition.y - firstY);
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, firstY + distanceFromFirst);
            }
        }

        /// <summary>
        /// cut3ScrollTarget의 첫 번째/마지막 자식 anchoredPosition.y 차이의 절댓값으로 Cut3 총 스크롤 거리를 계산한다.
        /// 자식이 없거나 1개 이하인 경우 0을 반환해 스크롤이 발생하지 않도록 방어한다.
        /// </summary>
        private float ComputeCut3ScrollDistance()
        {
            if (cut3ScrollTarget == null || cut3ScrollTarget.childCount < 2) return 0f;

            RectTransform first = cut3ScrollTarget.GetChild(0) as RectTransform;
            RectTransform last = cut3ScrollTarget.GetChild(cut3ScrollTarget.childCount - 1) as RectTransform;

            if (first == null || last == null) return 0f;

            return Mathf.Abs(last.anchoredPosition.y - first.anchoredPosition.y);
        }

        private IEnumerator CoCut4_StageCrossfade()
        {
            yield return StartCoroutine(CoFadeCanvasGroup(cut4CanvasGroup, 0f, 1f, CutFadeDuration));

            int stageCount = dirtyStageSprites != null ? dirtyStageSprites.Length : 0;
            float dynamicHoldPerStage = 0f;

            if (stageCount > 0)
            {
                dynamicHoldPerStage = Mathf.Max(0f, (Cut4LoopBudgetSeconds / stageCount) - Cut4StageCrossfadeDuration);

                if ((Cut4StageCrossfadeDuration * stageCount) > Cut4LoopBudgetSeconds)
                {
                    Debug.LogWarning($"[EndingCreditsDirector] Cut4 stageCount={stageCount}가 너무 많아 크로스페이드만으로 {Cut4LoopBudgetSeconds}초 예산을 초과합니다. 대기시간이 0으로 클램프됩니다.");
                }
            }

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

                yield return new WaitForSeconds(dynamicHoldPerStage);

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

            // Cut3와 동일하게, 세로로 쌓아둔 4블록을 스크롤 컨테이너(cut5ScrollTarget)를
            // 정지-이동 반복 없이 한 번의 연속 등속 스크롤로 위로 훑고 지나가는 크레딧 롤 연출.
            // 소품 교체는 스크롤과 별개의 병렬 코루틴(CoCut5SchedulePropSwaps)에서 타이밍을 맞춰 처리한다.
            UpdateCut5PropSprite(0);

            StartCoroutine(CoCut5SchedulePropSwaps());

            yield return StartCoroutine(CoScrollUpContinuous(cut5ScrollTarget, Cut5CreditBlockSpacing * (Cut5CreditBlockCount - 1), Cut5ScrollDuration));

            // Cut5는 원래도 자체 종료 페이드가 없다 — CoCut6_Dedication이 cut5CanvasGroup을 페이드아웃한다.
            // 46초 예산 계산(1s fade-in + 45s 스크롤=46s)이 이 구조를 전제로 하므로 여기서 fade-out을 추가하지 않는다.
        }

        /// <summary>
        /// Cut5 소품 이미지를 스크롤과 병렬로 일정 간격마다 교체하는 스케줄러 코루틴.
        /// 이 코루틴은 스크롤과 병렬로 실행되며(StartCoroutine만 하고 yield하지 않음),
        /// 이 파일의 다른 모든 코루틴과 달리 CoPlayEndingSequence 흐름에서 직접 yield되지 않는다.
        /// OnSkipClicked()의 StopAllCoroutines()가 이 코루틴도 함께 정지시키므로 별도 정리 처리는 불필요하다.
        /// </summary>
        private IEnumerator CoCut5SchedulePropSwaps()
        {
            float perBlockInterval = Cut5ScrollDuration / (Cut5CreditBlockCount - 1);

            for (int i = 1; i < Cut5CreditBlockCount; i++)
            {
                yield return new WaitForSeconds(perBlockInterval);

                UpdateCut5PropSprite(i);
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
        /// 대상 RectTransform의 anchoredPosition.y를 정지-이동 반복 없이 한 번의 등속으로 위로 스크롤하는 코루틴.
        /// (Cut5 팀 크레딧 스크롤에서 사용)
        /// </summary>
        private IEnumerator CoScrollUpContinuous(RectTransform target, float distance, float duration)
        {
            if (target == null) yield break;

            Vector2 startPos = target.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, distance); // 위로 이동 (양수 = 위)
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
        /// 대상 RectTransform의 anchoredPosition.y를 정지-이동 반복 없이 한 번의 등속으로 아래로 스크롤하는 코루틴.
        /// (Cut3 코믹 시퀀스 위에서 아래로 내려오는 Tilt-Down 연출용)
        /// </summary>
        private IEnumerator CoScrollDownContinuous(RectTransform target, float distance, float duration)
        {
            if (target == null) yield break;

            Vector2 startPos = target.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, -distance); // 아래로 이동 (음수 = 아래)
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

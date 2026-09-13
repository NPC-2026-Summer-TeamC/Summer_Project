using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField] private CanvasGroup cut1CanvasGroup;
        [SerializeField] private TextMeshProUGUI cut1ScriptText;

        [Header("Cut2 - 거울, Tilt-Down, White-Out")]
        [SerializeField] private CanvasGroup cut2CanvasGroup;
        [SerializeField] private RectTransform cut2TiltTarget;

        [Header("Cut3 - 코믹 5컷 스크롤 시퀀스")]
        [SerializeField] private CanvasGroup cut3CanvasGroup;
        [Tooltip("EndingCredit01~05.png를 순서대로 배치한 스크롤 컨테이너(빈 오브젝트)의 RectTransform. " +
                 "RectMask2D가 붙은 Cut3Panel의 직계 자식이어야 한다(부모를 뷰포트로 사용).")]
        [SerializeField] private RectTransform cut3ScrollTarget;

        [Header("Cut4 - 스테이지별 dirty→clean 전환 스크롤")]
        [SerializeField] private CanvasGroup cut4CanvasGroup;
        [Tooltip("Before(dirty)/After(clean) 이미지 쌍을 세로로 배치한 스크롤 컨테이너의 RectTransform. " +
                 "RectMask2D가 붙은 Cut4Panel의 직계 자식이어야 한다(부모를 뷰포트로 사용).")]
        [SerializeField] private RectTransform cut4ScrollTarget;
        [Tooltip("더러운 방(Before) 이미지 배열. cut4CleanImages와 같은 인덱스가 같은 위치에 겹쳐 배치된 한 쌍이다.")]
        [SerializeField] private Image[] cut4DirtyImages;
        [Tooltip("깨끗한 방(After) 이미지 배열. cut4DirtyImages와 인덱스로 1:1 매칭된다.")]
        [SerializeField] private Image[] cut4CleanImages;

        [Header("Cut5 - 수집요소 + 기여 크레딧 스크롤")]
        [SerializeField] private CanvasGroup cut5CanvasGroup;
        [Tooltip("소품 이미지와 팀 크레딧 텍스트 블록을 세로로 배치한 스크롤 컨테이너의 RectTransform. " +
                 "RectMask2D가 붙은 Cut5Panel의 직계 자식이어야 한다(부모를 뷰포트로 사용).")]
        [SerializeField] private RectTransform cut5ScrollTarget;

        [Header("Cut6 - 엔딩 문구, BGM 종료/클릭 대기 후 복귀")]
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

        // Cut5 팀 크레딧 / Cut6 헌사 문구는 씬(TextMeshProUGUI)이 직접 소유한다.
        // 코드는 페이드 등 연출만 담당하고 문자열을 덮어쓰지 않는다.

        private const float CutFadeDuration = 1.0f;
        private const float Cut1LineDisplayDuration = 4.5f;
        private const float Cut2TiltDuration = 14.0f;
        private const float Cut2TiltDistance = 200f;
        private const float Cut2WhiteOutFadeDuration = 5.0f;

        // 기획서 "크레딧 올라가는 속도는 세 연출 모두 100px 속도로"에 맞춰 Cut3/Cut4/Cut5는
        // 전부 이 속도의 등속 스크롤이다. 재생 시간은 씬 ScrollContent에 선언된 Height(디자이너가
        // 직접 정한 콘텐츠 길이)에서 BuildScrollPlan이 산출하며, 아래 예산 상수는 시간을 강제하는
        // 값이 아니라 스토리보드 타임코드 대비 편차가 크면 경고만 띄우는 참고값이다.
        private const float ScrollSpeedPixelsPerSecond = 100f;
        private const float Cut3BudgetSeconds = 40.0f;
        private const float Cut4BudgetSeconds = 40.0f;
        private const float Cut5BudgetSeconds = 46.0f;
        private const float BudgetWarningToleranceSeconds = 1.0f;

        // ScrollContent 자식들의 실측 높이가 선언된 Height를 이 값(px) 이상 넘으면, 마지막 항목이
        // 박스 밖으로 잘릴 위험이 있다는 경고를 띄운다(연출 자체는 선언 Height 기준으로 그대로 진행).
        private const float ContentOverflowTolerancePixels = 2f;

        // Cut3~Cut5는 하나의 연속된 크레딧 롤로 이어붙인다(CoCreditRoll). 모든 컷이 화면 위로
        // 완전히 빠져나간 뒤 Cut6로 넘어가기 전 이 시간만큼 빈 화면을 유지한다.
        private const float RollEndHoldSeconds = 2.0f;

        // Cut2 화이트아웃 페이드와 Cut3 진입(아래→위)은 기본적으로 동시에 시작해 자연스럽게 이어지도록
        // 한다. 겹침이 지저분해 보이면 이 값을 0보다 크게 늘려 Cut3 시작을 그만큼 늦출 수 있다.
        private const float Cut3EntryDelaySeconds = 5.0f;

        // Cut4 dirty→clean 전환 트리거 지점(0=화면 하단, 1=화면 상단, 0.5=화면 중앙)과 전환 페이드 길이.
        // 튜닝이 필요할 일이 거의 없는 고정 연출값이라 Inspector로 노출하지 않는다.
        private const float Cut4TriggerViewportRatio = 0.5f;
        private const float Cut4DirtyFadeDuration = 1.0f;

        private const float Cut6TextFadeDuration = 2.0f;
        // Cut6은 헌사 문구를 띄운 뒤 엔딩 BGM이 끝날 때까지 대기하다 자동으로 복귀한다.
        // BGM이 이미 이 시점에 끝나 있거나 대사 직후 끝나더라도 최소 이 시간만큼은 화면을 유지한다.
        private const float Cut6MinHoldSeconds = 5.0f;
        private const string MainMenuSceneName = "MainMenuScene";

        private bool isReturningToMainMenu = false;
        private bool isCut6ClickRequested = false;
        private float bgmStartTime;
        private CanvasGroup[] cutGroups;

        /// <summary>
        /// 스크롤 1회 재생에 필요한 시작 위치/이동 거리/소요 시간을 담는 값 타입.
        /// BuildScrollPlan이 ScrollContent의 선언 Height로부터 계산해 반환한다.
        /// 콘텐츠는 화면 아래에서 완전히 진입해(StartAnchoredY) 화면 위로 완전히 이탈할 때까지
        /// (Distance) 이동하며, HandoffDistance는 그중 "콘텐츠 하단이 뷰포트 하단에 닿는" 지점으로
        /// 다음 컷을 이 지점에서 시작시키면 이음매 없이 이어붙일 수 있다.
        /// </summary>
        private readonly struct ScrollPlan
        {
            public readonly float StartAnchoredY;
            public readonly float Distance;
            public readonly float Duration;
            public readonly float HandoffDistance;
            public readonly bool IsValid;

            public ScrollPlan(float startAnchoredY, float distance, float duration, float handoffDistance, bool isValid)
            {
                StartAnchoredY = startAnchoredY;
                Distance = distance;
                Duration = duration;
                HandoffDistance = handoffDistance;
                IsValid = isValid;
            }
        }

        /// <summary>
        /// Cut4 dirty→clean 전환이 발화되는 scrollTarget.anchoredPosition.y 임계값과,
        /// 그 임계값에 대응하는 cut4DirtyImages 인덱스를 함께 담는 값 타입.
        /// </summary>
        private readonly struct Cut4Trigger
        {
            public readonly float TriggerAnchoredY;
            public readonly int DirtyIndex;

            public Cut4Trigger(float triggerAnchoredY, int dirtyIndex)
            {
                TriggerAnchoredY = triggerAnchoredY;
                DirtyIndex = dirtyIndex;
            }
        }

        private void Awake()
        {
            cutGroups = new[]
            {
                cut1CanvasGroup, cut2CanvasGroup, cut3CanvasGroup,
                cut4CanvasGroup, cut5CanvasGroup, cut6CanvasGroup
            };
        }

        private void Start()
        {
            InitializeCutPanels();

            if (skipButton != null) {
                skipButton.onClick.AddListener(OnSkipClicked);
            }

            if (cut6FinalClickButton != null) {
                cut6FinalClickButton.onClick.AddListener(OnFinalClickToMainMenu);
            }

            StartCoroutine(CoPlayEndingSequence());
        }

#if UNITY_EDITOR
        // [디버그] Tab을 누르고 있는 동안만 8배속. 씬 이탈 시 OnDisable에서 1.0으로 복구된다.
        private const float DebugSpeedMultiplier = 8f;
        private const KeyCode HoldSpeedKey = KeyCode.Tab;

        private void Update()
        {
            if (Input.GetKeyDown(HoldSpeedKey)) {
                Time.timeScale = DebugSpeedMultiplier;
            } else if (Input.GetKeyUp(HoldSpeedKey)) {
                Time.timeScale = 1f;
            }
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }
#endif

        /// <summary>
        /// 모든 Cut 패널을 alpha 0 / 입력 차단 상태로 강제 초기화한다.
        /// (씬에서 초기값 세팅을 놓쳐도 방어되도록 코드에서 한 번 더 보장)
        /// </summary>
        private void InitializeCutPanels()
        {
            foreach (CanvasGroup cg in cutGroups) {
                if (cg == null) continue;

                if (!cg.gameObject.activeSelf) {
                    cg.gameObject.SetActive(true);
                }

                SetCanvasGroupState(cg, 0f, false, false);
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
            yield return CoCut1_DoorAndShoes();
            yield return CoCut2_MirrorTiltAndWhiteOut();

            // Cut2 화이트아웃 페이드아웃과 Cut3 진입(아래→위)을 동시에 진행해 컷 전환이 뚝 끊기지
            // 않고 자연스럽게 이어지도록 한다(화이트아웃 완료를 기다리지 않고 바로 시작).
            StartCoroutine(CoCut2WhiteOut());

            if (Cut3EntryDelaySeconds > 0f) {
                yield return new WaitForSeconds(Cut3EntryDelaySeconds);
            }

            yield return CoCreditRoll();
            yield return CoCut6_Dedication();
        }

        private IEnumerator CoCut1_DoorAndShoes()
        {
            if (SoundManager.Instance != null) {
                SoundManager.Instance.PlaySFX(doorOpenSfx);
                SoundManager.Instance.PlayBGM(endingBgmClip);
            }

            // Cut6의 "BGM 종료까지 대기" 판정 기준점. SoundManager는 재생 위치를 노출하지 않으므로
            // 이 시작 시각과 endingBgmClip.length로 남은 시간을 역산한다.
            bgmStartTime = Time.time;

            yield return CoFadeCanvasGroup(cut1CanvasGroup, 0f, 1f, CutFadeDuration);

            for (int i = 0; i < cut1ScriptLines.Length; i++) {
                if (cut1ScriptText != null) {
                    cut1ScriptText.text = cut1ScriptLines[i];
                }

                yield return new WaitForSeconds(Cut1LineDisplayDuration);
            }

            yield return CoFadeCanvasGroup(cut1CanvasGroup, 1f, 0f, CutFadeDuration);
        }

        /// <summary>
        /// Cut2 페이드인 + 거울 Tilt-Down까지만 진행한다. 화이트아웃은 <see cref="CoCut2WhiteOut"/>이
        /// 별도로 맡아, 호출부(CoPlayEndingSequence)가 Cut3 진입과 동시에 병렬 실행할 수 있게 한다.
        /// </summary>
        private IEnumerator CoCut2_MirrorTiltAndWhiteOut()
        {
            yield return CoFadeCanvasGroup(cut2CanvasGroup, 0f, 1f, CutFadeDuration);

            yield return CoScrollUp(cut2TiltTarget, Cut2TiltDistance, Cut2TiltDuration);
        }

        /// <summary>
        /// Cut2 White-Out 페이드 아웃. Cut3 진입(아래→위)과 동시에 재생되어 "거울 화면이 흐려지는
        /// 동안 첫 코믹컷이 아래에서 떠오르는" 자연스러운 전환을 만든다.
        /// </summary>
        private IEnumerator CoCut2WhiteOut()
        {
            yield return CoFadeCanvasGroup(cut2CanvasGroup, 1f, 0f, Cut2WhiteOutFadeDuration);
        }

        /// <summary>
        /// Cut3~Cut5를 하나의 연속된 크레딧 롤로 이어붙여 재생한다. 각 컷은 화면 아래에서 올라와
        /// 화면 위로 완전히 빠져나갈 때까지 스크롤하며, 앞 컷의 콘텐츠 하단이 뷰포트 하단에 닿는
        /// 순간(HandoffDistance)에 다음 컷이 같은 자리에서 이어받아 컷 경계가 뚝 끊기지 않는다.
        /// </summary>
        private IEnumerator CoCreditRoll()
        {
            bool cut3HandoffReady = false;
            bool cut4HandoffReady = false;

            Coroutine cut3Routine = StartCoroutine(CoRunRollSegment(
                cut3CanvasGroup, cut3ScrollTarget, Cut3BudgetSeconds, "Cut3",
                () => cut3HandoffReady = true));
            yield return new WaitUntil(() => cut3HandoffReady);

            Coroutine cut4Routine = StartCoroutine(CoRunRollSegment(
                cut4CanvasGroup, cut4ScrollTarget, Cut4BudgetSeconds, "Cut4",
                () => cut4HandoffReady = true,
                BuildCut4ProgressFactory()));
            yield return new WaitUntil(() => cut4HandoffReady);

            // 마지막 컷은 다음으로 넘겨줄 대상이 없으므로 handoff 없이 끝까지(화면 밖 이탈까지) 재생한다.
            Coroutine cut5Routine = StartCoroutine(CoRunRollSegment(
                cut5CanvasGroup, cut5ScrollTarget, Cut5BudgetSeconds, "Cut5", null));

            // 세 세그먼트가 병렬로 실행 중이므로, 모두 실제로 끝날 때까지 순서대로 대기한다
            // (앞선 세그먼트는 handoff 시점에 이미 상당 부분 재생되었으므로 대개 즉시 반환된다).
            yield return cut3Routine;
            yield return cut4Routine;
            yield return cut5Routine;

            yield return new WaitForSeconds(RollEndHoldSeconds);
        }

        /// <summary>
        /// CanvasGroup 하나와 ScrollContent 하나를 "화면 아래 진입 → 등속 스크롤 → 화면 위 이탈"까지
        /// 재생하는 크레딧 롤 세그먼트. 대상이 없거나 레이아웃을 계산할 수 없으면 즉시 handoff를
        /// 신호하고 종료해, 오케스트레이터(CoCreditRoll)의 WaitUntil이 멈춰버리는 일을 막는다.
        /// onProgressFactory는 시작 위치가 확정된 뒤 뷰포트/시작좌표를 받아 프레임별 진행 콜백을
        /// 만들어 반환한다(Cut4의 dirty→clean 트리거 판정에 사용, 그 외는 null).
        /// </summary>
        private IEnumerator CoRunRollSegment(
            CanvasGroup cg,
            RectTransform scrollTarget,
            float budgetSeconds,
            string cutLabel,
            Action onHandoff,
            Func<RectTransform, float, Action<float>> onProgressFactory = null)
        {
            ScrollPlan plan = BuildScrollPlan(scrollTarget, budgetSeconds, cutLabel);

            if (!plan.IsValid || cg == null || scrollTarget == null) {
                onHandoff?.Invoke();
                yield break;
            }

            SetAnchoredY(scrollTarget, plan.StartAnchoredY);
            SetCanvasGroupState(cg, 1f, false, false);

            RectTransform viewport = scrollTarget.parent as RectTransform;
            Action<float> onProgress = onProgressFactory?.Invoke(viewport, plan.StartAnchoredY);

            bool handoffFired = false;
            float handoffProgress = plan.Distance > 0f ? Mathf.Clamp01(plan.HandoffDistance / plan.Distance) : 1f;

            yield return CoScrollUp(scrollTarget, plan.Distance, plan.Duration, t => {
                onProgress?.Invoke(t);

                if (!handoffFired && t >= handoffProgress) {
                    handoffFired = true;
                    onHandoff?.Invoke();
                }
            });

            // 부동소수 오차 등으로 진행률이 정확히 임계값을 지나치지 못했을 경우의 안전망.
            if (!handoffFired) {
                onHandoff?.Invoke();
            }

            SetCanvasGroupState(cg, 0f, false, false);
        }

        /// <summary>
        /// Cut4 세그먼트 전용 onProgress 팩토리. dirty/clean 레이어를 재생 시작 상태로 되돌리고,
        /// 확정된 시작 좌표로 트리거 지점을 계산한 뒤, 스크롤 진행에 따라 트리거를 순회하는
        /// 콜백을 반환한다(기존 dirty→clean 크로스페이드 트리거 판정 로직을 그대로 옮긴 것).
        /// </summary>
        private Func<RectTransform, float, Action<float>> BuildCut4ProgressFactory()
        {
            return (viewport, startAnchoredY) => {
                PrepareCut4Layers();

                Cut4Trigger[] triggers = BuildCut4Triggers(viewport, startAnchoredY);
                int triggerCursor = 0;

                return _ => {
                    if (cut4ScrollTarget == null) return;

                    float currentAnchoredY = cut4ScrollTarget.anchoredPosition.y;

                    // 스크롤이 단조 증가이므로 커서를 되돌릴 필요 없이 한 방향으로만 전진한다.
                    while (triggerCursor < triggers.Length && currentAnchoredY >= triggers[triggerCursor].TriggerAnchoredY) {
                        int dirtyIndex = triggers[triggerCursor].DirtyIndex;

                        if (cut4DirtyImages != null && dirtyIndex >= 0 && dirtyIndex < cut4DirtyImages.Length && cut4DirtyImages[dirtyIndex] != null) {
                            StartCoroutine(CoFadeGraphicAlpha(cut4DirtyImages[dirtyIndex], 1f, 0f, Cut4DirtyFadeDuration));
                        }

                        triggerCursor++;
                    }
                };
            };
        }

        /// <summary>
        /// Cut4 dirty/clean 쌍의 알파와 렌더 순서를 재생 시작 전 상태로 되돌린다.
        /// dirty가 clean 위에 그려지도록 sibling index를 강제해, 씬에 배치된 순서와 무관하게
        /// "더러운 방이 서서히 투명해지며 아래 깨끗한 방이 드러나는" 연출을 보장한다.
        /// </summary>
        private void PrepareCut4Layers()
        {
            if (cut4DirtyImages == null) return;

            for (int i = 0; i < cut4DirtyImages.Length; i++) {
                Image clean = (cut4CleanImages != null && i < cut4CleanImages.Length) ? cut4CleanImages[i] : null;
                Image dirty = cut4DirtyImages[i];

                if (clean != null) {
                    clean.transform.SetAsLastSibling();
                    SetGraphicAlpha(clean, 1f);
                }

                if (dirty != null) {
                    dirty.transform.SetAsLastSibling();
                    SetGraphicAlpha(dirty, 1f);
                }
            }
        }

        /// <summary>
        /// 각 dirty 이미지가 화면의 Cut4TriggerViewportRatio 지점을 지나는 시점의
        /// scrollTarget.anchoredPosition.y 값을 계산해 오름차순으로 정렬한 트리거 배열을 반환한다.
        /// (Cut4 세그먼트의 onProgress 콜백이 프레임당 1회 비교만으로 순회할 수 있도록 사전 정렬)
        /// </summary>
        private Cut4Trigger[] BuildCut4Triggers(RectTransform viewport, float startAnchoredY)
        {
            int pairCount = cut4DirtyImages != null ? cut4DirtyImages.Length : 0;
            var triggers = new Cut4Trigger[pairCount];

            if (viewport == null) {
                for (int i = 0; i < pairCount; i++) {
                    triggers[i] = new Cut4Trigger(float.PositiveInfinity, i);
                }

                return triggers;
            }

            float viewportHeight = viewport.rect.height;
            float triggerLineViewportY = viewport.rect.yMax - viewportHeight * Cut4TriggerViewportRatio;

            for (int i = 0; i < pairCount; i++) {
                Image dirty = cut4DirtyImages[i];

                if (dirty == null) {
                    triggers[i] = new Cut4Trigger(float.PositiveInfinity, i);
                    continue;
                }

                // 시작 위치(startAnchoredY) 기준 dirty의 뷰포트 로컬 y좌표를 구한 뒤,
                // scrollTarget 이동량 = 목표 y좌표 - 현재 y좌표 만큼 더한 지점이 발화 시점이다.
                float dirtyViewportYAtStart = RectTransformUtility
                    .CalculateRelativeRectTransformBounds(viewport, dirty.rectTransform).center.y;
                float triggerAnchoredY = startAnchoredY + (triggerLineViewportY - dirtyViewportYAtStart);

                triggers[i] = new Cut4Trigger(triggerAnchoredY, i);
            }

            Array.Sort(triggers, (a, b) => a.TriggerAnchoredY.CompareTo(b.TriggerAnchoredY));
            return triggers;
        }

        private IEnumerator CoCut6_Dedication()
        {
            // Cut5는 CoCreditRoll의 마지막 세그먼트로 이미 화면 위로 완전히 빠져나가며
            // cut5CanvasGroup이 alpha 0으로 정리된 상태이므로, 여기서 별도 페이드아웃이 필요 없다.

            if (cut6DedicationText != null) {
                SetGraphicAlpha(cut6DedicationText, 0f);
            }

            yield return CoFadeCanvasGroup(cut6CanvasGroup, 0f, 1f, CutFadeDuration);

            if (cut6DedicationText != null) {
                yield return CoFadeGraphicAlpha(cut6DedicationText, 0f, 1f, Cut6TextFadeDuration);
            }

            SetCanvasGroupState(cut6CanvasGroup, 1f, true, true);

            // BGM이 끝날 때까지 자동 대기하되(SoundManager가 재생 위치를 노출하지 않아 시작 시각+길이로 역산),
            // 최소 Cut6MinHoldSeconds는 보장한다. 버튼을 누르면 그 전에도 즉시 종료된다.
            float bgmRemaining = endingBgmClip != null ? endingBgmClip.length - (Time.time - bgmStartTime) : 0f;
            float holdDuration = Mathf.Max(Cut6MinHoldSeconds, bgmRemaining);
            float holdElapsed = 0f;

            while (holdElapsed < holdDuration && !isCut6ClickRequested) {
                holdElapsed += Time.deltaTime;
                yield return null;
            }

            ReturnToMainMenu();
        }

        private void OnFinalClickToMainMenu()
        {
            isCut6ClickRequested = true;
        }

        /// <summary>
        /// Skip 버튼 클릭 시 즉시 MainMenuScene으로 복귀한다.
        /// (기존 Botton_Back의 OnMouseDown_SwitchScene을 대체)
        /// </summary>
        public void OnSkipClicked()
        {
            StopAllCoroutines();
            ReturnToMainMenu();
        }

        /// <summary>
        /// MainMenuScene 복귀를 한 곳에서 처리한다(Skip / Cut6 자동·클릭 종료 공용).
        /// 중복 호출을 막고, 다른 씬들과 동일하게 SceneFlowManager 부재 시 SceneManager로 폴백한다.
        /// </summary>
        private void ReturnToMainMenu()
        {
            if (isReturningToMainMenu) return;
            isReturningToMainMenu = true;

            if (SceneFlowManager.Instance != null) {
                SceneFlowManager.Instance.LoadScene(MainMenuSceneName);
            } else {
                SceneManager.LoadScene(MainMenuSceneName);
            }
        }

        /// <summary>
        /// 0~1 진행률을 duration에 걸쳐 매 프레임 onStep으로 전달하는 공용 보간 코루틴.
        /// 페이드/스크롤 등 이 파일의 모든 시간 기반 연출이 이 코루틴 위에서 동작한다.
        /// </summary>
        private IEnumerator CoLerp(float duration, Action<float> onStep)
        {
            float elapsed = 0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                onStep(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            onStep(1f);
        }

        /// <summary>
        /// SceneFlowManager.CoFade의 Lerp 패턴을 차용한 Director 내부 전용 CanvasGroup 페이드 코루틴.
        /// </summary>
        private IEnumerator CoFadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            if (cg == null) yield break;

            yield return CoLerp(duration, t => cg.alpha = Mathf.Lerp(from, to, t));

            bool isVisible = to > 0f;
            cg.interactable = isVisible;
            cg.blocksRaycasts = isVisible;
        }

        /// <summary>
        /// 대상 RectTransform의 anchoredPosition.y를 정지-이동 반복 없이 한 번의 등속으로
        /// 위(+Y)로 스크롤하는 코루틴. Cut2 틸트다운, Cut3/Cut4/Cut5 크레딧 스크롤이 모두 이 방향을 공유한다.
        /// onProgress는 매 프레임 0~1 진행률을 전달받아 Cut4의 dirty→clean 트리거 판정 등에 쓰인다.
        /// </summary>
        private IEnumerator CoScrollUp(RectTransform target, float distance, float duration, Action<float> onProgress = null)
        {
            if (target == null) yield break;

            Vector2 startPos = target.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, distance);

            yield return CoLerp(duration, t => {
                target.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                onProgress?.Invoke(t);
            });
        }

        /// <summary>
        /// Image/TextMeshProUGUI 등 Graphic 공통 알파값을 Lerp로 페이드하는 내부 유틸 코루틴.
        /// (Cut4 dirty 이미지, Cut6 텍스트 페이드에서 공용으로 사용)
        /// </summary>
        private IEnumerator CoFadeGraphicAlpha(Graphic graphic, float from, float to, float duration)
        {
            if (graphic == null) yield break;

            yield return CoLerp(duration, t => SetGraphicAlpha(graphic, Mathf.Lerp(from, to, t)));
        }

        private void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null) return;

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private void SetAnchoredY(RectTransform target, float y)
        {
            if (target == null) return;

            Vector2 pos = target.anchoredPosition;
            pos.y = y;
            target.anchoredPosition = pos;
        }

        /// <summary>
        /// scrollTarget(부모를 뷰포트로 삼는 스크롤 컨테이너, pivot/anchor는 top-center(0.5,1) 고정)의
        /// 선언된 Height(sizeDelta.y)로부터 시작 위치/이동 거리/소요 시간을 계산한다.
        /// 콘텐츠는 뷰포트 아래에서 완전히 진입해(StartAnchoredY) 뷰포트 위로 완전히 이탈할 때까지
        /// 이동하므로 이동 거리 = 콘텐츠 Height + 뷰포트 높이이며, 속도는 ScrollSpeedPixelsPerSecond
        /// (100px/s) 고정이다. HandoffDistance(=콘텐츠 Height)는 콘텐츠 하단이 뷰포트 하단에 닿는
        /// 지점으로, 이 순간 다음 컷을 같은 자리에서 시작시키면 컷 사이가 끊김 없이 이어진다.
        /// Height가 아직 설정되지 않은 placeholder(뷰포트보다 작음) 상태라면 자식 실측 높이로
        /// 폴백하고 경고를 남긴다. 계산된 콘텐츠 통과 시간이 budgetSeconds와 BudgetWarningToleranceSeconds
        /// 이상 차이 나면 레이아웃 Height 조정이 필요하다는 경고를 남긴다.
        /// </summary>
        private ScrollPlan BuildScrollPlan(RectTransform scrollTarget, float budgetSeconds, string cutLabel)
        {
            if (scrollTarget == null) return default;

            RectTransform viewport = scrollTarget.parent as RectTransform;

            if (viewport == null) {
                Debug.LogWarning($"[EndingCreditsDirector] {cutLabel}: scrollTarget의 부모가 RectTransform이 아니라 뷰포트 크기를 알 수 없습니다. 스크롤이 발생하지 않습니다.");
                return default;
            }

            float viewportHeight = viewport.rect.height;
            float declaredHeight = scrollTarget.rect.height;

            // 자식들이 실제로 차지하는 범위는 scrollTarget 자신을 기준(root)으로 측정해,
            // scrollTarget의 anchoredPosition(곧 덮어쓸 값)과 무관한 "콘텐츠 자체 크기"를 얻는다.
            float measuredHeight = RectTransformUtility
                .CalculateRelativeRectTransformBounds(scrollTarget, scrollTarget).size.y;

            float contentHeight;

            if (declaredHeight < viewportHeight) {
                // Height 미설정(기본값 100 등) placeholder 상태로 판단해 실측값으로 폴백한다.
                contentHeight = Mathf.Max(measuredHeight, viewportHeight);
                Debug.LogWarning($"[EndingCreditsDirector] {cutLabel}: ScrollContent의 Height({declaredHeight:F0}px)가 뷰포트 높이({viewportHeight:F0}px)보다 작습니다. 실측 콘텐츠 높이({measuredHeight:F0}px)로 대체합니다 — ScrollContent의 Height를 콘텐츠 길이에 맞게 설정하세요.");
            } else {
                contentHeight = declaredHeight;

                if (measuredHeight > declaredHeight + ContentOverflowTolerancePixels) {
                    Debug.LogWarning($"[EndingCreditsDirector] {cutLabel}: 콘텐츠 실측 높이({measuredHeight:F0}px)가 ScrollContent의 Height({declaredHeight:F0}px)를 초과합니다. 마지막 항목이 박스 밖으로 밀려 조기에 사라질 수 있으니 Height를 늘리세요.");
                }
            }

            // 콘텐츠 상단이 뷰포트 하단에 걸린 위치(뷰포트 바로 아래, 완전히 가려진 상태)를 시작점으로 삼는다.
            // pivot(0.5,1) 기준 anchoredPosition.y는 곧 콘텐츠 상단의, 뷰포트 중심 기준 오프셋이다.
            float startAnchoredY = -viewportHeight;
            float distance = contentHeight + viewportHeight;
            float duration = distance / ScrollSpeedPixelsPerSecond;
            float handoffDistance = contentHeight;

            float scrollSeconds = contentHeight / ScrollSpeedPixelsPerSecond;
            float deviation = Mathf.Abs(scrollSeconds - budgetSeconds);
            if (deviation > BudgetWarningToleranceSeconds) {
                Debug.LogWarning($"[EndingCreditsDirector] {cutLabel} 콘텐츠 Height={contentHeight:F0}px, 통과 시간={scrollSeconds:F1}s(100px/s 기준) — 스토리보드 예산 {budgetSeconds:F0}s과 {deviation:F1}s 차이가 납니다. ScrollContent의 Height를 조정하세요.");
            }

            return new ScrollPlan(startAnchoredY, distance, duration, handoffDistance, true);
        }
    }
}

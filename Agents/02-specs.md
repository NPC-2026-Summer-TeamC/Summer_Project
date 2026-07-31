# 2) 기술 스펙 및 구현 규격 (Unity C#)

본 문서는 **"606호에 어서오세요" (Welcome606)** 프로젝트의 엔진 스펙, 아키텍처 패턴, 현재 구현 현황 및 C# 구현 규격을 정의합니다.

---

## A. Runtime & Core Engine Spec

| 구분 | 환경 및 기술 사양 |
| :--- | :--- |
| **Engine** | Unity 2022.3 LTS 이상 (URP 2D/3D 환경) |
| **Language** | C# (.NET Standard / Unity C# strict) |
| **UI System** | Unity Canvas UI + TextMesh Pro (`TMPro.TextMeshProUGUI`) |
| **Input System** | Unity Input System Package & 2D Mouse Pointer Event |
| **Persistence** | PlayerPrefs + JSON Serialization (`JsonUtility`) |

---

## B. Core 아키텍처 패턴 & 구현 현황

### 1) Persistence Architecture (`UserDataManager`) — [✅ 구현 완료]
- `UserData` 데이터 직렬화 모델: `StageProgress[] chapters` (5개 챕터 × 3개 스테이지 `bool[]`), `bool[] collectedItems`
- `UserDataManager` 싱글톤:
  - Lazy Singleton (`Instance`) 및 `DontDestroyOnLoad` 처리 완료.
  - JSON 기반 세이브/로드 (`PlayerPrefs.SetString("UserDataJson", json)`).
  - `ClearStage(chapter, stage)`: 특정 챕터/스테이지 클리어 여부 반영 및 자동 저장.
  - `CollectItem(chapter)` & `HasCollectedItem(chapter)`: 수집품 획득 상태 반영.
  - 앱 일시정지(`OnApplicationPause`) 및 종료(`OnApplicationQuit`) 시 자동 저장 연동.
- ⏳ **리팩토링 계획 (브랜치 `map-unlock`)**:
  - `maxUnlockChapter`, `maxUnlockStage` 2개 진행 변수 체계로 간소화.
  - UI 실시간 알림을 위한 `event Action OnUserDataChanged` 발송 로직 도입.

### 2) Scene & Context Management — [⏳ 진행 예정 (브랜치 `scene-flow`)]
- **용어 정의**: **챕터 = 맵** (Map01~05), **스테이지 = 실제 퍼즐 단계** (맵별 1~3번 퍼즐).
- `GameManager` 싱글톤: 최상위 씬 전이 컨텍스트(`selectedChapter`, `selectedStage`) 유지.
- `SceneFlowManager`: Overlay Canvas (`CanvasGroup`) 기반 페이드 비동기 씬 전이 및 광클 입력 차단 (`blocksRaycasts = true`).
- `MapNavigationController`: 유저 진행도(`IsChapterUnlocked`) 기반 이전/다음 맵 씬 이동 화살표 제어.
- `StageSelectController`: 현재 맵 내 1~3번 스테이지 진입 버튼 해금/숨김(`SetActive`) 및 퍼즐 씬 진입.

### 3) Puzzle Strategy Pattern (`PuzzleBoardController`) — [⏳ 진행 예정]
- `PuzzleBoardController`는 슬라이딩 퍼즐 그리드 생성 및 타일 이동 판정 담당.
- `StageData` (ScriptableObject) 퍼즐 설정 로드 및 `IPuzzleRule` 전략 인터페이스를 통한 챕터별 기믹 연동.

---

## C. C# 코딩 컨벤션 & Naming Rule

> [!IMPORTANT]
> - 모든 UI 텍스트는 Legacy `UnityEngine.UI.Text` 사용 금지 → 반드시 `TMPro.TextMeshProUGUI` 사용.
> - `Manager` 클래스 비대화(God Object) 방지: Pure C# 데이터는 `Data`, 파서/서비스는 `Service`, UI 및 개별 동작은 `Controller`로 책임 분할.

| 역할 분류 | 클래스 명명 접미사 | 주요 책임 및 특징 | 프로젝트 예시 |
| :--- | :--- | :--- | :--- |
| **Manager** | `*Manager` | 전역 도메인/시스템 총괄 싱글톤 | `UserDataManager` [완료], `DialogUIManager` [완료], `GameManager` [예정] |
| **Controller** | `*Controller` | 개별 GameObject 행동/입력 제어 (`MonoBehaviour`) | `MapNavigationController` [예정], `StageSelectController` [예정] |
| **Director** | `*Director` | 상위 흐름 및 연출 지휘 | `CutsceneDirector` [예정] |
| **Service / Parser**| `*Service` / `*Parser` | Pure C# 비-MonoBehaviour 로직/파싱 | `DialogueParser` [예정], `SaveService` [예정] |
| **Data / Config**| `*Data` / `*Config` | 데이터 직렬화 모델 & ScriptableObject | `UserData` [완료], `StageData` [예정] |
| **Handler** | `*Handler` / `OnMouseDown_*`| 이벤트 반응 헬퍼 | `OnMouseDown_SwitchScene` [완료] |


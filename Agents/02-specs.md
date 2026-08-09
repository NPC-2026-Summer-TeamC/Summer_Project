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

### 1) Persistence Architecture (`UserDataManager`) — 🟢
- `UserData` 데이터 직렬화 모델: `maxUnlockChapter`, `maxUnlockStage` 기반 간소화 진행도 체계 및 `collectedItems` 배열 (1~5 챕터 수집품 관리).
- `UserDataManager` 싱글톤:
  - Lazy Singleton (`Instance`) 및 `DontDestroyOnLoad` 세이프가드 처리 완료.
  - JSON 기반 세이브/로드 (`PlayerPrefs.SetString("UserDataJson", json)`).
  - `ClearStage(chapter, stage)`: 특정 챕터/스테이지 클리어 반영, 최고 진척도 자동 갱신 및 `OnUserDataChanged` 이벤트 발송.
  - `CollectItem(chapter)` & `HasCollectedItem(chapter)`: 1~5 챕터 수집품 해금 여부 판별.
  - 앱 일시정지(`OnApplicationPause`) 및 종료(`OnApplicationQuit`) 시 자동 저장 연동.

### 2) Scene & Context Management — 🟢
- **용어 정의**: **챕터 = 맵** (Map01~05), **스테이지 = 실제 퍼즐 단계** (맵별 1~3번 퍼즐).
- `GameManager` 싱글톤: 최상위 씬 전이 컨텍스트(`SelectedChapter`, `SelectedStage`) 보존 및 멀티 씬 전환 상태 제어.
- `SceneFlowManager` 싱글톤: CanvasGroup Overlay 기반 비동기 페이드 In/Out 씬 전이 및 로딩 중 입력 차단 (`blocksRaycasts = true`).
- `MapNavigationController`: `UserDataManager.Instance.MaxUnlockChapter` 기반 이전/다음 맵 이동 화살표 비활성화/해금 처리 및 페이드 씬 전환 연동.
- `StageSelectController`: 현재 선택된 챕터(`GameManager.Instance.SelectedChapter`)의 1~3번 퍼즐 버튼 해금/숨김(`SetActive`) 및 퍼즐 씬 진입 제어.
- `StageManager`: 퍼즐 씬 라이프사이클 관할, 퍼즐 완료 시 `UserDataManager.Instance.ClearStage` 자동 연동, 클리어 팝업 연출 및 맵 화면 비동기 복귀.

### 3) Sound Architecture (`SoundManager` & `SoundSettingModalController`) — 🟢
- `SoundManager`: Master/BGM/SFX 음량 제어(`MasterVolume`, `BGMVolume`, `SFXVolume`), `PlaySFX` 및 PlayerPrefs 저장 연동.
- `SoundSettingModalController`: 마스터, 효과음, 배경음악 3종 슬라이더 및 퍼센트(`80%`) 실시간 동기화, PlayerPrefs 저장 & 진행도 초기화(`OnResetProgressClicked`) 기능 연동.

### 4) Collection Item Architecture (`ItemPhotoController`) — 🟢
- `ItemPhotoController`: `UserDataManager.HasCollectedItem(chapter)` 기반 1~5 챕터 수집 아이템 슬롯 해금/잠금 제어 (`RefreshUI`).
- 아이템 클릭 시 스토리 대사 모달(`DialogModal_PF`) 연동 및 대사 종료 시 모달 자동 비활성화, 닫기 클릭 시 페이드 비동기 맵 복귀.

### 5) Puzzle Core & Validation Architecture (`BoardManager` & `ValidationManager`) — 🟡
- `BoardManager`, `TileController`, `DragInputManager`, `BagRandomizer`: 슬라이딩 퍼즐 그리드 생성, 타일 선택/드래그 입력 및 BagRandomizer 색상 배치 연동 완료.
- `ShapeFinder`: BFS 기반 연결된 타일 클러스터/모양 탐색 알고리즘 완료.
- `ValidationManager`, `AllColoredValidator`, `TargetShapeValidator`: 전략적 퍼즐 조건 판정 엔진 구축 완료 (PR #91).
> [!TODO]
- 15개 스테이지 `StageData` SO 구축, 챕터별 기믹(`IPuzzleRule`), ActionLog 기반 Undo/Redo 연동 예정.

### 6) Chapter 6 Ending Quest Architecture (`EndingQuestController`) — 🟢
- `EndingQuestController`, `EndingCollectibleItem`, `EndingTodoUIController`: 5개 챕터 클리어 후 해금되는 챕터 6 (`isEnding = true`), Todo 1개 순차 노출 UI, 엔딩 수집 완료 시 자동 `EndingEvent` (고정속도 Cutscene 연출) 및 `CreditsScene` 페이드 비동기 전환 완료.
---

## C. C# 코딩 컨벤션 & Naming Rule

> [!IMPORTANT]
> - 모든 UI 텍스트는 Legacy `UnityEngine.UI.Text` 사용 금지 ➔ 반드시 `TMPro.TextMeshProUGUI` 사용.
> - `Manager` 클래스 비대화(God Object) 방지: Pure C# 데이터는 `Data`, 파서/서비스는 `Service`, UI 및 개별 동작은 `Controller`로 책임 분할.

- **Manager** (`*Manager`)
  - 전역 도메인/시스템 총괄 싱글톤
  - `UserDataManager` 🟢, `DialogUIManager` 🟢, `GameManager` 🟢, `SceneFlowManager` 🟢, `StageManager` 🟡, `SoundManager` 🟡

- **Controller** (`*Controller`)
  - 개별 GameObject/UI 행동 및 사용자 입력 제어 (`MonoBehaviour`)
  - `MapNavigationController` 🟡, `StageSelectController` 🟢, `SoundSettingModalController` 🟢, `ItemPhotoController` 🟡, `PuzzleBoardController` 🔴

- **Director** (`*Director`)
  - 상위 흐름 및 컷씬 연출 지휘
  - `CutsceneDirector` 🔴

- **Service / Parser** (`*Service`, `*Parser`)
  - Pure C# 비-MonoBehaviour 데이터 파싱 및 로직 처리
  - `DialogueParser` 🟢, `SaveService` 🔴

- **Data / Config** (`*Data`, `*Config`)
  - 데이터 직렬화 모델 & ScriptableObject
  - `UserData` 🟢, `StageData` 🔴

- **Helper / Listener** (`*Helper`, `OnMouseDown_*`)
  - 이벤트 반응 및 런타임/에디터 테스트 헬퍼
  - `MapUnlockDebugHelper` 🟢, `StageManagerDebugHelper` 🟢, `OnMouseDown_SwitchScene` 🟢

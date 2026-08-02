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

### 2) Puzzle System & Tile Coloring — [⏳ 진행 중 (#67, #73, #75)]
- `BoardManager`, `BoardData`, `TileData`, `RuntimeState`: 퍼즐 보드 생성 및 좌표 기반 타일 조회 판정 (`#67`).
- `DragInputManager`: 드래그 시작/진행/종료 감지, `dragTileList` 중복 선택 방지 및 타일 색칠/상태 갱신 (`#73`).
- Bag Randomizer: 16개 색상 무작위 셔플 bag 생성 및 색상 공급/리필 연동 (`#75`).

### 3) Dialogue & Story Presentation — [⏳ 진행 중 (#68, #71)]
- `DialogueParser` & `DialogueData`: CSV 기반 대사 로드 및 데이터 바인딩.
- `DialogueManager` & `ScriptSettingController`: 타이핑 연출, Auto/Skip (읽은 텍스트만 / 모든 텍스트) 제어 (`#68`).
- `LogModalController`: 지난 대사 기록 조회.
- UI 아트 연동: `DialogModal_PF.prefab` 대사창 아트 적용 및 레이아웃 연동 패스 (`#70`, `#71`).

### 4) Sound Management (`SoundManager`) — [⏳ 진행 중 (#65)]
- BGM 및 SFX 사운드 리소스 연결, 피치 및 볼륨 조절 싱글톤 연동.

---

## C. C# 코딩 컨벤션 & Naming Rule

> [!IMPORTANT]
> - 모든 UI 텍스트는 Legacy `UnityEngine.UI.Text` 사용 금지 → 반드시 `TMPro.TextMeshProUGUI` 사용.
> - `Manager` 클래스 비대화(God Object) 방지: Pure C# 데이터는 `Data`, 파서/서비스는 `Service`, UI 및 개별 동작은 `Controller`로 책임 분할.

| 역할 분류 | 클래스 명명 접미사 | 주요 책임 및 특징 | 프로젝트 예시 |
| :--- | :--- | :--- | :--- |
| **Manager** | `*Manager` | 전역 도메인/시스템 총괄 싱글톤 | `UserDataManager` [완료], `BoardManager` [진행중], `SoundManager` [진행중] |
| **Controller** | `*Controller` | 개별 GameObject 행동/입력 제어 (`MonoBehaviour`) | `MapNavigationController` [완료], `ScriptSettingController` [진행중] |
| **Director** | `*Director` | 상위 흐름 및 연출 지휘 | `CutsceneDirector` [예정] |
| **Service / Parser**| `*Service` / `*Parser` | Pure C# 비-MonoBehaviour 로직/파싱 | `DialogueParser` [완료], `SaveService` [예정] |
| **Data / Config**| `*Data` / `*Config` | 데이터 직렬화 모델 & ScriptableObject | `UserData` [완료], `BoardData` [완료], `TileData` [완료] |
| **Handler** | `*Handler` / `OnMouseDown_*`| 이벤트 반응 헬퍼 | `OnMouseDown_SwitchScene` [완료] |
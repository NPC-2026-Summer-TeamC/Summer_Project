# 2) 기술 스펙 및 구현 규격 (Unity C#)

본 문서는 **"606호에 어서오세요" (Welcome606)** 프로젝트의 엔진 스펙, 아키텍처 패턴 및 C# 구현 규격을 정의합니다.

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

## B. Core 아키텍처 패턴

### 1) Scene & Context Management (`GameManager`)
- `GameManager` 싱글톤이 최상위 씬 이동 컨텍스트(`selectedChapter`, `selectedStage`)를 유지.
- `SceneFlowManager`를 통해 `MainMenuScene` ↔ `Map01~05Scene` ↔ `StageEntryScene` ↔ `StageScene` 전이 제어.

### 2) Persistence Architecture (`UserDataManager`)
- `UserDataManager`가 데이터 저장/불러오기 (`Save()`, `Load()`), 초기화 (`ResetProgress()`), 스테이지 클리어 (`ClearStage()`), 아이템 획득 (`CollectItem()`) 캡슐화.
- 애플리케이션 일시정지 (`OnApplicationPause`), 종료 (`OnApplicationQuit`) 시 자동 저장.

### 3) Puzzle Strategy Pattern (`PuzzleBoardController`)
- `PuzzleBoardController`는 슬라이딩 퍼즐 그리드 생성 및 타일 이동 판정을 담당.
- `StageData` (ScriptableObject)로부터 퍼즐 설정을 읽어오고, `IPuzzleRule` 전략 인터페이스를 통해 챕터별 특수 기믹(장애물, 역방향 등)을 주입받아 수행.

---

## C. C# 코딩 컨벤션 & Naming Rule

> [!IMPORTANT]
> - 모든 UI 텍스트는 Legacy `UnityEngine.UI.Text` 사용 금지 → 반드시 `TMPro.TextMeshProUGUI` 사용.
> - `Manager` 클래스 비대화(God Object) 방지: Pure C# 데이터는 `Data`, 파서/서비스는 `Service`, UI 및 개별 동작은 `Controller`로 책임 분할.

| 역할 분류 | 클래스 명명 접미사 | 주요 책임 및 특징 | 프로젝트 예시 |
| :--- | :--- | :--- | :--- |
| **Manager** | `*Manager` | 전역 도메인/시스템 총괄 싱글톤 | `UserDataManager`, `GameManager`, `DialogManager` |
| **Controller** | `*Controller` | 개별 GameObject 행동/입력 제어 (`MonoBehaviour`) | `PuzzleBoardController`, `SettingModalController` |
| **Director** | `*Director` | 상위 흐름 및 연출 지휘 | `CutsceneDirector` |
| **Service** | `*Service` / `*Parser` | Pure C# 비-MonoBehaviour 로직/파싱/API | `DialogueParser`, `SaveService` |
| **Data / Config**| `*Data` / `*Config` | 데이터 직렬화 모델 & ScriptableObject | `UserData`, `StageData`, `DialogData` |
| **Handler** | `*Handler` / `OnMouseDown_*`| 이벤트 반응 헬퍼 | `OnMouseDown_SwitchScene` |


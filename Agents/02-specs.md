# 2) 기술 스펙 및 구현 규격 (Unity C#)

본 문서는 **"606호에 어서오세요" (Welcome606)** 프로젝트의 엔진 스펙, 아키텍처 패턴 및 구현 표준을 정리합니다.

---

## A. Runtime / Core & Engine

- **엔진**: Unity 2022.3 LTS 이상 (Universal Render Pipeline - URP 2D/3D)
- **언어**: C# (.NET Standard / Unity C# strict)
- **UI 시스템**: Unity Canvas UI + TextMesh Pro (TMP)
- **입력 시스템**: Unity Input System Package / Legacy Mouse Interaction

---

## B. core 아키텍처 패턴

### 1) Scene & State Management (싱글톤 패턴)
- **GameManager & SceneFlowManager**:
  - `GameManager` 싱글톤이 현재 `selectedChapter`, `selectedStage` 등 씬 이동 컨텍스트를 유지.
  - `SceneFlowManager`를 통해 `MainMenuScene` -> `Map01~05Scene` -> `StageEntryScene` -> `StageScene` 간의 데이터 손실 없는 전이를 처리.

### 2) User Data & Persistence (`UserDataManager`)
- **JSON + PlayerPrefs**:
  - `UserDataManager` 내에 `Save()`, `Load()`, `ResetProgress()`, `ClearStage()`, `CollectItem()` 캡슐화.
  - 해금 검증 메서드 (`IsStageUnlocked(chapter, stage)`, `IsChapterUnlocked(chapter)`) 지원.
  - 애플리케이션 종료(`OnApplicationQuit`), 일시정지(`OnApplicationPause`) 시 자동 저장.

### 3) UI & Dialog Architecture (Modal Stack + Manager)
- **DialogManager**: 대사 진행(Auto, Skip, Previous, Log) 및 대사 데이터 바인딩 총괄.
- **DialogUIManager**: UI 모달 Stack 관리 (설정 탭, 대사 로그 팝업, 아이템 리스트 팝업 제어).
- **SettingModal**: 볼륨 슬라이더 및 **게임 진행도 초기화** (UserDataManager.Instance.ResetProgress() 호출) 처리.

### 4) Puzzle Engine Structure (Strategy Pattern)
- **PuzzleBoardController**: N x M 타일 슬라이딩 그리드 생성, 이동 판정, 완성 여부 검사 코어.
- **StageData (ScriptableObject)**: 퍼즐 초기 배치, 타일 이미지, 목표 상태 정의.
- **IPuzzleRule (전략 패턴)**: 챕터 1~5별 특수 퍼즐 기믹(장애물, 역방향 이동, 이동 횟수 제한 등)을 전략 객체로 확장.

---

## C. 코딩 컨벤션 & 구현 규칙

1. **싱글톤 생성**: `UserDataManager.Instance`, `GameManager.Instance` 호출 시 DontDestroyOnLoad 적용 및 안전한 Quitting 체크 처리.
2. **UI Text**: 모든 UI 텍스트는 `TMPro.TextMeshProUGUI` 사용 (Legacy Text 사용 금지).
3. **데이터 보존**: `PlayerPrefs.SetString("UserDataJson", json)` 저장 방식 유지 및 필드 추가 시 `JsonUtility.FromJsonOverwrite` 활용.

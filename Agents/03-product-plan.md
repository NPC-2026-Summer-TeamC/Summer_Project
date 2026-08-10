# 3) 게임 기획 및 씬 플레이 루프 기획안

본 문서는 스토리 기반 15개 타일 퍼즐 게임 **"606호에 어서오세요" (Welcome606)**의 핵심 기획, 씬 구조 및 게임 플레이 루프를 정의합니다.

---

## A. 게임 개요 및 용어 정의

- **용어 정의**: 
  - **챕터 (Chapter) = 맵 (Map)** (1~5 챕터)
  - **스테이지 (Stage) = 실제 퍼즐 단계** (맵별 1~3번 퍼즐)
- **콘셉트**: "606호"라는 미스터리 공간을 배경으로 펼쳐지는 스토리 연출 및 타일 퍼즐 게임.
- **볼륨**: 5개 챕터(맵) × 챕터별 3개 스테이지(퍼즐 단계) = **총 15개 퍼즐 스테이지**.
- **핵심 루프**: 맵 탐색 ➔ 퍼즐 진입 & 클리어 ➔ 스토리 대사 연출 ➔ 수집 아이템 해금 ➔ 다음 맵 이동.

---

## B. 씬 & UI 플레이 흐름도

```txt
[타이틀 화면 (MainMenuScene)]
       │
       ▼
 [맵 화면 (Map01Scene ~ Map05Scene)] ◄─── (MapNavigationController: 좌/우 이동 화살표 제어)
       │  └─ StageSelectController: 1~3번 스테이지(퍼즐) 버튼 해금/숨김(SetActive) 및 씬 진입 제어
       │
       ├───► [퍼즐 진입 상호작용] ──► [스테이지 선택 (StageEntryScene)] ──► [퍼즐 플레이 (StageScene)]
       │                                                                         │ (클리어 시 진행도 저장)
       │                                                                         ▼
       ├───► [대사/스토리 연출 (DialogModal_PF)] ◄────────────────────────────────┘
       │        ├─ Auto/Skip/Previous/Log 기능
       │        └─ 스크립트 전용 설정 탭
       │
       ├───► [아이템 상호작용 (퍼즐 3개 클리어 시 활성화)] ──► [아이템 리스트 팝업 (ItemListScene/Modal)]
       │
       └─► [환경설정 (SettingModal_PF)] ──► 볼륨 조절 & 게임 진행도 초기화
```

---

## C. 씬 및 주요 UI 기능 명세 현황

### `MainMenuScene.unity`: 🟢 85%
- `UserDataManager`, `SettingModalController`
- 게임 스타트, 최근 진행 위치 맵 이동, 환경설정 팝업
> [!TODO]
> 기능 연동 완료, 메인 연출 폴리싱 필요

### `Map01~05Scene.unity`: 🟡 60%
- `MapNavigationController`, `StageSelectController`, `SceneFlowManager`
- 맵 탐색, 1~3번 퍼즐 버튼 해금, 이전/다음 맵 이동
> [!TODO]
> 해금/씬 이동 완료, 맵 대사/청소 상태 연동 대기

### `StageEntryScene.unity`: 🟢 85%
- `StageSelectController`, `UserDataManager`, `GameManager`
- 챕터별 3개 스테이지 중 해금된 퍼즐 단계 선택
> [!TODO]
> 버튼 해금 완료, 디스플레이 연출 보완 필요

### `StageScene.unity`: 🟡 60%
- `BoardManager`, `ValidationManager`, `StageManager`
- 15개 타일 퍼즐 판정, 검증 엔진 연동, 챕터별 기믹 전략 연동 및 클리어 저장
> [!TODO]
> Validation 엔진 완료 (PR #91), 15개 스테이지 SO 데이터 작성 및 기믹 룰셋 구현 대기

### `ItemPhotoScene.unity`: 🟢 80%
- `ItemPhotoController`, `DialogueManager`, `UserDataManager`
- 획득한 1~5 챕터 수집 아이템 조회 및 스토리 대사 연동
> [!TODO]
> 대사 연동 완료, 에셋 매핑 마무리 필요

### `CreditsScene.unity`: 🟢 80%
- `SceneFlowManager`, `EndingQuestController`
- 챕터 6 엔딩 퀘스트 완료 후 크레딧 연출 씬 전이 및 출력
> [!TODO]
> 씬 에셋 및 엔딩 퀘스트 연동 완료, 크레딧 연출 polish 대기

### `PF_DialogModalScene.unity`: 🟢 85%
- `DialogUIManager`, `DialogueManager`
- 스토리 대사 팝업 모달 독립 연출 테스트
> [!TODO]
> 대사 연동 완료, 세부 예외처리 필요

### `PF_SoundSettingScene.unity`: 🟢 85%
- `SoundSettingModalController`, `SoundManager`
- 사운드 환경설정 3종 슬라이더 독립 연출 테스트
> [!TODO]
> 3종 음량 및 리셋 완료, 사운드 믹서 대기

### `DialogModal_PF.prefab`: 🟢 85%
- `DialogUIManager`, `DialogueManager`
- 캐릭터 일러스트/대사 연출 팝업 (Auto/Skip/Log)
> [!TODO]
> 프리팹 완성, 컷씬 전용 모드 대기

### `LogModal_PF.prefab`: 🟢 90%
- `DialogUIManager.OpenLogModal()`
- 지나간 대사 기록 조회 모달
> [!TODO]
> 기존 대사 복원 및 로그 출력 완료

### `SettingModal_PF.prefab`: 🟢 85%
- `DialogUIManager.OpenSettingModal()`
- 대사 스크립트 전용 설정 모달
> [!TODO]
> 기초 옵션 완비

### `SoundSettingModal_PF.prefab`: 🟢 90%
- `SoundSettingModalController`, `SoundManager`
- 음량 3종 조절(마스터/BGM/SFX) & 진행도 초기화
> [!TODO]
> 독립 프리팹 완성

---

## D. 역할 기반 클래스 네이밍 가이드 & C# 모듈 현황

> [!TIP]
> 본 프로젝트의 모든 C# 스크립트는 단일 책임 원칙(SRP)과 역할 기반 접미사(Suffix) 규격을 엄격히 준수하며, 특정 매니저에 로직이 비대해지는 **God Object** 작성을 금지합니다.

### 1. 역할 분류 접미사 (Naming Rules)
- **Manager** (`*Manager`): 전역 도메인/시스템 총괄 싱글톤
- **Controller** (`*Controller`): 개별 GameObject/UI 행동 및 사용자 입력 제어
- **Director** (`*Director`): 컷씬 및 상위 스크립트 연출 지휘
- **Service / Parser** (`*Service`, `*Parser`): 비-MonoBehaviour 데이터 파싱 및 순수 로직
- **Data / Config** (`*Data`, `*Config`): 데이터 직렬화 모델 및 ScriptableObject
- **Helper / Listener** (`*Helper`, `OnMouseDown_*`): 이벤트 반응 및 런타임/에디터 테스트 헬퍼

---

### 2. C# 스크립트 모듈 구현 현황

#### System Managers (`Scripts/Managers`, `Scripts/Core`, `Scripts/Sound`)

### `UserDataManager.cs`: 🟢 85%
- `UserData`, `SaveService`
- 세이브/로드, 최고 진척도 갱신 & `OnUserDataChanged` 이벤트 제어
> [!TODO]
> 저장 파일 암호화 및 자동 백업 세이프가드 연동

### `GameManager.cs`: 🟢 80%
- `UserDataManager`
- 최상위 씬 간 선택 챕터/스테이지 컨텍스트(`SelectedChapter`, `SelectedStage`) 보존 싱글톤
> [!TODO]
> 예외 씬 재진입 시 안전 초기화 로직 보완

### `SceneFlowManager.cs`: 🟢 85%
- `CanvasGroup`
- CanvasGroup Overlay 기반 비동기 페이드 In/Out 씬 전환 및 입력 차단 싱글톤
> [!TODO]
> 커스텀 페이드 연출 타임라인 보완

### `StageManager.cs`: 🟡 70%
- `UserDataManager`, `GameManager`
- 퍼즐 씬 런타임 클리어 처리, `UserDataManager.ClearStage` 연동 & 맵 복귀
> [!TODO]
> 클리어 승리 연출 팝업 및 별점/점수 산정 시스템 연동

### `SoundManager.cs`: 🟡 65%
- `SoundSettingModalController`
- Master/BGM/SFX 음량 제어 및 SFX 재생, PlayerPrefs 저장 연동 (단일 BGM)
> [!TODO]
> UI/퍼즐/대사 SFX 재생 이벤트 바인딩

### `DialogUIManager.cs`: 🟢 80%
- `DialogueManager`, `DialogueParser`
- 대사 팝업, 로그 모달, 설정 모달 Open/Close UI 총괄 제어
> [!TODO]
> 컷씬 전용 모드 대사 연출 제어 보완

#### Component Controllers (`Scripts/UI`, `Scripts/Puzzle`)

### `MapNavigationController.cs`: 🟡 75%
- `UserDataManager`, `SceneFlowManager`
- 유저 진행도 기반 이전/다음 맵 이동 화살표 해금 연동 & 페이드 씬 전이
> [!TODO]
> 맵 클리어 후 맵 배경 더러움 ➔ 깨끗함 visual switch 연동

### `StageSelectController.cs`: 🟢 80%
- `UserDataManager`, `GameManager`
- 챕터 내 1~3번 퍼즐 진입 버튼 해금/숨김(`SetActive`) 및 퍼즐 씬 진입
> [!TODO]
> 선택한 스테이지 정보 팝업 연출 보완

### `SoundSettingModalController.cs`: 🟢 85%
- `SoundManager`
- 사운드 3종 슬라이더, 실시간 % 텍스트, PlayerPrefs 저장/리셋 제어
> [!TODO]
> 오디오 믹서 세부 그룹 연결 최신화

### `ItemPhotoController.cs`: 🟡 75%
- `UserDataManager`, `DialogueManager`
- 1~5 챕터 수집 아이템 해금 인터랙션, 스토리 대사 연동 및 맵 복귀
> [!TODO]
> 수집품 상세 이미지 에셋 매핑 마무리

### `BoardManager.cs` / `ValidationManager.cs`: 🟡 60%
- `StageData`, `IPuzzleRule`, `IValidator`
- 퍼즐 타일 선택/슬라이딩 매칭, BagRandomizer 배치, BFS 모양 탐색 및 `ValidationManager` 조건 검증 엔진
> [!TODO]
> Validation 엔진 완료 (PR #91), 15개 스테이지 SO 데이터 및 챕터별 기믹 전략 구현

### `EndingQuestController.cs`: 🟢 90%
- `EndingCollectibleItem`, `EndingTodoUIController`, `UserDataManager`
- 챕터 6 Todo 4단계 수집 퀘스트, 엔딩 BGM 전환 및 `CreditsScene` 페이드 전이
> [!TODO]
> 연출 텍스트 다이얼로그 가독성 보완

#### Services & Data Models (`Scripts/Core`, `Scripts/Dialogue`, `Scripts/Puzzle`)

### `UserData.cs`: 🟢 90%
- `UserDataManager`
- 챕터/스테이지 진행도 및 수집 아이템 획득 상태 직렬화 데이터 모델
> [!TODO]
> 백업 직렬화 필드 추가 검토

### `DialogueParser.cs`: 🟢 85%
- `DialogueManager`
- CSV/Text 기반 스토리 대사 파싱 및 스크립트 데이터 구조화
> [!TODO]
> 대사 엑셀 파싱 예외 처리 보완

### `StageData.cs`: 🔴 20%
- `PuzzleBoardController`
- 퍼즐 스테이지별 보드 크기, 타일 layout, 기믹 룰셋 데이터 정의 ScriptableObject
> [!TODO]
> 15개 스테이지 SO 데이터 파일 작성

#### Utils & Test Helpers (`Scripts/Tests`, `Scripts/UI`)

### `MapUnlockDebugHelper.cs`: 🟢 90%
- `UserDataManager`
- 실시간 챕터/스테이지 해금 테스트용 런타임/에디터 헬퍼
> [!TODO]
> 디버그 GUI 단축키 정리

### `StageManagerDebugHelper.cs`: 🟢 90%
- `StageManager`
- 퍼즐 씬 강제 클리어('C') 및 맵 복귀('R') 테스트 헬퍼
> [!TODO]
> 디버그 단축키 화면 안내 보완

### `OnMouseDown_SwitchScene.cs`: 🟢 85%
- `SceneFlowManager`
- 2D 오브젝트 클릭 시 단순 씬 전환 헬퍼
> [!TODO]
> 클릭 효과음 재생 연결

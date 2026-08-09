# 1) 폴더 및 모듈 아키텍처 (Unity C#)

본 문서는 **"606호에 어서오세요" (Welcome606)** 프로젝트의 유니티 C# 폴더 구조, 파일 배치 및 모듈 책임을 정의합니다.

---

## A. Top-Level 폴더 구조

```txt
Summer_Project
├─ Agents                                 # 에이전트 작업 가이드 및 문서
│  ├─ 01-folder-architecture.md
│  ├─ 02-specs.md
│  ├─ 03-product-plan.md
│  ├─ reports/                             # 작업 리포트 기록
│  └─ todo/                                # 작업 TODO 관리
│
└─ Welcome606                             # Unity 메인 프로젝트
   └─ Assets
      ├─ Prefabs                          # 재사용 UI 모달 프리팹 🟢
      │  ├─ DialogModal_PF.prefab          # 스토리 대사 연출 모달 🟢
      │  ├─ LogModal_PF.prefab             # 대사 로그 모달 🟢
      │  ├─ SettingModal_PF.prefab         # 대사 설정 모달 🟢
      │  └─ SoundSettingModal_PF.prefab    # 독립 사운드 환경설정 모달 🟢
      │
      ├─ Scenes                           # 유니티 씬 🟢
      │  ├─ MainMenuScene.unity            # 타이틀 메인 화면 🟢
      │  ├─ Map01Scene~Map05Scene.unity    # 5개 챕터 맵 화면 🟡
      │  ├─ StageEntryScene.unity          # 3개 퍼즐 스테이지 선택 화면 🟢
      │  ├─ StageScene.unity               # 퍼즐 인게임 씬 🔴
      │  ├─ ItemPhotoScene.unity           # 수집 아이템 보관함 씬 🟢
      │  ├─ CreditsScene.unity             # 엔딩 크레딧 화면 🔴
      │  ├─ PF_DialogModalScene.unity      # 대사 모달 연출 테스트 씬 🟢
      │  └─ PF_SoundSettingScene.unity     # 사운드 설정 연출 테스트 씬 🟢
      │
      ├─ Scripts                          # C# 스크립트 모듈 🟢
      │  ├─ Core                          # 데이터 & 핵심 매니저 (UserDataManager.cs 🟢)
      │  ├─ Dialogue                      # 대사 시스템 (DialogueManager.cs 🟢)
      │  ├─ Managers                      # 전역 싱글톤 (GameManager.cs, SceneFlowManager.cs, StageManager.cs 🟢/🟡)
      │  ├─ Sound                         # 사운드 관리 (SoundManager.cs 🟡)
      │  ├─ UI                            # UI 컨트롤러 (MapNavigationController.cs, StageSelectController.cs 🟢/🟡)
      │  ├─ Puzzle                        # 퍼즐 코어 엔진 (Board, Input, Shape, Validation 🟡)
      │  ├─ Ending                        # 챕터 6 엔딩 퀘스트 시스템 (EndingQuestController.cs 🟢)
      │  └─ Tests                         # 단위/런타임 디버그 헬퍼 (MapUnlockDebugHelper.cs 🟢)
      │
      ├─ Settings / Sprites / Audio       # URP 설정, 스프라이트, 오디오 리소스
      └─ TextMesh Pro                     # TMP 폰트 리소스
```

---

## B. Scripts 모듈 책임 & Naming Convention

### `Scripts/Core`: 🟢 85%
- `UserData.cs`, `UserDataManager.cs`
- 유저 진행도(챕터/스테이지/수집품) JSON 세이브/로드 & 해금 이벤트 발송 싱글톤
> [!TODO]
> 세이브 파일 암호화 및 자동 백업 세이프가드 연동

### `Scripts/Managers`: 🟢 80%
- `GameManager.cs`, `SceneFlowManager.cs`, `StageManager.cs`, `DialogUIManager.cs`
- 전역 선택 컨텍스트, 비동기 페이드 씬 전환, 퍼즐 런타임 클리어 판정 및 대사 UI 총괄 싱글톤
> [!TODO]
> 클리어 승리 연출 및 예외 씬 재진입 안전 로직 보완

### `Scripts/Sound`: 🟡 65%
- `SoundManager.cs`
- Master/BGM/SFX 음량 제어 및 SFX 재생, PlayerPrefs 저장 연동 (단일 BGM)
> [!TODO]
> UI/퍼즐/대사 SFX 재생 이벤트 바인딩

### `Scripts/UI`: 🟡 75%
- `MapNavigationController.cs`, `StageSelectController.cs`, `SoundSettingModalController.cs`, `ItemPhotoController.cs`
- 맵 간 이동, 스테이지 진입 버튼 해금, 사운드 모달 & 수집품 보관함 UI 제어
> [!TODO]
> 맵 클리어 후 맵 배경 더러움 ➔ 깨끗함 visual switch 연동

### `Scripts/Puzzle`: 🟡 60%
- `BoardManager.cs`, `RuntimeState.cs`, `DragInputManager.cs`, `ShapeFinder.cs`, `ValidationManager.cs`, `StageData.cs`
- 퍼즐 타일 선택/슬라이딩 매칭, BFS 모양 탐색, 판정 검증 엔진(`IValidator`) 및 15개 퍼즐 보드 ScriptableObject 데이터
> [!TODO]
> 15개 스테이지 SO 작성 및 챕터별 특수 기믹 전략 구현

### `Scripts/Ending`: 🟢 100%
- `EndingQuestController.cs`, `EndingCollectibleItem.cs`, `EndingTodoUIController.cs`
- 챕터 6 Todo 4단계 수집 퀘스트, 단일 Todo 순차 노출 UI, `isEnding` 상태 세이브/로드, EndingEvent 자동 연출 및 크레딧 씬 페이드 전환 제어 완료

### `Scripts/Dialogue`: 🟢 85%
- `DialogueManager.cs`, `DialogueParser.cs`
- CSV/Text 기반 스토리 대사 파싱 및 팝업 연출 모달 바인딩
> [!TODO]
> 컷씬 전용 대사 모드 및 예외 처리 보완

### `Scripts/Tests`: 🟢 90%
- `MapUnlockDebugHelper.cs`, `StageManagerDebugHelper.cs`, `PuzzleTest.cs`, `OnMouseDown_SwitchScene.cs`
- 실시간 런타임/에디터 테스트 헬퍼 및 씬 전환 반응 스크립트
> [!TODO]
> 디버그 단축키 정리 및 효과음 연결

---

## C. 클래스 역할별 접미사 (Naming Rules)

> [!TIP]
> 유니티 클래스는 역할에 맞는 접미사를 사용하고 Manager 하나에 로직이 비대해지지 않도록 **God Object**를 방지합니다.

- **Manager**: 전역 도메인/시스템 총괄 싱글톤 (`UserDataManager`, `GameManager`, `DialogUIManager`)
- **Controller**: 개별 GameObject 행동/입력 제어 (`MapNavigationController`, `StageSelectController`, `SettingModalController`)
- **Director**: 상위 연출 및 흐름 지휘 (`CutsceneDirector`)
- **Service / Parser**: 비-MonoBehaviour 순수 로직/파서 (`DialogueParser`, `SaveService`)
- **Data / Config**: 데이터 직렬화 모델 및 ScriptableObject (`UserData`, `StageData`)
- **Handler / Listener**: 이벤트 반응 헬퍼 (`OnMouseDown_SwitchScene`)

---

## D. 모듈 간 의존성 & 안전 규칙

1. **단방향 참조 준수**: UI 및 Puzzle 모듈은 `Managers` 및 `Data` 레이어를 참조할 수 있지만, 역방향 참조는 이벤트/델리게이트를 통해 연동합니다.
2. **UserData 캡슐화**: 저장 데이터 직접 수정 금지. 반드시 `UserDataManager.Instance` 메서드 (`ClearStage`, `CollectItem`, `ResetProgress`)를 경유합니다.
3. **Puzzle Engine 독립성**: `PuzzleBoardController`는 UI 로직과 분리되어 `IPuzzleRule` 전략 객체를 주입받아 작동합니다.
4. **God Object 방지**: Manager에 비대한 로직이 몰리지 않도록 데이터는 `Data`, 비-Mono 파싱은 `Service`, 개별 UI/오브젝트는 `Controller`로 책임 분할합니다.
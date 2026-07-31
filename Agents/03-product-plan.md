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

| 씬/프리팹 에셋 명 | 주요 기능 및 역할 | 구현 스크립트 연동 | 구현 상태 |
| :--- | :--- | :--- | :---: |
| **MainMenuScene.unity** | 게임 스타트, 최근 진행 위치 맵 이동, 환경설정 팝업 | `UserDataManager`, `SettingModalController` | ✅ 씬 에셋 배치완료 |
| **Map01~05Scene.unity** | 맵 탐색, 1~3번 퍼즐 버튼 해금, 이전/다음 맵 이동 | `MapNavigationController`, `StageSelectController` | ✅ 씬 에셋 배치완료 |
| **StageEntryScene.unity**| 챕터별 3개 스테이지 중 해금된 퍼즐 단계 선택 | `StageSelectController`, `UserDataManager` | ✅ 씬 에셋 배치완료 |
| **StageScene.unity** | 15개 타일 퍼즐 판정, 챕터별 기믹 전략 연동 및 클리어 저장 | `PuzzleBoardController`, `StageManager` | ✅ 씬 에셋 배치완료 |
| **ItemListScene.unity** | 획득한 수집 아이템 리스트 조회 | `UserDataManager.HasCollectedItem()` | ✅ 씬 에셋 배치완료 |
| **CreditsScene.unity** | 게임 엔딩 후 크레딧 연출 출력 | `SceneFlowManager` | ✅ 씬 에셋 배치완료 |
| **PF_DialogModalScene.unity**| 스토리 대사 팝업 모달 독립 연출 테스트 | `DialogUIManager`, `DialogManager` | ✅ 씬 에셋 배치완료 |
| **DialogModal_PF.prefab** | 캐릭터 일러스트/대사 연출 팝업 (Auto/Skip/Log) | `DialogUIManager` | ✅ 프리팹 완성 |
| **LogModal_PF.prefab** | 지나간 대사 기록 조회 모달 | `DialogUIManager.OpenLogModal()` | ✅ 프리팹 완성 |
| **SettingModal_PF.prefab** | 볼륨 조절 & 게임 진행도 초기화 (`PlayerPrefs` 리셋) | `DialogUIManager.OpenSettingModal()`, `UserDataManager` | ✅ 프리팹 완성 |

---

## D. 역할 기반 클래스 네이밍 가이드

- **System Managers**: `UserDataManager` [완료], `DialogUIManager` [완료], `GameManager` [예정], `SceneFlowManager` [예정], `StageManager` [예정]
- **Component Controllers**: `MapNavigationController` [예정], `StageSelectController` [예정], `PuzzleBoardController` [예정], `SettingModalController` [예정]
- **Services & Data Models**: `UserData` [완료], `DialogueParser` [예정], `StageData` [예정]
- **Utils & Helpers**: `OnMouseDown_SwitchScene` [완료], `UserDataTest` [완료]


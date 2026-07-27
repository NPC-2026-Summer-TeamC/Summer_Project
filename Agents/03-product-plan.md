# 3) 게임 기획 및 씬 플레이 루프 기획안

본 문서는 스토리 기반 15개 타일 퍼즐 게임 **"606호에 어서오세요" (Welcome606)**의 핵심 기획, 씬 구조 및 게임 플레이 루프를 정의합니다.

---

## A. 게임 개요

- **콘셉트**: "606호"라는 미스터리 공간을 배경으로 펼쳐지는 스토리 연출 및 타일 퍼즐 게임.
- **볼륨**: 5개 맵 (챕터 1~5) × 맵별 3개 스테이지 = **총 15개 퍼즐 스테이지**.
- **핵심 루프**: 맵 탐색 ➔ 퍼즐 진입 & 클리어 ➔ 스토리 대사 연출 ➔ 수집 아이템 해금 ➔ 다음 맵 이동.

---

## B. 씬 & UI 플레이 흐름도

```txt
[타이틀 화면 (MainMenuScene)]
       │
       ▼
 [맵 화면 (Map01Scene ~ Map05Scene)] ◄─── (퍼즐 3개 클리어 시 좌/우 이동 화살표 활성화)
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

## C. 씬 및 주요 UI 기능 명세

| 구분 | 주요 기능 및 역할 | 연동 시스템 / 클래스 |
| :--- | :--- | :--- |
| **MainMenuScene** | Game Start (저장 데이터 기반 최근 위치 이동), 환경설정 팝업 | `UserDataManager`, `SettingModalController` |
| **Map01~05Scene** | 맵 탐색, 맵 진행도(0/3~3/3) 표시, 맵 이동 화살표 활성화 | `MapNavigationController`, `UserDataManager` |
| **StageEntryScene**| 챕터별 3개 스테이지 중 해금된 퍼즐 단계별 선택 | `UserDataManager.IsStageUnlocked()` |
| **StageScene** | 15개 타일 퍼즐 판정, 챕터별 기믹 전략 연동 및 클리어 저장 | `PuzzleBoardController`, `IPuzzleRule` |
| **DialogModal_PF**| 캐릭터 일러스트/대사 출력 (Auto/Skip/Log 및 속도 설정 탭) | `DialogManager`, `DialogUIManager` |
| **SettingModal_PF**| BGM/SFX 볼륨 조절, **게임 진행도 초기화** (PlayerPrefs 리셋) | `UserDataManager.ResetProgress()` |

---

## D. 역할 기반 클래스 네이밍 가이드

- **System Managers**: `UserDataManager` (데이터 저장/불러오기), `DialogManager` (대사 연출 총괄), `StageManager` (스테이지 관리)
- **Component Controllers**: `PuzzleBoardController` (퍼즐 그리드/타일 제어), `MapNavigationController` (맵 이동 화살표 제어), `SettingModalController` (환경설정 UI 제어)
- **Services & Data Models**: `DialogueParser` (대사 CSV/JSON 파서), `UserData` (진행도 저장 데이터), `StageData` (ScriptableObject 설정)


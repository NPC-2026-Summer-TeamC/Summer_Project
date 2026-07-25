# 1) 폴더 및 모듈 아키텍처 (Unity C#)

본 문서는 **"606호에 어서오세요" (Welcome606)** 프로젝트의 유니티 C# 폴더 아키텍처 및 모듈 책임을 정의합니다.

---

## A. Top-level 폴더 구조

```txt
Summer_Project
├─ Agents                                 # ✅ 에이전트 작업 가이드 및 문서
│  ├─ 01-folder-architecture.md
│  ├─ 02-specs.md
│  ├─ 03-product-plan.md
│  ├─ reports
│  │  └─ _template.md
│  └─ todo
│     ├─ _template.md
│     └─ 00-todo-list.md
│
└─ Welcome606                             # Unity 메인 프로젝트
   └─ Assets
      ├─ Audio                            # BGM, SFX 오디오 리소스
      ├─ Prefabs                          # 재사용 가능한 UI 모달 및 게임 오브젝트 프리팹
      │  ├─ DialogModal_PF
      │  ├─ SettingModal_PF
      │  ├─ LogModal_PF
      │  └─ ItemListModal_PF
      ├─ Scenes                           # 유니티 씬
      │  ├─ MainMenuScene
      │  ├─ Map01Scene ~ Map05Scene
      │  ├─ StageEntryScene
      │  ├─ StageScene (퍼즐 메인)
      │  ├─ ItemListScene
      │  └─ CreditsScene
      ├─ Scripts                          # C# 스크립트 모듈
      │  ├─ Data                          # 유저 데이터 및 데이터 모델
      │  ├─ Dialog                        # 대사 및 스크립트 제어
      │  ├─ Managers                      # 매니저 클래스 (싱글톤)
      │  ├─ Puzzle                        # 타일 퍼즐 코어 및 규칙 패턴
      │  ├─ UI                            # UI 모달 및 컨트롤러
      │  └─ Utils                         # 유틸리티 및 씬 전환 도구
      ├─ Settings                         # URP 및 프로젝트 설정
      └─ Sprites                          # 캐릭터 대사, 배경, 타일 일러스트
```

---

## B. Scripts 내부 모듈 책임 (FSD & Layered Architecture)

```txt
Assets/Scripts
├─ Data                                  # 도메인 모델 및 데이터 정의
│  ├─ UserData.cs                        # 진행도, 해금, 수집품 직렬화 모델
│  └─ StageData.cs                       # 타일 배치 및 스테이지 데이터 (ScriptableObject)
│
├─ Managers                              # 전역 시스템 관리자 (Singleton)
│  ├─ UserDataManager.cs                 # 저장/불러오기, 초기화(Reset), 해금 조건 검증
│  ├─ GameManager.cs                     # 전체 게임 상태 및 씬 전환 컨텍스트 관리
│  └─ SceneFlowManager.cs                # 씬 인덱스/이름 기반 안전한 이동 제어
│
├─ Dialog                                # 스토리 대사 및 스크립트 연출
│  ├─ DialogData.cs                      # 대사 노드/목록 구조
│  └─ DialogManager.cs                   # Auto/Skip/Log/대사 진행 및 UI 바인딩
│
├─ UI                                    # UI 뷰 및 모달 컨트롤러
│  ├─ DialogUIManager.cs                 # 대사 모달 내 로그/설정 탭 팝업 Stack 관리
│  ├─ SettingModalController.cs          # 볼륨 슬라이더 & 게임 진행도 초기화 연동
│  └─ ItemListModalController.cs         # 수집 아이템 팝업 및 상세 표시
│
├─ Puzzle                                # 퍼즐 메인 엔진
│  ├─ PuzzleBoardController.cs           # 타일 그리드 생성, 이동 및 완성 판정
│  ├─ Tile.cs                            # 개별 타일 상호작용 및 이동 애니메이션
│  ├─ IPuzzleRule.cs                     # 챕터별 특수 퍼즐 규칙 인터페이스 (Strategy Pattern)
│  └─ ChapterPuzzleRules/                # 1~5장 개별 퍼즐 특수 규칙 구현체
│
└─ Utils                                 # 헬퍼 및 이벤팅
   └─ OnMouseDown_SwitchScene.cs         # 단순 상호작용 씬 전환 헬퍼
```

---

## C. 모듈 간 의존성 규칙

1. **상위 레이어 단방향 참조**: UI 및 Puzzle 모듈은 `Managers` 및 `Data` 레이어를 참조할 수 있지만, 역방향 직결 참조는 이벤트/델리게이트를 통합니다.
2. **UserData 캡슐화**: 저장 데이터 직접 수정 금지. 반드시 `UserDataManager.Instance` 메서드(`ClearStage`, `CollectItem`, `ResetProgress`)를 통해 변경합니다.
3. **Puzzle Engine 독립성**: `PuzzleBoardController`는 UI 모달 로직과 분리되어 `IPuzzleRule` 전략을 주입받아 작동합니다.
# 기여 가이드 (Contributing Guide)

"606호에 어서오세요" 프로젝트에 기여해 주셔서 감사합니다.
원활한 협업과 코드 품질 유지를 위해 아래의 개발 워크플로우와 컨벤션을 준수해 주세요.

---

## 1. 브랜치 전략 (Branch Strategy)

```
main                  → 항상 실행 가능한 안정 버전 (배포용)
develop               → 기본 통합 개발 브랜치
feature/이름-기능명   → 기능 개발용 브랜치
fix/이름-내용         → 버그 수정용 브랜치
docs/이름-내용        → 문서 작업용 브랜치
```

- `main` 및 `develop` 브랜치에는 직접 push할 수 없으며, **PR(Pull Request)을 통해서만 병합**됩니다.
- 새 작업 시작 단계:
  1. `develop` 브랜치에서 최신 상태로 **Fetch → Pull** 진행
  2. `develop`을 기준으로 새 작업 브랜치 생성 (예: `feature/jimin-tile-drag`)
  3. 로컬 작업 및 기능 구현
  4. 로컬 커밋 후 원격 저장소로 **Push / Publish**
  5. GitHub에서 PR 생성 후 리뷰 요청 (최종 머지는 팀장이 수행)

---

## 2. 커밋 컨벤션 (Commit Convention)

커밋 메시지 앞에는 지정된 이모지와 태그를 붙이며, 메시지 끝에는 관련 **(#이슈번호)** 를 반드시 포함합니다. (작업자 이름은 메시지에 포함하지 않음)
작업 단위는 가능한 한 작게 나누어 커밋하는 것을 권장합니다.

> **형식**: `<이모지><태그>: <설명> (#이슈번호)`

| 이모지 | 태그 | 설명 | 커밋 메시지 예시 |
|:---:|:---|:---|:---|
| ✨ | `FEAT` | 새로운 기능 추가 | `✨FEAT: 타일 드래그 기능 구현 (#12)` |
| 🎨 | `STYLE` | UI/비주얼 조정 (기능 변화 없음) | `🎨STYLE: 메인 메뉴 버튼 색상 변경 (#15)` |
| 🛠 | `FIX` | 코드 오류 수정, 오타 정정, 로직 버그 해결 | `🛠FIX: 타일 드래그 시 씬 이탈 버그 수정 (#18)` |
| 🔧 | `REFACTOR` | 기능 변화 없는 코드 구조 개선 | `🔧REFACTOR: 데이터 검증 로직 모듈화 (#22)` |
| 📂 | `DOCS` | README / 문서 작성 및 수정 | `📂DOCS: 커밋 컨벤션 안내 가이드 수정 (#5)` |
| 📦 | `CHORE` | 파일 이동, 폴더 구조 변경 등 단순 관리 작업 | `📦CHORE: 사운드 리소스 폴더 위치 이동 (#30)` |

> 💡 **주의 사항**
> - 이슈 번호 앞에는 반드시 `#` 기호를 붙여야 GitHub 이슈와 자동 연결됩니다.
> - 연관 이슈가 없는 단순 작업일 경우 `(#이슈번호)`는 생략할 수 있습니다.

---

## 3. Pull Request (PR) 규칙

- 모든 작업은 `develop` 브랜치를 타겟으로 PR을 생성합니다.
- PR 본문에 이슈 해결 상태를 명시합니다:
  - 이슈 완료 시: `Close #이슈번호` (머지 시 해당 이슈 자동 종료)
  - 연관/진행 중 표시: `Related to #이슈번호` (자동 종료되지 않음)
- **최소 1인 이상의 리뷰 승인(Approve)** 이 있어야 머지할 수 있습니다.
- 본인이 작성한 PR은 직접 머지하지 않고 팀원에게 리뷰를 요청합니다.

---

## 4. 이슈 및 프로젝트 관리

- **이슈 등록**: 버그 발견 또는 신규 구현 항목이 생기면 GitHub **Issues**에 등록합니다.
  - 라벨(`버그` / `기능`) 및 담당자(Assignee)를 지정합니다.
- **칸반보드 (GitHub Projects)**:
  - 작업 시작 시 이슈의 상태를 `In Progress`로 변경합니다.
  - PR과 이슈를 연결하여 작업 흐름을 투명하게 관리합니다.

---

## 5. 코딩 컨벤션 (Coding Convention)

### 5.1 네이밍 규칙
| 대상 | 규칙 | 예시 |
|---|---|---|
| 스크립트(클래스)/함수 | `PascalCase` | `TileController`, `RuleValidator`, `CheckShapeValid()` |
| 변수 | `camelCase` | `currentColor`, `isDragging` |
| 프리팹 | `기능명_PF` | `Tile_PF`, `NPC_Dialogue_PF` |
| 씬 파일 | `기능명Scene` | `MainMenuScene`, `Stage01Scene` |

### 5.2 주석 규칙
- 함수의 "목적"과 "왜/어떻게"를 설명하는 의미 있는 주석을 작성합니다.
- 코드 자체로 자명한 단순 변수 등에는 불필요한 주석을 지양합니다.

```csharp
// ✅ 좋은 예: 함수의 목적과 예외 처리 배경 설명
// 플레이어가 타일을 드래그할 때 호출됨. 이동 가능 여부는 RuleValidator에서 별도 검증
public void OnTileDrag(Vector2 pos) { ... }

// ❌ 지양할 예: 자명한 변수 선언 주석
int count = 0; // count 변수
```

### 5.3 중괄호 스타일
- 함수는 여는 중괄호(`{`)를 다음 줄로 내립니다 (Allman 스타일).
- `if`/`while` 등 제어문은 같은 줄에 붙여서 작성합니다 (K&R/OTBS 스타일).

```csharp
// ✅ 함수: 줄바꿈 후 {
public void OnTileDrag(Vector2 pos)
{
    ...
}

// ✅ 제어문: 같은 줄에 {
if (isValid) {
    ...
}

while (hasNext) {
    ...
}
```

---

## 6. 폴더 및 에셋 구조 컨벤션

`Assets` 폴더 하위는 다음 분류 기준을 준수하며, 새 폴더 생성 시 팀원과 상의합니다.

| 폴더 | 용도 | 규칙 |
|---|---|---|
| `Assets/Scripts` | C# 스크립트 | 역할별 하위 분류 (`Data`, `Managers`, `Controllers` 등) |
| `Assets/Prefabs` | 프리팹 에셋 | 기능별 하위 분류 및 `기능명_PF` 네이밍 준수 |
| `Assets/Scenes` | 씬 파일 | 정식 씬은 최상위, 테스트 씬은 `Scenes/Tests`에 위치 |
| `Assets/Sprites` | 이미지 및 2D 텍스처 | 용도별 분류 (`UI`, `Tile`, `NPC`, `Item` 등) |
| `Assets/Audio` | 오디오 파일 | `BGM`, `SFX` 등 용도별 분류 |
| `Assets/Settings` | URP 및 프로젝트 설정 | Unity 자동 관리 폴더 (임의 구조 변경 금지) |
| `Assets/TextMesh Pro` | TMP 리소스 | Unity 자동 관리 폴더 (임의 구조 변경 금지) |

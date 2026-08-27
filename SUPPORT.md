# 지원 및 개발 환경 가이드 (Support & Setup)

"606호에 어서오세요" 프로젝트의 개발 환경 설정 및 문제 해결 가이드입니다.

---

## 1. 개발 환경 및 요구 사양

- **Unity 엔진 버전**: `Unity 6000.3.11f1` (Unity 6)
- **렌더 파이프라인**: URP (Universal Render Pipeline)
- **텍스트 시스템**: TextMeshPro (TMP)

---

## 2. Git LFS (Large File Storage) 설정

이미지, 오디오, 폰트, 3D 모델 등 대용량 바이너리 파일 관리를 위해 Git LFS를 필수로 사용합니다.

### 설치 및 초기화
1. [Git LFS 공식 사이트](https://git-lfs.com/)에서 설치 프로그램을 다운로드하여 설치합니다.
2. 프로젝트 루트 폴더에서 터미널을 열고 다음 명령어를 실행합니다 (최초 1회):
   ```bash
   git lfs install
   ```
3. 저장소의 `.gitattributes`에 대상 확장자가 등록되어 있으므로 추가 설정 없이 일반적인 `git add`, `git commit`, `git push`로 자동 처리됩니다.

---

## 3. 자주 묻는 질문 (FAQ) & 트러블슈팅

### Q1. 씬이나 프리팹에서 Missing 에셋이 발생해요.
- Git LFS가 정상 설치/동기화되지 않았을 가능성이 높습니다.
- 터미널에서 `git lfs pull`을 실행하여 대용량 바이너리 에셋을 다시 받아오세요.

### Q2. Unity 버전 불일치 경고가 뜹니다.
- 프로젝트는 `Unity 6000.3.11f1` 버전을 기준으로 개발됩니다. Unity Hub에서 동일한 버전을 설치하여 열어주세요.

---

## 4. 도움 및 지원 요청 채널

- **버그 제보 및 기능 제안**: GitHub 저장소의 [Issues](../../issues) 탭을 통해 등록해 주세요.
- **협업 및 기술 문의**: 팀 슬랙/디스코드 또는 GitHub Discussions/Issues를 활용해 주세요.

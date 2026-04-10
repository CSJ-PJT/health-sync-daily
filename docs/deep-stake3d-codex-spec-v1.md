# DeepStake3D Codex 구현 명세서 v1

`DeepStake3D` is the active project. Continue from this handoff note only.

## 0. 프로젝트 범위

### 활성 프로젝트

- `unity/DeepStake3D`

### 구현 금지 경로

- `unity/DeepStakeUnity`
- 레거시/참고용만
- 구현 계속하지 말 것

## 1. 현재 확인된 상태

다음은 이미 확인된 현재 상태다.

- `Boot -> MainMenu -> WorldPrototype3D` 흐름 정상
- `Start Local Play` 정상
- `WorldPrototype3D` 3D 쿼터뷰 씬 진입 정상
- 로컬 저장 JSON 생성 정상
- 메인 메뉴 클릭 이슈 해결됨
- 현재 프로젝트 방향은 3D 쿼터뷰 / 라이프심 / 회복 / 정착 / 구조적 압박 / 차원 확장이다

### 확인된 주요 파일/시스템

- `Assets/Scripts/World/WorldPrototype3DController.cs`
- `Assets/Scripts/Player/PlayerMover3D.cs`
- `Assets/Scripts/Save/LocalSaveService.cs`
- `Assets/Scripts/UI/HudStatusView.cs`
- `Assets/Data/world-prototype-3d.json`

## 2. 작품 방향 고정 규칙

Deep Stake는 가상 현대 세계를 배경으로 한 3D 쿼터뷰 라이프심 RPG다.

### 고정 방향

- 초반: 흙, 물, 장부, 창고, 정착, 생활 복구
- 중반: 공급망, 기록 조작, 세력 압박, 공명 이상
- 후반: 3D / 4D / 5D 차원 전이, 고차원 건설과 문명 운영

### 금지/유지 규칙

- 게임 내 raw health 수치 직접 표시 금지
- 반드시 derived health-link values만 사용
- 현실의 특정 민족, 종교, 실제 공인/정치인을 직접 대응시키지 말 것
- 적대 세력은 완전히 허구적인 구조로 유지할 것
- 지금 단계에서는 멀티플레이 우선 구현 금지
- 지금 단계에서는 2D-first 방향으로 복귀 금지

## 3. 세계관/서사 베이스라인

### 첫 지역

- 시작 지역: `Longest Dawn`

역할:

- 프롤로그 지역
- 회복의 첫 시도
- 기록 훼손과 토지 말소의 첫 단서
- 첫 비콘 설치 지역

### 적대 세력

- 작업명: `Continuum Directorate`

성격:

- 초국가 자본 권력 네트워크
- 투자기구, 물류재단, 개발조합, 재건펀드, 데이터 신탁, 기록기관 등으로 분산 존재

기능:

- 농지 담보화 및 회수
- 공급망 지연/차단
- 부채 계약 종속
- 기록 조작
- 구호를 통한 종속 구조 형성
- 지역 자립성 붕괴 유도
- 공명 저하 유지

### 이야기 구조

- 초반: 지역 복구, 기록 수집, 공급 복원, 정착 시작
- 중반: 지역 간 반복 패턴 발견, 압박 구조 파악, 4차원 잔향 인식
- 후반: 5차원 진입, 전투 없는 고차원 건설 중심 운영

## 4. 장기 비전은 넓게, 이번 구현 범위는 좁게

장기적으로 Deep Stake는 매우 넓은 세계를 가진다.

### 장기 구조

- 광범위한 가상 현대 세계
- 대도시, 농촌, 항만, 산업 벨트, 외곽 정착지, 정부/행정 구역 존재
- 마을/도시는 성향과 압박값을 가진다
- 플레이어는 빛 정렬 / 어둠 정렬 / 통제 / 회복 / 중재 방향으로 갈 수 있다
- 어느 차원이든 건설 가능해야 한다
- 특히 5차원은 거의 전투가 없는 메인 건설/운영 단계처럼 작동해야 한다

그러나 이번 패스의 실제 구현 범위:

이번 패스는 전체 세계를 만들지 말고, `Longest Dawn` 첫 버티컬 슬라이스를 완성하되, 이후 광역 세계/정렬/차원 건설 구조로 확장 가능한 기반만 설계할 것.

## 5. 이번 패스의 핵심 목표

현재 기술 프로토타입을 첫 실제 플레이 가능한 버티컬 슬라이스로 끌어올려라.

### 반드시 완성해야 할 플레이 루프

- Boot
- MainMenu
- Start Local Play
- WorldPrototype3D 진입
- 이동
- Farm Sign 상호작용
- Archivist 또는 첫 NPC와 대화
- 첫 settlement beacon 설치
- 저장
- 종료/재실행/Continue Latest Save
- 저장 상태 복원 확인

## 6. 이번 패스 구현 항목

### 6-1. 첫 플레이 루프 개선

다음 루프를 실제 플레이 가능하게 완성할 것.

- 플레이어 이동
- 표지판 상호작용
- NPC 대화
- 비콘 설치
- 저장
- 불러오기
- 상태 유지

#### 최소 저장 대상

- 플레이어 위치
- 첫 표지판 상호작용 상태
- 첫 NPC 대화 진행 상태
- beacon 설치 여부 및 위치
- 첫 지역 퀘스트/플래그 상태
- 기본 압박 힌트 상태

### 6-2. Longest Dawn 첫 지역 재구성

`WorldPrototype3D`를 테스트 씬이 아니라, 읽히는 시작 지역으로 재배치할 것.

#### 첫 지역 필수 구역

- recovery field / farm edge
- archive observer or Archivist area
- Farm Sign landmark
- beacon placement zone
- storage / supply hint zone
- pathing that naturally teaches the loop

#### 요구사항

- 프리미티브 메쉬 사용 가능
- 배치만 봐도 공간의 목적이 드러나야 함
- 시야/동선/거리감 정리
- 버려졌지만 다시 살릴 수 있는 곳처럼 보여야 함

### 6-3. 첫 NPC 및 상호작용 구현

이번 패스에서 최소한 다음 서사 기능이 들어가야 한다.

#### Farm Sign

플레이어의 첫 상호작용 오브젝트.
이 땅이 단순 폐허가 아니라 첫 회복 주기를 기다리던 장소라는 메시지를 줘야 한다.

#### Archivist

초반의 첫 지식 제공자.

역할:

- 기록 훼손
- 토지 말소
- 같은 양식/인장의 반복
- 붕괴가 우연이 아님을 암시

대화 원칙:

- 처음부터 모든 비밀을 설명하지 말 것
- 플레이어가 직접 단서를 연결하도록 만들 것
- 생활적이고 건조한 톤 유지
- 지나치게 초반부터 우주적 설명 금지

### 6-4. Continuum Directorate의 첫 통합

이번 패스에서는 세력을 전면적으로 구현하지 말고, 첫 지역에서 체감되는 구조적 압박으로 나타낼 것.

#### 표현 방식

- 훼손된 장부
- 토지/압류 관련 통지문
- 지연된 공급 흔적
- 같은 인장/문양/서식의 반복
- 기록 일부 누락
- HUD 또는 상태 텍스트의 압박 힌트

#### 금지 사항

- 현실의 실제 정부, 실제 민족, 실제 종교, 실제 공인 직접 매핑 금지
- 노골적 선전 문구처럼 보이는 표현 금지

### 6-5. 임시 UI 개선

현재 임시 UI를 다음 수준으로 올릴 것.

#### 필요 요소

- clearer MainMenu
- Start Local Play / Continue Latest Save 가 명확할 것
- 현재 조작 힌트 표시
- 상호작용 프롬프트 표시
- 저장 성공 표시
- 로드 성공 표시
- 압박/세력 힌트 1개 이상 표시

#### 원칙

- 최종 아트 불필요
- 읽기 쉬운 임시 UI면 충분
- 모바일에서도 터치 가능한 크기 고려

### 6-6. 모바일 입력 레이어 추가

현재 코드는 키보드 중심이다.
Android smoke test용 임시 모바일 입력 경로를 추가할 것.

#### 필수 모바일 입력

- movement
- interact
- talk
- place
- save/test action

#### 구현 원칙

- 간단한 임시 온스크린 UI로 충분
- 키보드 지원은 유지
- 에디터와 Android 둘 다 망가지지 않게
- Input System Package 기준으로 동작할 것

### 6-7. Save / Load 루프 강화

`Continue Latest Save`가 실제로 동작해야 한다.

#### 요구사항

- 단순히 씬만 로드하지 말 것
- 저장된 상태를 복원할 것
- 앱 재실행 후에도 상태 유지 확인 가능해야 함
- 로컬 JSON 저장 구조가 읽기 쉬워야 함

#### 예상 저장 파일

- `deepstake-slot-01.json`

### 6-8. 데이터 드리븐 구조 강화

`Assets/Data/world-prototype-3d.json`을 확장해서, 첫 구역의 요소가 데이터 기반으로 유지되도록 할 것.

#### 데이터화 대상

가능한 범위에서 다음을 데이터 기반으로 유지:

- NPC 배치
- interactable placement
- sign text / ids
- beacon placement zone
- first zone hint text
- basic pressure flags
- first quest state hooks

#### 목표

지금은 작은 구조여도, 이후 지역/도시/정렬 시스템으로 확장 가능해야 한다.

## 7. 미래 확장을 고려한 설계 기반

이번 패스에서 전부 구현하지는 않지만, 다음 확장을 막지 않는 구조로 설계할 것.

### 7-1. 광역 세계 확장 가능성

장기적으로 3차원 세계에는 다음 지역 유형이 존재한다.

- Longest Dawn
- 초고밀도 수도권 메가시티 중심부
- 항만 물류권
- 산업 벨트
- 외곽 회복 지대
- 북부 개척지
- 고압박 특구

이번 패스에서는 Longest Dawn만 구현하되, 추후 지역 단위 확장을 고려한 데이터 구조와 시스템 네이밍을 사용할 것.

### 7-2. 정렬/압박 시스템 확장 가능성

장기적으로 도시/마을/구역은 다음 값을 가질 수 있다.

- recovery
- pressure
- light alignment
- dark alignment
- persuasion potential
- memory stability

이번 패스에서는 이 전체 시스템을 만들지 않아도 되지만, 최소한 첫 지역에 pressure hint 또는 향후 확장 가능한 상태값 훅을 둘 것.

### 7-3. 전 차원 건설 가능 원칙

장기 원칙:

- 3차원: 물질 기반 건설
- 4차원: 기억/공명 기반 건설
- 5차원: 전투 없는 고차원 문명 건설

이번 패스에서는 3차원 첫 beacon만 구현하되, 이것이 장기적으로 모든 차원에서 가능한 건설 시스템의 첫 형태가 되도록 네이밍과 구조를 잡을 것.

## 8. 5차원 관련 고정 방향

이 부분은 이번 패스에서 전면 구현하지 말 것.
하지만 향후 방향은 고정한다.

### 고정 원칙

- 5차원은 단순 엔딩 공간이 아니다
- 5차원은 사실상 전투 없는 메인 운영/건설 단계다
- 5차원에서는 건설, 연결, 조율, 문명 운영이 핵심이다
- 5차원은 다중 행성 구조가 가능하다
- 5차원 존재는 3차원과 직접 교류하지 못한다
- 하지만 4차원을 통해 에너지/영감/공명 지원은 가능하다

### 이번 패스에서 필요한 것

- 이 방향을 깨지 않는 방식으로 첫 beacon과 first-zone lore를 설계할 것
- 너무 이른 시점에 UFO/외계 행성/5차원 직접 연출을 넣지 말 것
- 대신 향후 4D/5D 확장과 충돌하지 않는 정도의 암시만 허용

## 9. 안드로이드 스모크 테스트 준비

이번 패스 결과물은 Android에서 최소 스모크 테스트가 가능해야 한다.

### 확인할 설정

- Active Input Handling:
  - Input System Package (New)
- Product Name:
  - Deep Stake
- Package Identifier:
  - com.roboheart.deepstake
- Scene Order
  - Boot
  - MainMenu
  - WorldPrototype3D

### 목표

- Android-safe scene flow
- mobile UI path available
- no editor-only blockers in critical loop
- local save/load path does not obviously break the smoke test

## 10. 이번 패스에서 기대하는 결과물

### 이번 패스 완료 시 다음이 가능해야 한다

#### 플레이 가능 상태

- 메뉴에서 새 게임 시작 가능
- 월드 진입 가능
- 이동 가능
- 표지판 조사 가능
- NPC 대화 가능
- 비콘 설치 가능
- 저장 가능
- Continue Latest Save 가능
- 재실행 후 상태 복원 가능

#### 표현 가능 상태

- Longest Dawn이 단순 테스트 맵이 아니라 실제 첫 지역처럼 보일 것
- Continuum Directorate의 구조적 흔적이 약하게라도 느껴질 것
- 회복 / 기록 / 압박 / 정착의 테마가 첫 10분 안에 보일 것

## 11. 이번 패스에서 구현하지 말 것

다음은 이번 패스에서 무리하게 만들지 말 것.

- 전체 광역 도시맵
- 복수 지역 완전 구현
- 정치/외교 풀 시스템
- 마을 회유 풀 시스템
- 완전한 정렬 수치 시스템
- 4차원 월드 플레이어블 구현
- 5차원 월드 플레이어블 구현
- 멀티플레이
- 최종 아트
- 복잡한 전투 시스템

이번 패스는 첫 버티컬 슬라이스 완성 + 미래 확장 기반 설계가 목표다.

## 12. 구현 스타일 가이드

### 코드

- 읽기 쉽게
- 과도한 프레임워크 도입 금지
- 기존 파일/시스템 존중
- 작은 확장 단위로 추가
- 하드코딩을 최소화하고 필요한 곳은 JSON/data driven 처리

### 씬/오브젝트

- 프리미티브 메쉬 허용
- 구조와 목적 전달이 우선
- 테스트 가능성이 미관보다 우선

### UI

- 단순/임시여도 좋지만 명확해야 함
- 모바일에서 눌릴 크기 고려
- 상태 텍스트 피드백 중요

## 13. Codex 작업 결과 출력 형식

작업을 끝낼 때 반드시 아래 형식으로 정리할 것.

### REQUIRED OUTPUT

- COMPLETED
- IN PROGRESS
- FILES CHANGED
- FIRST PLAYABLE LOOP
- LONGEST DAWN ZONE CHANGES
- NPC / SIGN INTERACTION
- FACTION INTEGRATION
- MOBILE INPUT STATUS
- SAVE / LOAD STATUS
- DATA-DRIVEN CHANGES
- MANUAL UNITY EDITOR STEPS FOR ANDROID BUILD
- NEXT CODEX PROMPT

## 14. 추천 커밋 메시지

- `DeepStake3D first vertical slice loop, beacon placement, and save-load foundation`
- 또는
- `DeepStake3D Longest Dawn first playable slice with mobile input and persistence`

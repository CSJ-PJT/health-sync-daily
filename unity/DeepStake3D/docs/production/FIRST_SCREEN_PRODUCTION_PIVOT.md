# First Screen Production Pivot

## 1. Problem Summary

- `WorldPrototypeVisualPass.cs` 중심의 code-generated primitive visual pass는 속도가 너무 느리다.
- 작은 폴리시 루프를 여러 번 돌렸지만, 스크린샷 기준으로 강한 시각적 점프가 잘 나오지 않았다.
- 로컬 스크린샷 자동화는 이제 충분히 쓸 수 있는 수준까지 왔다.
- 현재 병목은 검증 도구가 아니라 아트 생산 방식이다.
- 즉, procedural primitive patch로 첫 화면을 "완성도 있는 장소"처럼 보이게 만드는 접근이 비용 대비 성과가 낮다.

## 2. New Production Direction

- 첫 화면을 procedural primitive patch만으로 아름답게 만들려는 시도를 중단한다.
- 앞으로는 asset-based 또는 manual level-art 기반으로 first screen을 만든다.
- Codex의 역할은 다음에 집중한다:
  - 씬 구조 정리
  - 시각 전용 루트/폴더 구조 제안
  - anchor 연동 유지
  - 스크린샷 검증 자동화
  - 배치/프리팹 연결 보조
- Codex가 primitive만으로 아트 디렉션을 대신하는 방식은 중단한다.

## 3. Target First Screen

목표 화면:

- DeepStake recovery settlement / rebuilt field outpost
- player start가 첫눈에 보여야 한다
- notice board가 첫 objective landmark로 읽혀야 한다
- buildings가 objective를 묻지 않고 프레이밍해야 한다
- background가 test-map edge/grid를 조용히 가려야 한다
- ground는 calm하고 intentional해야 하며 주인공이 되면 안 된다

요약 문장:

- "무너진 뒤 다시 세운 회복 정착지 입구에서, 플레이어-길-공지판이 즉시 읽히는 첫 화면"

## 4. Asset / Module Needs

필요한 모듈:

- ground/path tiles or meshes
- settlement building modules
- roof / wall / base variants
- notice board / signboard
- fence / low wall / boundary blockers
- crates / barrels / work-yard props
- trees / shrubs / background blockers
- lighting / camera preset

권장 특성:

- 저주파, 읽기 쉬운 실루엣
- quarter-view camera에서 덩어리로 읽히는 재질 분리
- 지나치게 사실적이지 않은 stylized low-poly 또는 stylized semi-low-poly
- 반복 배치해도 덜 티 나는 바닥/경계 모듈

## 5. Implementation Options

### A. Manual Unity Editor Scene Dressing

- Unity Editor에서 직접 first screen만 visual dressing
- 장점:
  - 가장 직접적이고 결과 예측 가능
  - 스크린샷 기준 빠른 개선 가능
- 단점:
  - 사용자 또는 아티스트의 수동 배치 시간이 필요

### B. Imported Stylized Low-Poly Asset Pack

- 외부 stylized low-poly pack을 가져와서 first screen 구성
- 장점:
  - primitive보다 훨씬 빠르게 "게임 같은 화면" 가능
  - roof / wall / base / prop 분리도 즉시 확보 가능
- 단점:
  - 스타일 선정이 중요
  - 무료/유료 여부 결정 필요

### C. Hybrid: Keep Gameplay Anchors, Replace Only Visuals

- 현재 gameplay anchor와 controller는 유지
- visuals만 별도 root 아래 교체
- 장점:
  - 가장 현실적
  - gameplay risk가 낮음
  - Codex와 수동 레벨아트가 같이 일하기 쉬움
- 단점:
  - 시각 루트와 gameplay 루트 분리가 필요

## 6. Recommended Fastest Path

추천 경로:

- gameplay anchor와 current controller는 유지한다
- first screen용 별도 visual dressing root를 만든다
- 기존 anchor 주변에 visual-only modules를 수동 또는 prefab 배치한다
- existing EditorRender screenshot loop로 바로 비교한다
- broad carpet ground generation은 다시 하지 않는다

권장 구조 예시:

- `WorldPrototype3DController`
- `FirstScreenVisualDressing`
  - `GroundModules`
  - `NoticeBoardSetpiece`
  - `BuildingFronts`
  - `BackgroundBlockers`
  - `WorkYardProps`

핵심 원칙:

- gameplay object는 gameplay대로 둔다
- visuals는 visual-only root 아래 둔다
- collision/interaction이 필요 없는 것은 visual-only로 유지한다

## 7. 1-Day Execution Plan

### Hour 1

- 사용할 asset pack 또는 placeholder module 세트를 고른다
- 폴더 구조와 visual-only root를 정한다

### Hour 2

- first screen composition을 수동으로 block한다
- player start, route, notice board, buildings, background blockers를 먼저 잡는다

### Hour 3

- player / notice / camera 관계를 맞춘다
- 건물 프레이밍과 배경 edge blocking을 조정한다

### Hour 4

- existing EditorRender screenshot loop로 baseline과 비교한다
- 한눈에 좋아졌는지 판단한다

### Hour 5

- visually obvious improvement가 있으면 정리 후 commit
- 아니면 과감히 중단하고 asset/style를 다시 선택한다

## 8. Decisions Needed From User

- free asset pack을 쓸지, paid asset pack도 허용할지
- manual Unity Editor placement를 허용할지
- current low-poly style을 유지할지, 더 강한 asset style로 갈지
- DeepStake3D만 현재 active Unity project로 볼지

## Recommended Next Step

- 먼저 asset sourcing 방향을 결정한다:
  - free only
  - paid allowed
  - existing internal modules only
- 그 다음 `FirstScreenVisualDressing` 루트 기준으로 visual-only 배치 계획을 잡는다.
- 이후부터는 `WorldPrototypeVisualPass.cs`로 first screen을 예쁘게 만드는 시도 대신, 실제 모듈 배치와 screenshot 비교로 간다.

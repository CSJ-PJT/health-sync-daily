# First Screen Asset Implementation Plan

## 1. Visual-Only Scene Structure

권장 루트 오브젝트:

- `FirstScreenVisualDressing`

권장 자식 그룹:

- `Ground`
- `NoticeLandmark`
- `BuildingFronts`
- `BoundaryBlockers`
- `Props`
- `LightingCameraHelpers`

의도:

- gameplay 오브젝트와 visual dressing을 명확히 분리한다.
- first screen용 비주얼을 한 루트 아래에서 교체/정리할 수 있게 만든다.
- 추후 visual dressing을 제거하거나 교체해도 gameplay 구조가 깨지지 않게 한다.

세부 역할:

- `Ground`
  - visual-only ground/path modules
  - player pocket, route, notice board 주변의 calm surface
- `NoticeLandmark`
  - notice board backdrop, frame, public board zone, local landmark dressing
- `BuildingFronts`
  - first screen에서 실제로 보이는 건물 전면부와 facade-only masses
- `BoundaryBlockers`
  - top/right edge를 가리는 fence, wall, shrub line, low berm, work-yard edge
- `Props`
  - crates, barrels, work props, storage piles
- `LightingCameraHelpers`
  - camera helper markers, lighting anchors, optional reflection/light reference helpers

## 2. Gameplay Preservation Rule

반드시 유지:

- current gameplay anchors
- player spawn
- notice board interaction
- NPC positions and interactions
- placement zones
- save/load
- controls
- current controller flow

원칙:

- visual dressing은 gameplay 위에 얹는 교체 가능한 층이다.
- visual-only 오브젝트는 gameplay 로직을 직접 참조하지 않는다.
- gameplay와 visual hierarchy를 섞지 않는다.
- visual dressing은 제거해도 현재 플레이 흐름이 유지되어야 한다.

## 3. Asset Requirements

필수 카테고리:

- ground/path modules
- building facade modules
- roof / wall / base variants
- notice board module
- fence / low wall modules
- crates / barrels / storage props
- tree / shrub / background blockers
- lighting / camera preset

스크린샷 기준으로 필요한 성질:

- quarter-view camera에서 읽히는 큰 덩어리
- roof / wall / base 구분이 한눈에 보이는 silhouette
- 지나치게 세밀한 장식보다 큰 mass block 위주
- 바닥은 조용하고 반복성이 덜 드러나는 재질/모듈
- background blockers는 낮고 길게, edge를 가리는 역할 중심

## 4. Fastest Implementation Path

가장 빠른 경로:

- imported stylized low-poly assets를 우선 사용한다
- 이미 적절한 asset set이 있다면 그것으로 first screen만 우선 드레싱한다
- 없으면 temporary visual-only prefabs를 만들 수는 있지만, primitive polishing처럼 시간을 쓰지 않는다

우선순위:

1. first screenshot composition
2. player -> route -> notice board readability
3. building framing
4. top/right background edge hiding
5. full map completeness는 나중

피해야 할 것:

- procedural primitive patch 재시도
- broad terrain carpets
- tiny facade spam
- first screen 바깥까지 한 번에 완성하려는 시도

## 5. One-Day Execution Plan

### Step 1. Import or Prepare Visual Modules

- 사용할 asset pack 또는 internal placeholder module을 정한다
- 필요한 모듈만 추려서 first screen 기준으로 준비한다

### Step 2. Create `FirstScreenVisualDressing`

- scene에 visual-only root를 만든다
- 위 자식 그룹 구조를 먼저 구성한다

### Step 3. Dress Only the First Camera View

- player pocket
- route
- notice landmark
- two or three visible building fronts
- top/right boundary blockers

이 다섯 가지만 먼저 잡는다.

### Step 4. Capture Screenshot

- existing EditorRender / auto loop로 screenshot 비교
- baseline과 비교해 즉시 improvement가 보이는지 확인한다

### Step 5. Commit Only If the Screenshot Jumps

- screenshot이 명확히 나아졌을 때만 commit
- 약하면 더 만지지 말고 asset/style 선택을 다시 판단한다

## 6. Stop Conditions

다음 상황이면 중단:

- suitable asset set이 없다
- imported asset style이 DeepStake target과 너무 어긋난다
- screenshot validation이 다시 막힌다
- visual jump 없이 시간만 쓰기 시작한다

명시적 중단 규칙:

- asset set이 없으면 사용자에게 선택/가져오기를 요청한다
- procedural ground patch loop로 돌아가지 않는다
- broad terrain carpet를 다시 만들지 않는다
- screenshot validation 실패 상태에서 blind visual work를 계속하지 않는다

## Recommended Asset Categories

- stylized low-poly ground/path set
- modular shack / recovery outpost facade set
- roof / wall / base material variants
- signboard / notice board kit
- fence / barricade / yard wall kit
- storage/work props set
- low tree / shrub / scrub silhouettes
- simple lighting preset or lookup setup for quarter-view readability

## Exact Next Implementation Step

- 먼저 사용할 asset source를 하나 확정한다.
- 그 다음 Unity Editor에서 `FirstScreenVisualDressing` 루트를 만들고,
- 첫 화면 카메라에 보이는 영역만 수동 배치로 막는다.

## Asset Dependency Decision

- 네, 계속하려면 사용자 측 asset 선택 또는 import 허용이 사실상 필요하다.
- existing internal modules만으로도 시작은 가능하지만, 지금까지의 결과를 보면 강한 시각적 점프를 내기 어렵다.
- 따라서 다음 실제 구현 전에는 다음 중 하나를 결정해야 한다:
  - free asset pack 사용
  - paid asset pack 허용
  - internal placeholder modules로 first screen만 대체

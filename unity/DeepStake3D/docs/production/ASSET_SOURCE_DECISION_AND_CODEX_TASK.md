# Asset Source Decision And Codex Task

## 1. Decision

- use asset/manual-level-art production path
- free assets first
- manual Unity Editor placement allowed
- stylized low-poly allowed
- current primitive visuals are gameplay anchor/reference only

요약:

- first screen은 더 이상 `WorldPrototypeVisualPass.cs` primitive polish로 해결하지 않는다.
- gameplay anchor는 유지하고, visual dressing은 asset/manual-level-art 경로로 전환한다.

## 2. Do Not Repeat

다시 하지 않을 것:

- no more `WorldPrototypeVisualPass.cs` primitive polish loop
- no broad generated ground carpets
- no tiny facade details
- no tint-only passes
- no unvalidated visual diffs

실무 원칙:

- screenshot에서 바로 안 보이는 미세 수정은 중단한다
- primitive를 계속 덧대는 방식으로 장소성을 만들려 하지 않는다
- validation 없는 시각 변경은 commit 후보로 보지 않는다

## 3. Required Asset Categories

- ground / path
- building fronts
- notice board / signboard
- fence / low wall / boundary blockers
- crates / barrels / work props
- trees / shrubs / background silhouettes

추가 권장:

- roof / wall / base variants
- simple lighting / readability helpers

## 4. Folder Policy

imported assets 위치:

- `unity/DeepStake3D/Assets/ThirdParty/<AssetPackName>/`

DeepStake wrapper / prefab / dressing 위치:

- `unity/DeepStake3D/Assets/DeepStake/VisualDressing/FirstScreen/`

원칙:

- third-party source는 원본 유지
- DeepStake용 조합 prefab/wrapper는 별도 경로에 둔다
- gameplay와 visual dressing은 폴더와 hierarchy 모두에서 분리한다

## 5. FirstScreenVisualDressing Hierarchy

권장 루트:

- `FirstScreenVisualDressing`

자식 그룹:

- `Ground`
- `NoticeLandmark`
- `BuildingFronts`
- `BoundaryBlockers`
- `Props`
- `LightingCameraHelpers`

목적:

- visual-only dressing을 한 루트 아래에서 교체/정리 가능하게 유지
- gameplay anchor에 종속되지 않는 presentation layer 확보

## 6. Codex Task After Assets Are Imported

assets import 이후 Codex가 할 일:

1. inspect asset folders
2. list usable prefabs / materials / meshes
3. propose a first-screen dressing plan
4. do not place anything until user approves

즉:

- Codex는 asset inventory와 placement plan을 먼저 정리한다
- 실제 배치/수정은 사용자 승인 뒤 진행한다

## 7. User Decision Required

확정 필요 항목:

- free only vs paid allowed
- selected asset source
- manual placement allowed yes/no
- keep low-poly style yes/no

현재 이미 확정된 항목:

- free assets first
- manual Unity Editor placement allowed
- stylized low-poly allowed
- current primitive visuals are gameplay anchor/reference only

현재 남은 핵심 결정:

- 어떤 free asset source를 쓸지
- free only를 유지할지, 필요하면 paid도 열지

## Immediate Next Step

- 먼저 사용할 free asset source를 정한다.
- 그 다음 import 대상 카테고리가 first-screen 구성에 충분한지 확인한다.
- 그 뒤에만 DeepStake3D first-screen dressing implementation으로 들어간다.

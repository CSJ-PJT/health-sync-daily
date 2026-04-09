# Fifth Dawn Store

## 상품 ID 전략

- 상품 ID는 로컬 모의 결제, 향후 App Store Connect, 향후 Play Console, 백엔드 카탈로그 메타데이터에서 모두 동일하게 유지합니다.
- 현재 기본 카탈로그 ID:
  - `fd_founders_pack`
  - `fd_starter_pack_01`
  - `fd_building_pack_luminous_garden`
  - `fd_building_pack_star_hub`
  - `fd_residence_pack_origin_home`
  - `fd_cosmetic_pack_dawn_beacons`

## 현재 구현 상태

- Fifth Dawn은 현재 개발용 모의 결제 어댑터를 사용합니다.
- 영구 해금은 아래 경로로 관리합니다.
  - 로컬 구매 기록
  - 로컬 entitlement 기록
  - 중앙 unlock resolver
- 이 구조로 오프라인 부팅은 유지하면서, 이후 StoreKit / Play Billing 검증 계층을 붙일 수 있습니다.

## App Store Connect 수동 설정

- 모든 영구 상품을 비소모성 인앱 상품으로 생성합니다.
- 상품 ID는 현재 코드의 ID와 완전히 동일하게 유지합니다.
- 가격, 지역화 문구, 심사용 스크린샷, 심사 노트, 상점 메타데이터는 수동으로 입력해야 합니다.
- 이후 Apple billing adapter를 연결할 때 현재 카탈로그 ID를 그대로 사용합니다.

## Play Console 수동 설정

- 모든 영구 상품을 일회성 인앱 상품으로 생성합니다.
- 상품 ID는 현재 코드의 ID와 완전히 동일하게 유지합니다.
- 가격 템플릿, 번역 문구, 심사용 스크린샷, 검토 자료는 수동 설정이 필요합니다.
- 이후 Google Play Billing adapter를 연결할 때 현재 카탈로그 ID를 그대로 사용합니다.

## 구매 복원

- 장기적으로는 스토어 계정 기준 복원 후 서버 검증을 거쳐 entitlement를 재구성해야 합니다.
- 현재 모의 복원은 로컬에서 `fulfilled` 상태인 구매 기록만 다시 entitlement로 복구합니다.

## 다음 단계

- StoreKit / Play Billing 어댑터 추가
- 영수증 / purchase token 서버 검증 경계 추가
- 로컬 모의 entitlement 대신 서버 검증 기반 durable entitlement로 점진 전환

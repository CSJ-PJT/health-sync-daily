import type { SettlementState } from "@/game/settlement/settlementTypes";

export function getSettlementUpgradeCost(level: SettlementState["level"]) {
  switch (level) {
    case 1:
      return 10;
    case 2:
      return 20;
    case 3:
      return 0;
  }
}

export function getSettlementTierLabel(level: SettlementState["level"]) {
  switch (level) {
    case 1:
      return "개척 거점";
    case 2:
      return "회복 정착지";
    case 3:
      return "공명 거주지";
  }
}

export function getSettlementUnlockedHighlights(state: SettlementState) {
  if (state.level === 1) {
    return ["정원 구역", "기본 배치", "거주 중심지"];
  }
  if (state.level === 2) {
    return ["등불", "바닥 장식", "휴식 공간"];
  }
  return ["공명 비콘", "감시 타워", "고급 방호"];
}

export function getSettlementFacilities(state: SettlementState) {
  const facilities = ["거주 중심지", "작업 공간"];

  if (state.level >= 2) {
    facilities.push("정화 공방", "휴식 공간", "등불 보관대");
  }

  if (state.level >= 3) {
    facilities.push("공명 비콘", "감시 타워", "고급 방호");
  }

  if (state.restoredStructures.includes("purity-lantern")) {
    facilities.push("정화 회로");
  }

  if (state.restoredStructures.includes("north-bridge")) {
    facilities.push("북쪽 통로");
  }

  if (state.restoredStructures.includes("north-outpost")) {
    facilities.push("북부 전초기지");
  }

  return facilities;
}

export function getNextSettlementGoal(state: SettlementState) {
  if (state.level === 1) {
    return "정화 공방과 휴식 공간을 여는 다음 단계까지 공명을 더 모아 보세요.";
  }
  if (state.level === 2) {
    return "공명 비콘과 고급 방호를 열어 최종 정착지 단계로 올라가세요.";
  }
  if (!state.restoredStructures.includes("north-outpost")) {
    return "북부 전초기지를 열어 상위 거주지 확장의 기반을 확보하세요.";
  }
  return "정착지 기반이 안정화됐습니다. 다음 거주 구역과 관문 단서를 준비하세요.";
}

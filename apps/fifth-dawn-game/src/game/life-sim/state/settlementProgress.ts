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
    return ["정원 화단", "기본 벤치", "거주 중심지"];
  }
  if (state.level === 2) {
    return ["등불", "바닥 패널", "러닝 게이트"];
  }
  return ["비콘", "감시 타워", "외곽 방벽"];
}

export function getSettlementFacilities(state: SettlementState) {
  const facilities = ["거주 중심지", "작은 작업대"];

  if (state.level >= 2) {
    facilities.push("정화 공방", "러닝 게이트", "등불 네트워크");
  }

  if (state.level >= 3) {
    facilities.push("공명 비콘", "감시 타워", "외곽 방벽");
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
    return "정화 공방과 러닝 게이트가 열릴 때까지 공명을 모으세요.";
  }
  if (state.level === 2) {
    return "공명 비콘과 외곽 방벽이 열리는 최종 단계로 향하세요.";
  }
  if (!state.restoredStructures.includes("north-outpost")) {
    return "북부 전초기지를 열어 외곽 정착지 기반을 확장하세요.";
  }
  return "정착지 기반이 안정화되었습니다. 다음 거주 구역 설계를 준비하세요.";
}

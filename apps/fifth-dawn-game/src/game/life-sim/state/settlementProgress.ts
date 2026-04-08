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
  return ["비콘", "타워", "벽체"];
}

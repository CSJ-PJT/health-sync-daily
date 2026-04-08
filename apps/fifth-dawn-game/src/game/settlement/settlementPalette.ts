import type { SettlementObjectType, SettlementTerrain } from "@/game/settlement/settlementTypes";

export const settlementTerrainPalette: Array<{ label: string; terrain: SettlementTerrain }> = [
  { label: "잔디", terrain: "grass" },
  { label: "모래", terrain: "sand" },
  { label: "물", terrain: "water" },
  { label: "트랙", terrain: "track" },
  { label: "석재", terrain: "stone" },
  { label: "정원", terrain: "garden" },
  { label: "광장", terrain: "plaza" },
];

export const settlementObjectPalette: Array<{ label: string; type: SettlementObjectType; emoji: string }> = [
  { label: "핵심 주거지", type: "home-core", emoji: "🏠" },
  { label: "나무", type: "tree", emoji: "🌳" },
  { label: "벤치", type: "bench", emoji: "🪑" },
  { label: "등불", type: "lamp", emoji: "🏮" },
  { label: "꽃", type: "flower", emoji: "🌸" },
  { label: "트랙 게이트", type: "track-gate", emoji: "🏁" },
  { label: "벽", type: "wall", emoji: "🧱" },
  { label: "바닥", type: "floor", emoji: "🟫" },
  { label: "타워", type: "tower", emoji: "🗼" },
  { label: "표지판", type: "sign", emoji: "🪧" },
  { label: "정원 화단", type: "garden-bed", emoji: "🪴" },
  { label: "공명 비콘", type: "beacon", emoji: "✨" },
];

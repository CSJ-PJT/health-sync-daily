import type { SettlementObjectType, SettlementTerrain } from "@/game/settlement/settlementTypes";

export const settlementTerrainPalette: Array<{ label: string; terrain: SettlementTerrain }> = [
  { label: "잔디", terrain: "grass" },
  { label: "모래", terrain: "sand" },
  { label: "물", terrain: "water" },
  { label: "러닝 트랙", terrain: "track" },
  { label: "석재", terrain: "stone" },
  { label: "정원", terrain: "garden" },
  { label: "광장", terrain: "plaza" },
];

export const settlementObjectPalette: Array<{ label: string; type: SettlementObjectType; emoji: string; unlockLevel: 1 | 2 | 3 }> = [
  { label: "거주 중심지", type: "home-core", emoji: "🏠", unlockLevel: 1 },
  { label: "나무", type: "tree", emoji: "🌳", unlockLevel: 1 },
  { label: "벤치", type: "bench", emoji: "🪑", unlockLevel: 1 },
  { label: "정원 화단", type: "garden-bed", emoji: "🪴", unlockLevel: 1 },
  { label: "표지판", type: "sign", emoji: "🪧", unlockLevel: 1 },
  { label: "등불", type: "lamp", emoji: "🏮", unlockLevel: 2 },
  { label: "꽃 장식", type: "flower", emoji: "🌸", unlockLevel: 2 },
  { label: "바닥 패널", type: "floor", emoji: "🟫", unlockLevel: 2 },
  { label: "러닝 게이트", type: "track-gate", emoji: "🏁", unlockLevel: 2 },
  { label: "벽체", type: "wall", emoji: "🧱", unlockLevel: 3 },
  { label: "감시 타워", type: "tower", emoji: "🗼", unlockLevel: 3 },
  { label: "공명 비콘", type: "beacon", emoji: "✨", unlockLevel: 3 },
];

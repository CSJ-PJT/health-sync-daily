import type { SettlementObjectType, SettlementTerrain, SettlementTheme } from "@/game/settlement/settlementTypes";

export const settlementTerrainPalette: Array<{ label: string; terrain: SettlementTerrain }> = [
  { label: "잔디", terrain: "grass" },
  { label: "모래", terrain: "sand" },
  { label: "물", terrain: "water" },
  { label: "트랙", terrain: "track" },
  { label: "석재", terrain: "stone" },
  { label: "정원", terrain: "garden" },
  { label: "광장", terrain: "plaza" },
];

export const settlementThemePalette: Array<{ label: string; theme: SettlementTheme }> = [
  { label: "복구 농장", theme: "recovery-farm" },
  { label: "여명 광장", theme: "dawn-square" },
  { label: "기본 스타 허브", theme: "star-hub" },
  { label: "빛의 정원", theme: "luminous-garden" },
  { label: "스타 허브 프라임", theme: "star-hub-prime" },
  { label: "오리진 홈", theme: "origin-home" },
];

export const settlementObjectPalette: Array<{
  label: string;
  type: SettlementObjectType;
  emoji: string;
  unlockLevel: 1 | 2 | 3;
}> = [
  { label: "거주 핵심", type: "home-core", emoji: "⌂", unlockLevel: 1 },
  { label: "나무", type: "tree", emoji: "🌲", unlockLevel: 1 },
  { label: "벤치", type: "bench", emoji: "🪑", unlockLevel: 1 },
  { label: "정원 화단", type: "garden-bed", emoji: "🪴", unlockLevel: 1 },
  { label: "표지판", type: "sign", emoji: "🪧", unlockLevel: 1 },
  { label: "가로등", type: "lamp", emoji: "💡", unlockLevel: 2 },
  { label: "꽃 장식", type: "flower", emoji: "🌸", unlockLevel: 2 },
  { label: "바닥 패널", type: "floor", emoji: "⬜", unlockLevel: 2 },
  { label: "트랙 게이트", type: "track-gate", emoji: "🏁", unlockLevel: 2 },
  { label: "벽체", type: "wall", emoji: "🧱", unlockLevel: 3 },
  { label: "감시 탑", type: "tower", emoji: "🗼", unlockLevel: 3 },
  { label: "공명 비콘", type: "beacon", emoji: "✨", unlockLevel: 3 },
  { label: "파운더 배너", type: "founder-banner", emoji: "🏳️", unlockLevel: 1 },
  { label: "파운더 첨탑", type: "founder-spire", emoji: "🜂", unlockLevel: 2 },
  { label: "스타터 상자", type: "dawn-crate", emoji: "📦", unlockLevel: 1 },
  { label: "스타터 등불", type: "starter-lamp", emoji: "🏮", unlockLevel: 1 },
  { label: "빛의 아치", type: "luminous-arch", emoji: "🌿", unlockLevel: 2 },
  { label: "정원 비콘", type: "garden-beacon", emoji: "💠", unlockLevel: 2 },
  { label: "스타 게이트", type: "star-gate", emoji: "🌀", unlockLevel: 3 },
  { label: "포털 첨탑", type: "portal-spire", emoji: "🔷", unlockLevel: 3 },
  { label: "허브 광장 코어", type: "hub-plaza", emoji: "✦", unlockLevel: 3 },
  { label: "오리진 스크린", type: "origin-screen", emoji: "🪟", unlockLevel: 2 },
  { label: "오리진 침대", type: "origin-bed", emoji: "🛏️", unlockLevel: 2 },
];

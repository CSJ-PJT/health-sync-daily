export type SettlementTerrain = "grass" | "sand" | "water" | "track" | "stone" | "garden" | "plaza";

export type SettlementTile = {
  x: number;
  y: number;
  terrain: SettlementTerrain;
};

export type SettlementTheme =
  | "recovery-farm"
  | "dawn-square"
  | "star-hub"
  | "luminous-garden"
  | "star-hub-prime"
  | "origin-home";

export type SettlementObjectType =
  | "tree"
  | "bench"
  | "lamp"
  | "flower"
  | "track-gate"
  | "wall"
  | "floor"
  | "tower"
  | "sign"
  | "home-core"
  | "garden-bed"
  | "beacon"
  | "founder-banner"
  | "founder-spire"
  | "dawn-crate"
  | "starter-lamp"
  | "luminous-arch"
  | "garden-beacon"
  | "star-gate"
  | "portal-spire"
  | "hub-plaza"
  | "origin-screen"
  | "origin-bed";

export type SettlementObject = {
  id: string;
  type: SettlementObjectType;
  x: number;
  y: number;
  rotation?: number;
  color?: string;
  ownerUserId?: string;
};

export type SettlementUpgradeLevel = 1 | 2 | 3;

export type SettlementState = {
  worldId: string;
  version: number;
  width: number;
  height: number;
  title: string;
  theme: SettlementTheme;
  level: SettlementUpgradeLevel;
  tiles: SettlementTile[];
  objects: SettlementObject[];
  unlockedObjectTypes: SettlementObjectType[];
  restoredStructures: string[];
  likes: number;
  visits: number;
};

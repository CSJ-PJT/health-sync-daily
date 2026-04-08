export type SettlementTerrain = "grass" | "sand" | "water" | "track" | "stone" | "garden" | "plaza";

export type SettlementTile = {
  x: number;
  y: number;
  terrain: SettlementTerrain;
};

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
  | "beacon";

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
  theme: "recovery-farm" | "dawn-square" | "star-hub";
  level: SettlementUpgradeLevel;
  tiles: SettlementTile[];
  objects: SettlementObject[];
  unlockedObjectTypes: SettlementObjectType[];
  restoredStructures: string[];
  likes: number;
  visits: number;
};

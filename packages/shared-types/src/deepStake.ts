export type DeepStakeBootMode = "cloud" | "local" | "degraded";

export type DeepStakeMapId = "farm" | "village" | "mine" | "north-pass";
export type DeepStakeFacing = "up" | "down" | "left" | "right";

export type DeepStakeItemId =
  | "hoe"
  | "watering-can"
  | "pickaxe"
  | "turnip-seeds"
  | "turnip"
  | "ore-fragment"
  | "scrap-bundle"
  | "resonance-shard"
  | "purity-lantern"
  | "dawn-broth"
  | "bridge-kit";

export type DeepStakeNpcId = "archivist" | "mechanic";

export type DeepStakeQuestId =
  | "first-harvest"
  | "mine-recon"
  | "repair-lantern"
  | "restore-bridge"
  | "north-reach";

export type DeepStakeQuestStatus = "available" | "completed";

export type DeepStakeFactionId =
  | "luminous-companions"
  | "serpent-court"
  | "deep-archive"
  | "dawnkeepers"
  | "shadow-administration";

export type DeepStakeBandId =
  | "unstable-shadow-band"
  | "mixed-threshold-band"
  | "dawn-aligned-band"
  | "high-luminous-band";

export type DeepStakeAscensionStage =
  | "three-d-world"
  | "four-d-threshold"
  | "dawn-transition"
  | "fifth-threshold"
  | "true-ascension"
  | "five-d-residency";

export type DeepStakeChoiceId =
  | "village-recovery-vow"
  | "accept-luminous-guidance"
  | "study-deep-archive"
  | "accept-shadow-bargain";

export type DeepStakeSettlementTheme =
  | "recovery-farm"
  | "dawn-square"
  | "star-hub"
  | "luminous-garden"
  | "star-hub-prime"
  | "origin-home";

export type DeepStakeSettlementObjectType =
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

export type DeepStakeInventory = Partial<Record<DeepStakeItemId, number>>;

export type DeepStakePlayerContract = {
  mapId: DeepStakeMapId;
  x: number;
  y: number;
  facing: DeepStakeFacing;
  energy: number;
  maxEnergy: number;
  inventory: DeepStakeInventory;
  hotbar: DeepStakeItemId[];
  selectedHotbarIndex: number;
};

export type DeepStakeCropPlotContract = {
  x: number;
  y: number;
  tilled: boolean;
  wateredOnDay?: number;
  cropKind?: "turnip";
  plantedOnDay?: number;
  growthStage: number;
  readyToHarvest: boolean;
};

export type DeepStakeResourceNodeContract = {
  id: string;
  mapId: DeepStakeMapId;
  x: number;
  y: number;
  itemId: "ore-fragment" | "scrap-bundle" | "resonance-shard";
  depletedUntilDay?: number;
};

export type DeepStakeHazardContract = {
  id: string;
  mapId: DeepStakeMapId;
  x: number;
  y: number;
  axis: "x" | "y";
  direction: 1 | -1;
  range: [number, number];
  labelKey: string;
};

export type DeepStakeRelationshipContract = {
  friendship: number;
  lastTalkDay?: number;
  level: 0 | 1 | 2 | 3;
  rewardedLevels: number[];
};

export type DeepStakeAlignmentContract = {
  luminousAffinity: number;
  shadowAffinity: number;
  resonanceStability: number;
  corruptionPressure: number;
  awakeningClarity: number;
  compassion: number;
  domination: number;
  attunement: number;
};

export type DeepStakeFactionAffinitiesContract = Record<DeepStakeFactionId, number>;

export type DeepStakeSupportSignalContract = {
  id: string;
  type: "blessing-pulse" | "encouragement-signal" | "protective-wave" | "shadow-whisper";
  sourceFactionId: DeepStakeFactionId;
  strength: number;
  sensed: boolean;
  noteKey: string;
};

export type DeepStakeWarFoundationContract = {
  resonanceGroupingKey: string;
  preferredSide: "luminous" | "threshold" | "shadow";
  territorialPressure: number;
  futureSquadEligible: boolean;
};

export type DeepStakeCodexEntryContract = {
  id: string;
  unlocked: boolean;
};

export type DeepStakeWorldHintContract = {
  id: string;
  unlocked: boolean;
};

export type DeepStakeAlignmentProfileContract = {
  alignment: DeepStakeAlignmentContract;
  factionAffinities: DeepStakeFactionAffinitiesContract;
  chosenPaths: DeepStakeChoiceId[];
  resonanceBand: DeepStakeBandId;
  supportSignals: DeepStakeSupportSignalContract[];
  warFoundation: DeepStakeWarFoundationContract;
  ascensionStage: DeepStakeAscensionStage;
  trueAscensionUnlocked: boolean;
  fiveDResidencyUnlocked: boolean;
  codexEntries: DeepStakeCodexEntryContract[];
  worldHints: DeepStakeWorldHintContract[];
};

export type DeepStakeStoryFlagsContract = {
  metArchivist: boolean;
  metMechanic: boolean;
  enteredMine: boolean;
  harvestedFirstCrop: boolean;
  repairedLantern: boolean;
  cookedFirstMeal: boolean;
  restoredBridge: boolean;
  surveyedNorthReach: boolean;
};

export type DeepStakeQuestContract = {
  id: DeepStakeQuestId;
  status: DeepStakeQuestStatus;
  completedOnDay?: number;
};

export type DeepStakeProgressionContract = {
  resonancePoints: number;
  unlockedRecipes: string[];
  discoveredMaps: DeepStakeMapId[];
  completedQuestIds: DeepStakeQuestId[];
};

export type DeepStakeSettlementObjectContract = {
  id: string;
  type: DeepStakeSettlementObjectType;
  x: number;
  y: number;
  rotation?: number;
  color?: string;
};

export type DeepStakeSettlementTileContract = {
  x: number;
  y: number;
  terrain: "grass" | "sand" | "water" | "track" | "stone" | "garden" | "plaza";
};

export type DeepStakeSettlementContract = {
  worldId: string;
  version: number;
  width: number;
  height: number;
  title: string;
  theme: DeepStakeSettlementTheme;
  level: 1 | 2 | 3;
  tiles: DeepStakeSettlementTileContract[];
  objects: DeepStakeSettlementObjectContract[];
  unlockedObjectTypes: DeepStakeSettlementObjectType[];
  restoredStructures: string[];
  likes: number;
  visits: number;
};

export type DeepStakeHealthBonusContract = {
  startEnergyBonus: number;
  recoveryBonus: number;
  cropEfficiencyBonus: number;
};

export type DeepStakeSaveContract = {
  schemaVersion: 1;
  slot: string;
  bootMode: DeepStakeBootMode;
  player: DeepStakePlayerContract;
  time: {
    day: number;
    minutes: number;
  };
  world: {
    plots: DeepStakeCropPlotContract[];
    resourceNodes: DeepStakeResourceNodeContract[];
    hazards: DeepStakeHazardContract[];
    storyFlags: DeepStakeStoryFlagsContract;
  };
  relationships: Record<DeepStakeNpcId, DeepStakeRelationshipContract>;
  quests: DeepStakeQuestContract[];
  progression: DeepStakeProgressionContract;
  alignmentProfile: DeepStakeAlignmentProfileContract;
  settlement: DeepStakeSettlementContract;
  healthBonuses: DeepStakeHealthBonusContract;
  lastDialogue?: {
    npcId: DeepStakeNpcId;
    lines: string[];
  };
};

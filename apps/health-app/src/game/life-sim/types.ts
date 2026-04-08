export type LocaleCode = "ko" | "en";

export type LocalizedText = {
  ko: string;
  en: string;
};

export type LifeSimMapId = "farm" | "village" | "mine";
export type LifeSimFacing = "up" | "down" | "left" | "right";
export type LifeSimTerrain =
  | "grass"
  | "path"
  | "soil"
  | "tilled"
  | "water"
  | "wood"
  | "stone"
  | "floor"
  | "ruin"
  | "wall";

export type LifeSimTile = {
  x: number;
  y: number;
  terrain: LifeSimTerrain;
  walkable: boolean;
  tillable?: boolean;
  warpTo?: {
    mapId: LifeSimMapId;
    x: number;
    y: number;
  };
  bed?: boolean;
  signText?: LocalizedText;
};

export type LifeSimMapDefinition = {
  id: LifeSimMapId;
  name: LocalizedText;
  width: number;
  height: number;
  tiles: LifeSimTile[];
  ambientHint: LocalizedText;
};

export type LifeSimItemId =
  | "hoe"
  | "watering-can"
  | "pickaxe"
  | "turnip-seeds"
  | "turnip"
  | "ore-fragment"
  | "scrap-bundle"
  | "purity-lantern";

export type LifeSimItemDefinition = {
  id: LifeSimItemId;
  name: LocalizedText;
  description: LocalizedText;
  stackable: boolean;
  category: "tool" | "seed" | "crop" | "resource" | "artifact";
};

export type LifeSimCropKind = "turnip";

export type LifeSimCropPlot = {
  x: number;
  y: number;
  tilled: boolean;
  wateredOnDay?: number;
  cropKind?: LifeSimCropKind;
  plantedOnDay?: number;
  growthStage: number;
  readyToHarvest: boolean;
};

export type LifeSimResourceNode = {
  id: string;
  mapId: LifeSimMapId;
  x: number;
  y: number;
  itemId: "ore-fragment" | "scrap-bundle";
  depletedUntilDay?: number;
};

export type LifeSimHazard = {
  id: string;
  mapId: LifeSimMapId;
  x: number;
  y: number;
  direction: 1 | -1;
  axis: "x" | "y";
  range: [number, number];
  label: LocalizedText;
};

export type LifeSimNpcId = "archivist" | "mechanic";

export type LifeSimNpcDefinition = {
  id: LifeSimNpcId;
  name: LocalizedText;
  mapId: LifeSimMapId;
  x: number;
  y: number;
};

export type LifeSimDialogueLine = {
  id: string;
  speaker: LifeSimNpcId;
  text: LocalizedText;
  condition?: "default" | "has-turnip" | "mine-visited" | "low-energy" | "first-day";
};

export type LifeSimRelationshipState = {
  friendship: number;
  lastTalkDay?: number;
};

export type LifeSimStoryFlags = {
  metArchivist: boolean;
  metMechanic: boolean;
  enteredMine: boolean;
  harvestedFirstCrop: boolean;
  repairedLantern: boolean;
};

export type LifeSimInventory = Record<LifeSimItemId, number>;

export type LifeSimPlayerState = {
  mapId: LifeSimMapId;
  x: number;
  y: number;
  facing: LifeSimFacing;
  energy: number;
  maxEnergy: number;
  inventory: LifeSimInventory;
  hotbar: LifeSimItemId[];
  selectedHotbarIndex: number;
};

export type LifeSimTimeState = {
  day: number;
  minutes: number;
};

export type LifeSimHealthBonuses = {
  startEnergyBonus: number;
  recoveryBonus: number;
  cropEfficiencyBonus: number;
};

export type LifeSimSettings = {
  resolutionScale: number;
  fullscreenPreferred: boolean;
  audioVolume: number;
  keyBindings: Record<LifeSimInputAction, string[]>;
};

export type LifeSimInputAction =
  | "move-up"
  | "move-down"
  | "move-left"
  | "move-right"
  | "interact"
  | "use-tool"
  | "sleep"
  | "hotbar-1"
  | "hotbar-2"
  | "hotbar-3"
  | "hotbar-4"
  | "hotbar-5";

export type LifeSimEventName =
  | "crop_harvested"
  | "npc_talked"
  | "resource_mined"
  | "hazard_hit"
  | "day_ended"
  | "story_flag_changed";

export type LifeSimGameplayEvent = {
  id: string;
  name: LifeSimEventName;
  payload: Record<string, unknown>;
  createdAt: string;
};

export type LifeSimUiMessage = {
  id: string;
  title: string;
  body: string;
};

export type LifeSimState = {
  version: number;
  slot: string;
  player: LifeSimPlayerState;
  time: LifeSimTimeState;
  plots: LifeSimCropPlot[];
  resourceNodes: LifeSimResourceNode[];
  hazards: LifeSimHazard[];
  relationships: Record<LifeSimNpcId, LifeSimRelationshipState>;
  storyFlags: LifeSimStoryFlags;
  healthBonuses: LifeSimHealthBonuses;
  settings: LifeSimSettings;
  lastDialogue?: {
    npcId: LifeSimNpcId;
    lines: string[];
  };
  messageLog: LifeSimUiMessage[];
  eventLog: LifeSimGameplayEvent[];
};

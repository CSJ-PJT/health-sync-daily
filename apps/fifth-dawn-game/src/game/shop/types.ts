import type { SettlementObjectType, SettlementTheme } from "@/game/settlement/settlementTypes";

export type FifthDawnProductId =
  | "fd_founders_pack"
  | "fd_starter_pack_01"
  | "fd_building_pack_luminous_garden"
  | "fd_building_pack_star_hub"
  | "fd_residence_pack_origin_home"
  | "fd_cosmetic_pack_dawn_beacons";

export type CommerceProductType = "non-consumable" | "consumable" | "subscription-compatible";
export type CommercePurchaseStatus = "pending" | "fulfilled" | "refunded" | "revoked";
export type BillingAvailability = "ready" | "mock" | "unavailable";

export type FifthDawnUnlockSet = {
  settlementObjectTypes: SettlementObjectType[];
  settlementThemes: SettlementTheme[];
  profileTitles: string[];
  profileBadges: string[];
  profileFrames: string[];
  beaconSkins: string[];
  starterBonusResonance?: number;
};

export type CommerceProduct = {
  id: FifthDawnProductId;
  platformProductId: string;
  title: string;
  summary: string;
  category: "founder" | "starter" | "building" | "residence" | "cosmetic";
  type: CommerceProductType;
  fairUseNote: string;
  unlockPreview: string[];
  unlocks: FifthDawnUnlockSet;
};

export type CommercePurchaseRecord = {
  id: string;
  productId: FifthDawnProductId;
  status: CommercePurchaseStatus;
  purchasedAt: string;
  source: "mock-billing" | "restore" | "future-storekit" | "future-play-billing";
  verificationState: "unverified-local" | "future-server-verified";
};

export type CommerceEntitlement = {
  productId: FifthDawnProductId;
  owned: boolean;
  grantedAt?: string;
  status: CommercePurchaseStatus;
};

export type CommerceCatalogState = {
  products: CommerceProduct[];
  purchases: CommercePurchaseRecord[];
  entitlements: CommerceEntitlement[];
  availability: BillingAvailability;
  diagnosticsMessage: string;
};

export type BillingPurchaseResult = {
  purchase: CommercePurchaseRecord | null;
  reason?: string;
};

export type BillingProvider = {
  listProducts: () => Promise<CommerceProduct[]>;
  purchase: (productId: FifthDawnProductId) => Promise<BillingPurchaseResult>;
  restore: () => Promise<CommercePurchaseRecord[]>;
  availability: BillingAvailability;
};

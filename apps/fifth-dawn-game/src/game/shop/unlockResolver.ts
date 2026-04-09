import { fifthDawnCatalog } from "@/game/shop/catalog";
import type {
  CommerceEntitlement,
  CommerceProduct,
  FifthDawnProductId,
  FifthDawnUnlockSet,
} from "@/game/shop/types";

const emptyUnlocks: FifthDawnUnlockSet = {
  settlementObjectTypes: [],
  settlementThemes: [],
  profileTitles: [],
  profileBadges: [],
  profileFrames: [],
  beaconSkins: [],
  starterBonusResonance: 0,
};

export function getProductById(productId: FifthDawnProductId) {
  return fifthDawnCatalog.find((product) => product.id === productId) || null;
}

export function resolveOwnedProducts(products: CommerceProduct[], entitlements: CommerceEntitlement[]) {
  const owned = new Set(entitlements.filter((entry) => entry.owned && entry.status === "fulfilled").map((entry) => entry.productId));
  return products.filter((product) => owned.has(product.id));
}

export function resolveUnlocksFromEntitlements(entitlements: CommerceEntitlement[]): FifthDawnUnlockSet {
  return entitlements.reduce<FifthDawnUnlockSet>((accumulator, entitlement) => {
    if (!entitlement.owned || entitlement.status !== "fulfilled") {
      return accumulator;
    }

    const product = getProductById(entitlement.productId);
    if (!product) {
      return accumulator;
    }

    return {
      settlementObjectTypes: Array.from(new Set([...accumulator.settlementObjectTypes, ...product.unlocks.settlementObjectTypes])),
      settlementThemes: Array.from(new Set([...accumulator.settlementThemes, ...product.unlocks.settlementThemes])),
      profileTitles: Array.from(new Set([...accumulator.profileTitles, ...product.unlocks.profileTitles])),
      profileBadges: Array.from(new Set([...accumulator.profileBadges, ...product.unlocks.profileBadges])),
      profileFrames: Array.from(new Set([...accumulator.profileFrames, ...product.unlocks.profileFrames])),
      beaconSkins: Array.from(new Set([...accumulator.beaconSkins, ...product.unlocks.beaconSkins])),
      starterBonusResonance:
        (accumulator.starterBonusResonance || 0) + (product.unlocks.starterBonusResonance || 0),
    };
  }, emptyUnlocks);
}

import { fifthDawnCatalog } from "@/game/shop/catalog";
import type {
  BillingProvider,
  BillingPurchaseResult,
  CommerceProduct,
  CommercePurchaseRecord,
  FifthDawnProductId,
} from "@/game/shop/types";

function createPurchaseRecord(productId: FifthDawnProductId, source: CommercePurchaseRecord["source"]): CommercePurchaseRecord {
  return {
    id: `purchase-${productId}-${Math.random().toString(36).slice(2, 10)}`,
    productId,
    status: "fulfilled",
    purchasedAt: new Date().toISOString(),
    source,
    verificationState: "unverified-local",
  };
}

export function createMockBillingProvider(existingPurchases: CommercePurchaseRecord[]): BillingProvider {
  return {
    availability: "mock",
    async listProducts(): Promise<CommerceProduct[]> {
      return fifthDawnCatalog;
    },
    async purchase(productId: FifthDawnProductId): Promise<BillingPurchaseResult> {
      const alreadyOwned = existingPurchases.some((entry) => entry.productId === productId && entry.status === "fulfilled");
      if (alreadyOwned) {
        return {
          purchase: null,
          reason: "이미 보유 중인 상품입니다.",
        };
      }

      return {
        purchase: createPurchaseRecord(productId, "mock-billing"),
      };
    },
    async restore() {
      return existingPurchases.filter((entry) => entry.status === "fulfilled");
    },
  };
}

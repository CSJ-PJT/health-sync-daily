import { fifthDawnCatalog } from "@/game/shop/catalog";
import { createMockBillingProvider } from "@/game/shop/billing";
import { resolveUnlocksFromEntitlements } from "@/game/shop/unlockResolver";
import type {
  CommerceCatalogState,
  CommerceEntitlement,
  CommerceProduct,
  CommercePurchaseRecord,
  FifthDawnProductId,
} from "@/game/shop/types";

const PURCHASES_KEY = "deep_stake_commerce_purchases";
const ENTITLEMENTS_KEY = "deep_stake_commerce_entitlements";

function readJson<T>(key: string, fallback: T): T {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : fallback;
  } catch {
    return fallback;
  }
}

function writeJson<T>(key: string, value: T) {
  localStorage.setItem(key, JSON.stringify(value));
}

function buildEntitlementsFromPurchases(purchases: CommercePurchaseRecord[]): CommerceEntitlement[] {
  return fifthDawnCatalog.map((product) => {
    const latest = purchases
      .filter((entry) => entry.productId === product.id)
      .sort((left, right) => right.purchasedAt.localeCompare(left.purchasedAt))[0];

    return {
      productId: product.id,
      owned: latest?.status === "fulfilled",
      grantedAt: latest?.purchasedAt,
      status: latest?.status || "pending",
    };
  });
}

export async function loadCommerceCatalogState(): Promise<CommerceCatalogState> {
  const purchases = readJson<CommercePurchaseRecord[]>(PURCHASES_KEY, []);
  const entitlements = readJson<CommerceEntitlement[]>(ENTITLEMENTS_KEY, buildEntitlementsFromPurchases(purchases));

  return {
    products: fifthDawnCatalog,
    purchases,
    entitlements,
    availability: "mock",
    diagnosticsMessage: "현재는 개발용 모의 결제 어댑터를 사용합니다. 이후 StoreKit / Play Billing 검증 경계로 교체됩니다.",
  };
}

export async function purchaseCatalogProduct(productId: FifthDawnProductId): Promise<CommerceCatalogState> {
  const current = await loadCommerceCatalogState();
  const provider = createMockBillingProvider(current.purchases);
  const result = await provider.purchase(productId);

  if (!result.purchase) {
    return {
      ...current,
      diagnosticsMessage: result.reason || "구매를 진행하지 못했습니다.",
    };
  }

  const purchases = [...current.purchases, result.purchase];
  const entitlements = buildEntitlementsFromPurchases(purchases);
  writeJson(PURCHASES_KEY, purchases);
  writeJson(ENTITLEMENTS_KEY, entitlements);

  return {
    ...current,
    purchases,
    entitlements,
    diagnosticsMessage: `${productId} 구매를 모의 처리했습니다. 이후 서버 검증 경계와 연결됩니다.`,
  };
}

export async function restoreCatalogPurchases(): Promise<CommerceCatalogState> {
  const current = await loadCommerceCatalogState();
  const provider = createMockBillingProvider(current.purchases);
  const purchases = await provider.restore();
  const entitlements = buildEntitlementsFromPurchases(purchases);
  writeJson(PURCHASES_KEY, purchases);
  writeJson(ENTITLEMENTS_KEY, entitlements);

  return {
    ...current,
    purchases,
    entitlements,
    diagnosticsMessage: purchases.length ? "보유 상품 복원을 완료했습니다." : "복원할 구매 내역이 없습니다.",
  };
}

export async function loadOwnedUnlocks() {
  const state = await loadCommerceCatalogState();
  return resolveUnlocksFromEntitlements(state.entitlements);
}

export function listCatalogProducts(): CommerceProduct[] {
  return fifthDawnCatalog;
}

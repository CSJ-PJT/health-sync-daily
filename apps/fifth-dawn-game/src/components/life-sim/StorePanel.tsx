import { RefreshCw, ShoppingBag, Sparkles } from "lucide-react";
import { getProductById } from "@/game/shop/unlockResolver";
import type { CommerceCatalogState, FifthDawnProductId } from "@/game/shop/types";

type Props = {
  state: CommerceCatalogState;
  onPurchase: (productId: FifthDawnProductId) => void;
  onRestore: () => void;
};

const categoryLabel = {
  founder: "파운더",
  starter: "스타터",
  building: "건축",
  residence: "거주",
  cosmetic: "코스메틱",
} as const;

export function StorePanel({ state, onPurchase, onRestore }: Props) {
  return (
    <div className="rounded-2xl border border-white/10 bg-black/20 p-4 text-sm">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2 font-medium">
          <ShoppingBag className="h-4 w-4 text-amber-200" />
          Deep Stake 상점
        </div>
        <div className="rounded-full border border-sky-300/20 bg-sky-500/10 px-2 py-1 text-[11px] text-sky-100">
          {state.availability === "mock" ? "모의 결제" : state.availability}
        </div>
      </div>

      <p className="mt-2 text-xs text-slate-300">
        정착지, 거주지, 꾸미기 중심의 공정한 카탈로그입니다. 영토 점령, 외교 성공, 승천 자격 같은 직접 우위는 판매하지 않습니다.
      </p>
      <div className="mt-2 text-[11px] text-slate-400">{state.diagnosticsMessage}</div>

      <div className="mt-4 grid gap-3">
        {state.products.map((product) => {
          const entitlement = state.entitlements.find((entry) => entry.productId === product.id);
          const owned = entitlement?.owned;

          return (
            <div key={product.id} className="rounded-2xl border border-white/10 bg-white/5 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="font-medium">{product.title}</div>
                  <div className="mt-1 text-xs uppercase tracking-[0.18em] text-amber-200/80">{categoryLabel[product.category]}</div>
                </div>
                <div
                  className={`rounded-full px-2 py-1 text-[11px] ${
                    owned ? "border border-emerald-300/20 bg-emerald-500/10 text-emerald-100" : "border border-white/10 bg-black/20 text-slate-300"
                  }`}
                >
                  {owned ? "보유 중" : "미보유"}
                </div>
              </div>

              <p className="mt-2 text-xs text-slate-300">{product.summary}</p>
              <div className="mt-3 space-y-1 text-xs text-slate-200">
                {product.unlockPreview.map((line) => (
                  <div key={line} className="flex items-start gap-2">
                    <Sparkles className="mt-0.5 h-3.5 w-3.5 text-amber-200" />
                    <span>{line}</span>
                  </div>
                ))}
              </div>
              <div className="mt-3 text-[11px] text-slate-400">{product.fairUseNote}</div>
              <button
                type="button"
                onClick={() => onPurchase(product.id)}
                disabled={owned}
                className="mt-4 w-full rounded-xl border border-white/10 bg-amber-500/15 px-3 py-3 text-sm disabled:cursor-not-allowed disabled:opacity-50"
              >
                {owned ? "이미 해금됨" : "구매 시뮬레이션"}
              </button>
            </div>
          );
        })}
      </div>

      <button type="button" onClick={onRestore} className="mt-4 w-full rounded-xl border border-white/10 bg-white/5 px-3 py-3 text-sm">
        <span className="inline-flex items-center gap-2">
          <RefreshCw className="h-4 w-4" />
          구매 복원
        </span>
      </button>

      <div className="mt-4 rounded-xl border border-white/10 bg-black/20 p-3 text-xs text-slate-300">
        <div className="font-medium">현재 해금 요약</div>
        <div className="mt-2 space-y-1">
          {state.entitlements.filter((entry) => entry.owned).length ? (
            state.entitlements
              .filter((entry) => entry.owned)
              .map((entry) => {
                const product = getProductById(entry.productId);
                return <div key={entry.productId}>- {product?.title || entry.productId}</div>;
              })
          ) : (
            <div>아직 해금한 상품이 없습니다.</div>
          )}
        </div>
      </div>
    </div>
  );
}

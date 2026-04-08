import { useEffect, useMemo, useState } from "react";
import { Link2, Sparkles } from "lucide-react";
import { FullscreenGameHost } from "@/components/FullscreenGameHost";
import { LifeSimArena } from "@/components/LifeSimArena";
import type { GameLinkBundle } from "@health-sync/shared-types";
import {
  clearStoredGameLinkToken,
  getStoredGameLinkToken,
  loadDerivedGameLinkBundle,
  setStoredGameLinkToken,
} from "@/services/repositories/gameLinkRepository";

export default function App() {
  const [tokenInput, setTokenInput] = useState(getStoredGameLinkToken());
  const [bundle, setBundle] = useState<GameLinkBundle | null>(null);
  const [loadingLink, setLoadingLink] = useState(false);

  const refreshBundle = async (token?: string) => {
    setLoadingLink(true);
    try {
      const next = await loadDerivedGameLinkBundle(token);
      setBundle(next);
    } finally {
      setLoadingLink(false);
    }
  };

  useEffect(() => {
    void refreshBundle();
  }, []);

  const subtitle = useMemo(() => {
    if (!bundle?.profile) {
      return "복구 농장, 새벽 광장, 정화 광산이 이어진 독립 생활 RPG 수직 슬라이스";
    }
    return `링크 활성화 · 활동 ${bundle.profile.activityTier} / 수면 ${bundle.profile.sleepTier} / 공명 ${bundle.profile.resonancePoints}`;
  }, [bundle]);

  return (
    <FullscreenGameHost
      title="Fifth Dawn Valley"
      subtitle={subtitle}
      sidebar={
        <div className="space-y-4 text-sm text-slate-200">
          <div>
            <div className="text-xs uppercase tracking-[0.25em] text-amber-200/70">Lore</div>
            <h2 className="mt-2 text-lg font-semibold">Longest Dawn Slice</h2>
            <p className="mt-2 text-slate-300">
              낮은 공명의 시대를 지나던 세계는 가장 긴 새벽을 통과하고 있습니다. 지상은 회복을 시작했고, 지하의 오래된 그림자
              행정 구조와 깊은 기록 보관소는 아직 완전히 사라지지 않았습니다.
            </p>
          </div>

          <div className="rounded-2xl border border-white/10 bg-black/20 p-4">
            <div className="mb-2 flex items-center gap-2 font-medium">
              <Link2 className="h-4 w-4 text-emerald-300" />
              Game Link
            </div>
            <p className="text-slate-300">
              링크 토큰을 입력하면 원본 건강 데이터가 아니라 파생 게임 전용 지표만 받아옵니다.
            </p>
            <input
              value={tokenInput}
              onChange={(event) => setTokenInput(event.target.value)}
              placeholder="link token"
              className="mt-3 w-full rounded-2xl border border-white/10 bg-black/30 px-4 py-3 text-sm outline-none placeholder:text-slate-500"
            />
            <div className="mt-3 flex gap-2">
              <button
                type="button"
                onClick={async () => {
                  setStoredGameLinkToken(tokenInput.trim());
                  await refreshBundle(tokenInput.trim());
                }}
                className="rounded-2xl border border-emerald-300/20 bg-emerald-500/15 px-4 py-2"
              >
                연결 적용
              </button>
              <button
                type="button"
                onClick={async () => {
                  clearStoredGameLinkToken();
                  setTokenInput("");
                  await refreshBundle("");
                }}
                className="rounded-2xl border border-white/10 bg-white/5 px-4 py-2"
              >
                해제
              </button>
            </div>
            <div className="mt-3 text-xs text-slate-400">
              {loadingLink
                ? "파생 지표를 불러오는 중입니다."
                : bundle?.accountLink
                  ? `연결 상태: ${bundle.accountLink.linkStatus} · 게임 계정 ${bundle.accountLink.gameAccountId}`
                  : "현재는 로컬 단독 플레이 상태입니다."}
            </div>
          </div>

          <div className="rounded-2xl border border-white/10 bg-black/20 p-4">
            <div className="mb-2 flex items-center gap-2 font-medium">
              <Sparkles className="h-4 w-4 text-sky-300" />
              파생 보너스
            </div>
            {bundle?.profile ? (
              <div className="space-y-2 text-slate-300">
                <div>활동 등급: {bundle.profile.activityTier}</div>
                <div>수면 등급: {bundle.profile.sleepTier}</div>
                <div>회복 등급: {bundle.profile.recoveryTier}</div>
                <div>주간 움직임 점수: {bundle.profile.weeklyMovementScore}</div>
                <div>공명 포인트: {bundle.profile.resonancePoints}</div>
              </div>
            ) : (
              <div className="text-slate-400">링크하지 않아도 게임은 끝까지 플레이할 수 있습니다.</div>
            )}
          </div>
        </div>
      }
    >
      <LifeSimArena />
    </FullscreenGameHost>
  );
}

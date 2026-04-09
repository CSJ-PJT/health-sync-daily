import { useEffect, useMemo, useState } from "react";
import { Link2, Sparkles } from "lucide-react";
import { FullscreenGameHost } from "@/components/FullscreenGameHost";
import { LifeSimArena } from "@/components/LifeSimArena";
import { fifthDawnMobileShell } from "@/config/mobileShell";
import type { GameLinkBundle } from "@health-sync/shared-types";
import {
  clearStoredGameAccountId,
  clearStoredGameLinkToken,
  getStoredGameAccountId,
  getStoredGameLinkToken,
  loadDerivedGameLinkBundle,
  setStoredGameAccountId,
  setStoredGameLinkToken,
} from "@/services/repositories/gameLinkRepository";

export default function App() {
  const [tokenInput, setTokenInput] = useState(getStoredGameLinkToken());
  const [gameAccountIdInput, setGameAccountIdInput] = useState(getStoredGameAccountId());
  const [bundle, setBundle] = useState<GameLinkBundle | null>(null);
  const [loadingLink, setLoadingLink] = useState(false);

  const refreshBundle = async (token?: string, gameAccountId?: string) => {
    setLoadingLink(true);
    try {
      const next = await loadDerivedGameLinkBundle(token, gameAccountId);
      setBundle(next);
    } finally {
      setLoadingLink(false);
    }
  };

  useEffect(() => {
    const background = fifthDawnMobileShell.statusBarColor;
    document.documentElement.style.backgroundColor = background;
    document.body.style.backgroundColor = background;
    void refreshBundle();
  }, []);

  const subtitle = useMemo(() => {
    if (!bundle?.profile) {
      return "복구 농장, 여명 광장, 정화 광산, 북부 개척지를 잇는 탑다운 라이프심 RPG";
    }
    return `활동 ${bundle.profile.activityTier} · 수면 ${bundle.profile.sleepTier} · 공명 ${bundle.profile.resonancePoints}`;
  }, [bundle]);

  return (
    <FullscreenGameHost
      title={fifthDawnMobileShell.appName}
      subtitle={subtitle}
      sidebar={
        <div className="space-y-4 text-sm text-slate-200">
          <div>
            <div className="text-xs uppercase tracking-[0.25em] text-amber-200/70">Lore</div>
            <h2 className="mt-2 text-lg font-semibold">Longest Dawn Slice</h2>
            <p className="mt-2 text-slate-300">
              오래된 공명층이 마을 아래에서 아직 완전히 꺼지지 않았습니다. 농장을 복구하고 지표의 깊은 기록과 그림자 행정의 흔적을
              추적해 정화 통로를 열고, 가장 긴 새벽 너머의 다음 거점으로 나아갑니다.
            </p>
          </div>

          <div className="rounded-2xl border border-white/10 bg-black/20 p-4">
            <div className="mb-2 flex items-center gap-2 font-medium">
              <Link2 className="h-4 w-4 text-emerald-300" />
              Fifth Dawn Link
            </div>
            <p className="text-slate-300">
              건강 앱에서 만든 파생 지표만 연동합니다. 원본 건강 데이터는 게임으로 직접 들어오지 않습니다.
            </p>
            <input
              value={gameAccountIdInput}
              onChange={(event) => setGameAccountIdInput(event.target.value)}
              placeholder="game account id"
              className="mt-3 w-full rounded-2xl border border-white/10 bg-black/30 px-4 py-3 text-sm outline-none placeholder:text-slate-500"
            />
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
                  setStoredGameAccountId(gameAccountIdInput.trim());
                  setStoredGameLinkToken(tokenInput.trim());
                  await refreshBundle(tokenInput.trim(), gameAccountIdInput.trim());
                }}
                className="rounded-2xl border border-emerald-300/20 bg-emerald-500/15 px-4 py-2"
              >
                링크 적용
              </button>
              <button
                type="button"
                onClick={async () => {
                  clearStoredGameAccountId();
                  clearStoredGameLinkToken();
                  setGameAccountIdInput("");
                  setTokenInput("");
                  await refreshBundle("", "");
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
                  : "현재는 로컬 플레이 전용 상태입니다."}
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
                <div>주간 이동 점수: {bundle.profile.weeklyMovementScore}</div>
                <div>공명 포인트: {bundle.profile.resonancePoints}</div>
              </div>
            ) : (
              <div className="text-slate-400">링크하지 않아도 게임은 그대로 플레이할 수 있습니다.</div>
            )}
            <div className="mt-3 text-xs text-slate-500">
              모바일 설정: {fifthDawnMobileShell.preferredOrientation} / {fifthDawnMobileShell.productKey}
            </div>
          </div>
        </div>
      }
    >
      <LifeSimArena />
    </FullscreenGameHost>
  );
}

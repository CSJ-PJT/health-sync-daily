import { useEffect, useMemo, useState } from "react";
import { CloudOff, Link2, Sparkles } from "lucide-react";
import { FullscreenGameHost } from "@/components/FullscreenGameHost";
import { GameRuntimeBoundary } from "@/components/GameRuntimeBoundary";
import { LifeSimArena } from "@/components/LifeSimArena";
import { fifthDawnMobileShell } from "@/config/mobileShell";
import { getRuntimeConfig, type FifthDawnBootMode } from "@/config/runtimeConfig";
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

const runtimeConfig = getRuntimeConfig();

function getBootModeMessage(mode: FifthDawnBootMode) {
  switch (mode) {
    case "cloud":
      return "클라우드 연동 사용 가능";
    case "degraded":
      return "클라우드 응답 없음 · 로컬 모드 계속";
    default:
      return "클라우드 설정 없음 · 로컬 모드";
  }
}

export default function App() {
  const [tokenInput, setTokenInput] = useState(getStoredGameLinkToken());
  const [gameAccountIdInput, setGameAccountIdInput] = useState(getStoredGameAccountId());
  const [bundle, setBundle] = useState<GameLinkBundle | null>(null);
  const [loadingLink, setLoadingLink] = useState(false);
  const [bootState, setBootState] = useState("게임 셸을 준비하고 있습니다.");
  const [bootMode, setBootMode] = useState<FifthDawnBootMode>(runtimeConfig.bootMode);
  const [cloudNotice, setCloudNotice] = useState(runtimeConfig.statusMessage);

  const refreshBundle = async (token?: string, gameAccountId?: string) => {
    const nextToken = token ?? tokenInput;
    const nextGameAccountId = gameAccountId ?? gameAccountIdInput;

    if (!runtimeConfig.cloudEnabled) {
      setBundle(null);
      setBootMode("local");
      setCloudNotice("클라우드 기능이 비활성화되어 로컬 저장만 사용합니다.");
      return;
    }

    setLoadingLink(true);
    try {
      const next = await loadDerivedGameLinkBundle(nextToken, nextGameAccountId);
      setBundle(next);

      if (next) {
        setBootMode("cloud");
        setCloudNotice("파생 보너스와 클라우드 저장을 사용할 수 있습니다.");
      } else if (nextToken && nextGameAccountId) {
        setBootMode("degraded");
        setCloudNotice("클라우드 응답을 받지 못해 로컬 모드로 계속 실행합니다.");
      } else {
        setBootMode("cloud");
        setCloudNotice("클라우드 구성은 준비됐지만 아직 게임 링크를 연결하지 않았습니다.");
      }
    } finally {
      setLoadingLink(false);
    }
  };

  useEffect(() => {
    const background = fifthDawnMobileShell.statusBarColor;
    document.documentElement.style.backgroundColor = background;
    document.body.style.backgroundColor = background;
    setBootState(`${fifthDawnMobileShell.appName} 앱이 실행 중입니다. ${getBootModeMessage(runtimeConfig.bootMode)}`);
    void refreshBundle(getStoredGameLinkToken(), getStoredGameAccountId());
  }, []);

  const subtitle = useMemo(() => {
    if (!bundle?.profile) {
      return "복구 농장과 정착지를 확장하며 공명 경로를 여는 탑다운 라이프심 RPG";
    }

    return `활동 ${bundle.profile.activityTier} · 수면 ${bundle.profile.sleepTier} · 공명 ${bundle.profile.resonancePoints}`;
  }, [bundle]);

  return (
    <>
      <div className="fixed left-3 right-3 top-3 z-[100] rounded-2xl border border-emerald-300/20 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-50 shadow-[0_12px_40px_rgba(0,0,0,0.35)] backdrop-blur md:left-6 md:right-auto md:w-[440px]">
        <div className="text-[10px] uppercase tracking-[0.28em] text-emerald-200/70">Deep Stake Status</div>
        <div className="mt-1 font-medium">앱은 실행 중입니다.</div>
        <div className="mt-1 text-xs text-emerald-50/90">{bootState}</div>
        <div className="mt-2 rounded-2xl border border-white/10 bg-black/20 px-3 py-2 text-[11px] text-emerald-100/90">현재 모드: {getBootModeMessage(bootMode)}</div>
      </div>

      <FullscreenGameHost
        title={fifthDawnMobileShell.appName}
        subtitle={subtitle}
        sidebar={
          <div className="space-y-4 text-sm text-slate-200">
            <div>
              <div className="text-xs uppercase tracking-[0.25em] text-amber-200/70">Lore</div>
              <h2 className="mt-2 text-lg font-semibold">Longest Dawn Slice</h2>
              <p className="mt-2 text-slate-300">
                Deep Stake는 무너진 마을과 복구 농장, 광산, 북부 개척지를 잇고 공명 경로를 여는 탑다운 라이프심 RPG입니다.
              </p>
            </div>

            <div className="rounded-2xl border border-white/10 bg-black/20 p-4">
              <div className="mb-2 flex items-center gap-2 font-medium">
                <CloudOff className="h-4 w-4 text-amber-300" />
                클라우드 상태
              </div>
              <p className="text-slate-300">{cloudNotice}</p>
              <div className="mt-3 text-xs text-slate-500">부팅 모드: {bootMode} / 제품 키: {fifthDawnMobileShell.productKey}</div>
            </div>

            <div className="rounded-2xl border border-white/10 bg-black/20 p-4">
              <div className="mb-2 flex items-center gap-2 font-medium">
                <Link2 className="h-4 w-4 text-emerald-300" />
                Deep Stake Link
              </div>
              <p className="text-slate-300">건강 앱에서 만든 파생 지표만 가져옵니다. 원본 건강 데이터는 게임으로 직접 들어오지 않습니다.</p>
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
                    const nextGameAccountId = gameAccountIdInput.trim();
                    const nextToken = tokenInput.trim();
                    setStoredGameAccountId(nextGameAccountId);
                    setStoredGameLinkToken(nextToken);
                    await refreshBundle(nextToken, nextGameAccountId);
                    setBootState("게임 링크 정보를 갱신했습니다.");
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
                    setBootState("게임 링크 정보를 해제했습니다.");
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
                    : "현재는 로컬 전용 플레이 상태입니다."}
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
                <div className="text-slate-400">클라우드 링크가 없어도 게임은 그대로 플레이할 수 있습니다.</div>
              )}
              <div className="mt-3 text-xs text-slate-500">모바일 설정: {fifthDawnMobileShell.preferredOrientation} / {fifthDawnMobileShell.productKey}</div>
            </div>
          </div>
        }
      >
        <GameRuntimeBoundary>
          <LifeSimArena />
        </GameRuntimeBoundary>
      </FullscreenGameHost>
    </>
  );
}

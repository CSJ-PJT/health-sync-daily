import { useEffect, useState } from "react";
import { Link2, RefreshCw, ShieldCheck, Unplug } from "lucide-react";
import { Header } from "@/components/Header";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useToast } from "@/hooks/use-toast";
import { supabase } from "@/integrations/supabase/client";
import { getStoredProfileId, getStoredUserId } from "@health-sync/shared-auth";
import type { GameLinkBundle } from "@health-sync/shared-types";
import {
  connectGameAccount,
  disconnectGameAccount,
  getHealthSideGameLinkBundle,
  refreshGameLinkBundle,
} from "@health-sync/game-link-sdk";

export default function GameLink() {
  const { toast } = useToast();
  const [gameAccountId, setGameAccountId] = useState("");
  const [bundle, setBundle] = useState<GameLinkBundle | null>(null);
  const [loading, setLoading] = useState(true);
  const profileId = getStoredProfileId();
  const userId = getStoredUserId();

  const reload = async () => {
    if (!profileId) {
      setBundle(null);
      setLoading(false);
      return;
    }

    setLoading(true);
    try {
      const next = await getHealthSideGameLinkBundle(supabase as never, profileId, userId);
      setBundle(next);
      if (next?.accountLink?.gameAccountId) {
        setGameAccountId(next.accountLink.gameAccountId);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void reload();
  }, [profileId]);

  const handleConnect = async () => {
    if (!profileId || !gameAccountId.trim()) return;
    await connectGameAccount(supabase as never, profileId, userId, gameAccountId.trim());
    toast({
      title: "게임 계정을 연결했습니다",
      description: "건강 데이터에서 파생된 게임 전용 지표만 안전하게 동기화합니다.",
    });
    await reload();
  };

  const handleDisconnect = async () => {
    if (!profileId) return;
    await disconnectGameAccount(supabase as never, profileId, userId);
    toast({
      title: "게임 계정 연결을 해제했습니다",
      description: "이후에는 새로운 게임 보너스와 미션이 전송되지 않습니다.",
    });
    await reload();
  };

  const handleRefresh = async () => {
    if (!profileId) return;
    await refreshGameLinkBundle(supabase as never, profileId, userId);
    toast({
      title: "파생 게임 지표를 갱신했습니다",
      description: "원본 건강 기록이 아니라 게임 전용 파생 지표만 다시 계산했습니다.",
    });
    await reload();
  };

  return (
    <div className="min-h-screen bg-background">
      <Header showNav />
      <div className="mx-auto max-w-5xl space-y-6 p-4">
        <div className="space-y-2">
          <h1 className="text-3xl font-bold">Game Link</h1>
          <p className="text-sm text-muted-foreground">
            독립 게임 앱에는 원본 건강 데이터가 전달되지 않습니다. 걸음, 수면, 회복, 수분 같은
            항목은 게임 전용 파생 지표로 변환된 뒤에만 공유됩니다.
          </p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Link2 className="h-5 w-5 text-primary" />
              연결 상태
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-3 md:grid-cols-[1fr_auto_auto]">
              <Input
                value={gameAccountId}
                onChange={(event) => setGameAccountId(event.target.value)}
                placeholder="게임 계정 ID"
              />
              <Button className="gap-2" onClick={handleConnect} disabled={!profileId}>
                <ShieldCheck className="h-4 w-4" />
                연결
              </Button>
              <Button variant="outline" className="gap-2" onClick={handleDisconnect} disabled={!bundle?.accountLink}>
                <Unplug className="h-4 w-4" />
                해제
              </Button>
            </div>
            <div className="rounded-2xl border bg-muted/20 p-4 text-sm">
              {loading
                ? "연결 정보를 불러오는 중입니다."
                : bundle?.accountLink
                  ? `현재 상태: ${bundle.accountLink.linkStatus} · 게임 계정 ${bundle.accountLink.gameAccountId}`
                  : "아직 연결된 게임 계정이 없습니다."}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <RefreshCw className="h-5 w-5 text-primary" />
              파생 게임 지표
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Button variant="outline" onClick={handleRefresh} disabled={!profileId}>
              지금 갱신
            </Button>
            {bundle?.profile ? (
              <div className="grid gap-3 md:grid-cols-2">
                <div className="rounded-2xl border p-4 text-sm">활동 등급: {bundle.profile.activityTier}</div>
                <div className="rounded-2xl border p-4 text-sm">수면 등급: {bundle.profile.sleepTier}</div>
                <div className="rounded-2xl border p-4 text-sm">회복 등급: {bundle.profile.recoveryTier}</div>
                <div className="rounded-2xl border p-4 text-sm">수분 등급: {bundle.profile.hydrationTier}</div>
                <div className="rounded-2xl border p-4 text-sm">주간 움직임 점수: {bundle.profile.weeklyMovementScore}</div>
                <div className="rounded-2xl border p-4 text-sm">공명 포인트: {bundle.profile.resonancePoints}</div>
              </div>
            ) : (
              <div className="rounded-2xl border border-dashed p-6 text-sm text-muted-foreground">
                아직 계산된 게임 링크 지표가 없습니다.
              </div>
            )}
          </CardContent>
        </Card>

        <div className="grid gap-4 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle>연동 미션</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {bundle?.missions?.length ? (
                bundle.missions.map((mission) => (
                  <div key={mission.id} className="rounded-xl border p-3 text-sm">
                    <div className="font-medium">{mission.title}</div>
                    <div className="mt-1 text-muted-foreground">{mission.description}</div>
                    <div className="mt-2 text-xs text-primary">
                      {mission.missionScope} · {mission.status}
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-sm text-muted-foreground">현재 노출된 미션이 없습니다.</div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>보상 동기화</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {bundle?.rewards?.length ? (
                bundle.rewards.map((reward) => (
                  <div key={reward.id} className="rounded-xl border p-3 text-sm">
                    <div className="font-medium">{reward.rewardKey}</div>
                    <div className="mt-1 text-muted-foreground">{reward.rewardType}</div>
                    <div className="mt-2 text-xs text-primary">
                      {reward.claimedAt ? "수령 완료" : "게임 앱에서 대기 중"}
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-sm text-muted-foreground">아직 연동된 보상이 없습니다.</div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

import { useEffect, useMemo, useRef, useState } from "react";
import { BedDouble, Droplets, Hammer, Pickaxe, Save, Sprout, Wheat } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";
import { lifeSimItems } from "@/game/life-sim/data/items";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import { renderLifeSimScene } from "@/game/life-sim/engine/renderLifeSimScene";
import { resolveInputAction } from "@/game/life-sim/engine/inputBindings";
import {
  advanceClock,
  cycleHazards,
  interactInWorld,
  movePlayer,
  selectHotbarIndex,
  sleepUntilNextDay,
  useSelectedHotbarItem,
  useToolAction,
} from "@/game/life-sim/state/lifeSimState";
import type { LifeSimFacing, LifeSimState } from "@/game/life-sim/types";
import { useToast } from "@/hooks/use-toast";
import { lifeSimRepository } from "@/services/repositories/lifeSimRepository";

type Props = {
  onExit: () => void;
};

function formatClock(minutes: number) {
  const hour = Math.floor(minutes / 60);
  const minute = minutes % 60;
  return `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
}

function getHotbarLabel(itemId: keyof typeof lifeSimItems, locale: "ko" | "en") {
  return t(lifeSimItems[itemId].name, locale);
}

export function LifeSimArena({ onExit }: Props) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [state, setState] = useState<LifeSimState | null>(null);
  const [saving, setSaving] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const locale = getLifeSimLocale();
  const { toast } = useToast();

  useEffect(() => {
    let cancelled = false;

    async function load() {
      const next = await lifeSimRepository.load("main");
      if (cancelled) return;
      setState(next);
      setLoaded(true);
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    if (!state || !canvasRef.current) return;
    renderLifeSimScene(canvasRef.current, state);
  }, [state]);

  useEffect(() => {
    if (!state) return;
    const timer = window.setInterval(() => {
      setState((current) => (current ? cycleHazards(advanceClock(current, 10)) : current));
    }, 2500);
    return () => window.clearInterval(timer);
  }, [state?.time.day]);

  useEffect(() => {
    if (!loaded || !state) return;
    const timer = window.setTimeout(() => {
      void lifeSimRepository.save(state, state.slot);
    }, 600);
    return () => window.clearTimeout(timer);
  }, [loaded, state]);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (!state) return;
      const action = resolveInputAction(event.key, state.settings);
      if (!action) return;
      event.preventDefault();

      setState((current) => {
        if (!current) return current;

        switch (action) {
          case "move-up":
            return movePlayer(current, "up");
          case "move-down":
            return movePlayer(current, "down");
          case "move-left":
            return movePlayer(current, "left");
          case "move-right":
            return movePlayer(current, "right");
          case "interact":
            return interactInWorld(current).state;
          case "use-tool":
            return useToolAction(current).state;
          case "sleep":
            return sleepUntilNextDay(current);
          case "hotbar-1":
            return selectHotbarIndex(current, 0);
          case "hotbar-2":
            return selectHotbarIndex(current, 1);
          case "hotbar-3":
            return selectHotbarIndex(current, 2);
          case "hotbar-4":
            return selectHotbarIndex(current, 3);
          case "hotbar-5":
            return selectHotbarIndex(current, 4);
          default:
            return current;
        }
      });
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [state]);

  const saveNow = async () => {
    if (!state) return;
    setSaving(true);
    await lifeSimRepository.save(state, state.slot);
    setSaving(false);
    toast({ title: "세이브 완료", description: "현재 농장과 마을 상태를 저장했습니다." });
  };

  const applyMovement = (facing: LifeSimFacing) => {
    setState((current) => (current ? movePlayer(current, facing) : current));
  };

  const applyInteract = () => {
    setState((current) => (current ? interactInWorld(current).state : current));
  };

  const applyTool = () => {
    setState((current) => (current ? useToolAction(current).state : current));
  };

  const applySleep = () => {
    setState((current) => (current ? sleepUntilNextDay(current) : current));
  };

  const selectedItem = useMemo(() => (state ? useSelectedHotbarItem(state) : "hoe"), [state]);

  if (!state) {
    return <div className="flex h-full items-center justify-center text-sm text-white/80">라이프 심 월드를 불러오는 중입니다.</div>;
  }

  return (
    <div className="grid h-full min-h-0 gap-4 p-4 lg:grid-cols-[minmax(280px,340px)_1fr_minmax(320px,380px)]">
      <div className="min-h-0 space-y-4 overflow-y-auto">
        <Card className="border-white/10 bg-white/5 text-white">
          <CardHeader>
            <CardTitle>플레이어 상태</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div>맵: {state.player.mapId === "farm" ? "복원 농장" : state.player.mapId === "village" ? "새벽 광장" : "정화 광맥"}</div>
            <div>시간: {state.time.day}일차 · {formatClock(state.time.minutes)}</div>
            <div>기력: {state.player.energy} / {state.player.maxEnergy}</div>
            <div>
              건강 보너스: 시작 +{state.healthBonuses.startEnergyBonus} · 회복 +{state.healthBonuses.recoveryBonus} · 작물 효율 +{state.healthBonuses.cropEfficiencyBonus}
            </div>
          </CardContent>
        </Card>

        <Card className="border-white/10 bg-white/5 text-white">
          <CardHeader>
            <CardTitle>핫바</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-5 gap-2">
            {state.player.hotbar.map((itemId, index) => (
              <button
                key={`${itemId}-${index}`}
                type="button"
                onClick={() => setState((current) => (current ? selectHotbarIndex(current, index) : current))}
                className={`rounded-xl border p-2 text-center text-xs transition ${
                  state.player.selectedHotbarIndex === index ? "border-emerald-300 bg-emerald-400/20" : "border-white/10 bg-black/10"
                }`}
              >
                <div className="mb-1 flex justify-center">
                  {itemId === "hoe" ? <Hammer className="h-4 w-4" /> : null}
                  {itemId === "watering-can" ? <Droplets className="h-4 w-4" /> : null}
                  {itemId === "pickaxe" ? <Pickaxe className="h-4 w-4" /> : null}
                  {itemId === "turnip-seeds" ? <Sprout className="h-4 w-4" /> : null}
                  {itemId === "turnip" ? <Wheat className="h-4 w-4" /> : null}
                </div>
                <div>{getHotbarLabel(itemId, locale)}</div>
                <div className="text-[10px] text-white/65">{state.player.inventory[itemId] || 0}</div>
              </button>
            ))}
          </CardContent>
        </Card>

        <Card className="border-white/10 bg-white/5 text-white">
          <CardHeader>
            <CardTitle>인벤토리</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            {Object.entries(state.player.inventory).map(([itemId, amount]) => (
              <div key={itemId} className="flex items-center justify-between rounded-xl border border-white/10 bg-black/10 px-3 py-2">
                <span>{getHotbarLabel(itemId as keyof typeof lifeSimItems, locale)}</span>
                <span>{amount}</span>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>

      <div className="min-h-0 rounded-3xl border border-white/10 bg-black/25 p-3 shadow-2xl">
        <div className="flex h-full flex-col gap-3">
          <div className="overflow-auto rounded-2xl border border-white/10 bg-black/20 p-2">
            <canvas ref={canvasRef} className="mx-auto max-w-full rounded-xl bg-black/30" />
          </div>
          <div className="grid gap-3 md:grid-cols-[1fr_auto]">
            <div className="grid grid-cols-3 gap-2">
              <div />
              <Button variant="secondary" onClick={() => applyMovement("up")}>위</Button>
              <div />
              <Button variant="secondary" onClick={() => applyMovement("left")}>왼쪽</Button>
              <Button variant="secondary" onClick={() => applyMovement("down")}>아래</Button>
              <Button variant="secondary" onClick={() => applyMovement("right")}>오른쪽</Button>
            </div>
            <div className="grid grid-cols-2 gap-2">
              <Button className="gap-2" onClick={applyTool}>
                <Hammer className="h-4 w-4" />
                사용
              </Button>
              <Button variant="secondary" onClick={applyInteract}>대화/수확</Button>
              <Button variant="secondary" className="gap-2" onClick={applySleep}>
                <BedDouble className="h-4 w-4" />
                취침
              </Button>
              <Button variant="secondary" className="gap-2" disabled={saving} onClick={saveNow}>
                <Save className="h-4 w-4" />
                저장
              </Button>
            </div>
          </div>
        </div>
      </div>

      <div className="min-h-0 space-y-4 overflow-y-auto">
        <Card className="border-white/10 bg-white/5 text-white">
          <CardHeader>
            <CardTitle>현재 선택</CardTitle>
          </CardHeader>
          <CardContent className="text-sm">
            <div>선택 도구: {getHotbarLabel(selectedItem, locale)}</div>
            <div className="mt-2 text-white/70">농장에서는 괭이 → 씨앗 → 물 주기 순서로 작물을 기를 수 있습니다.</div>
          </CardContent>
        </Card>

        <Card className="border-white/10 bg-white/5 text-white">
          <CardHeader>
            <CardTitle>대화와 이야기</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3 text-sm">
            {state.lastDialogue ? (
              <div className="rounded-2xl border border-emerald-300/20 bg-emerald-500/10 p-3">
                <div className="font-medium">{state.lastDialogue.npcId === "archivist" ? "기록관리인 아리아" : "정비사 도윤"}</div>
                {state.lastDialogue.lines.map((line, index) => (
                  <div key={`${state.lastDialogue.npcId}-${index}`} className="mt-1 text-white/80">
                    {line}
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-white/65">마을 광장에서 NPC와 대화해 보세요.</div>
            )}
          </CardContent>
        </Card>

        <Card className="border-white/10 bg-white/5 text-white">
          <CardHeader>
            <CardTitle>최근 메시지</CardTitle>
          </CardHeader>
          <CardContent>
            <ScrollArea className="h-56 pr-4">
              <div className="space-y-3">
                {state.messageLog.map((message) => (
                  <div key={message.id} className="rounded-2xl border border-white/10 bg-black/10 p-3">
                    <div className="font-medium">{message.title}</div>
                    <div className="mt-1 text-sm text-white/75">{message.body}</div>
                  </div>
                ))}
              </div>
            </ScrollArea>
          </CardContent>
        </Card>

        <div className="flex gap-2">
          <Button variant="outline" className="border-white/15 bg-white/5 text-white hover:bg-white/10" onClick={saveNow}>
            세이브
          </Button>
          <Button variant="outline" className="border-white/15 bg-white/5 text-white hover:bg-white/10" onClick={onExit}>
            엔터테인먼트로 돌아가기
          </Button>
        </div>
      </div>
    </div>
  );
}

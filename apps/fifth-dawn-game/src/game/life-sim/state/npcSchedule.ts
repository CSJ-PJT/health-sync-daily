import { lifeSimNpcs } from "@/game/life-sim/data/npcs";
import type {
  LifeSimDayPeriod,
  LifeSimMapId,
  LifeSimNpcDefinition,
  LifeSimNpcId,
  LifeSimNpcScheduleStop,
  LifeSimState,
} from "@/game/life-sim/types";

export function getLifeSimDayPeriod(minutes: number): LifeSimDayPeriod {
  if (minutes < 12 * 60) return "morning";
  if (minutes < 18 * 60) return "afternoon";
  return "evening";
}

export function getNpcScheduleStop(state: LifeSimState, npcId: LifeSimNpcId): LifeSimNpcScheduleStop {
  const npc = lifeSimNpcs.find((entry) => entry.id === npcId);
  if (!npc) {
    return {
      period: getLifeSimDayPeriod(state.time.minutes),
      mapId: "village",
      x: 0,
      y: 0,
      hint: { ko: "일정이 없습니다.", en: "No schedule." },
    };
  }
  const period = getLifeSimDayPeriod(state.time.minutes);
  return npc.schedule.find((stop) => stop.period === period) || npc.schedule[0];
}

export function getScheduledNpcsForMap(state: LifeSimState, mapId: LifeSimMapId) {
  return lifeSimNpcs
    .map((npc) => ({
      npc,
      stop: getNpcScheduleStop(state, npc.id),
    }))
    .filter((entry) => entry.stop.mapId === mapId);
}

export function getNpcAtPosition(state: LifeSimState, mapId: LifeSimMapId, x: number, y: number): LifeSimNpcDefinition | null {
  const match = getScheduledNpcsForMap(state, mapId).find((entry) => entry.stop.x === x && entry.stop.y === y);
  return match?.npc || null;
}

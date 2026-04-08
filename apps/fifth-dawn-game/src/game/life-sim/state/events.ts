import type { LifeSimEventName, LifeSimGameplayEvent } from "@/game/life-sim/types";

export type LifeSimEventSink = {
  emit: (name: LifeSimEventName, payload: Record<string, unknown>) => LifeSimGameplayEvent;
};

export function createLifeSimEventSink(existing: LifeSimGameplayEvent[]): LifeSimEventSink {
  return {
    emit(name, payload) {
      const event: LifeSimGameplayEvent = {
        id: `life-sim-event-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
        name,
        payload,
        createdAt: new Date().toISOString(),
      };
      existing.push(event);
      return event;
    },
  };
}

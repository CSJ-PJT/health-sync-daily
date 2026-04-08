import { lifeSimQuestDefinitions } from "@/game/life-sim/data/quests";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import type { LifeSimQuestId, LifeSimState, LocalizedText } from "@/game/life-sim/types";

function objectiveTextByQuest(state: LifeSimState, questId: LifeSimQuestId): LocalizedText {
  switch (questId) {
    case "first-harvest":
      return state.storyFlags.harvestedFirstCrop
        ? { ko: "첫 수확을 마쳤습니다.", en: "First harvest completed." }
        : { ko: "농장 밭을 갈고 씨앗을 심어 순무를 수확하세요.", en: "Till, plant, and harvest a turnip on the farm." };
    case "mine-recon":
      return state.storyFlags.enteredMine
        ? { ko: "광산 정찰을 마쳤습니다.", en: "Mine recon completed." }
        : { ko: "마을 북동쪽 통로를 따라 광산 유적 안으로 들어가 보세요.", en: "Enter the mine ruins through the eastern route." };
    case "repair-lantern":
      return state.storyFlags.repairedLantern
        ? { ko: "정화 등불 복구를 마쳤습니다.", en: "Purity lantern repaired." }
        : { ko: "광산 자원을 모아 정화 등불을 만들고 정비공에게 가져가세요.", en: "Craft a purity lantern and bring it to the mechanic." };
    case "restore-bridge":
      return state.storyFlags.restoredBridge
        ? { ko: "북쪽 다리 복구를 마쳤습니다.", en: "The northern bridge is restored." }
        : { ko: "복구 키트를 제작해 농장 북쪽 다리 표식에서 사용하세요.", en: "Craft a repair kit and use it at the northern bridge marker." };
  }
}

export function getActiveQuest(state: LifeSimState) {
  return state.quests.find((quest) => quest.status !== "completed") || null;
}

export function getQuestObjectiveText(state: LifeSimState, questId: LifeSimQuestId) {
  return t(objectiveTextByQuest(state, questId), getLifeSimLocale());
}

export function getActiveQuestSummary(state: LifeSimState) {
  const active = getActiveQuest(state);
  if (!active) {
    return {
      title: t({ ko: "현재 퀘스트 모두 완료", en: "Current quests complete" }),
      objective: t({
        ko: "다음 갱신에서 이어질 긴 새벽의 다음 장을 기다리세요.",
        en: "Wait for the next chapter of the Longest Dawn.",
      }),
    };
  }

  const definition = lifeSimQuestDefinitions[active.id];
  return {
    title: t(definition.title),
    objective: getQuestObjectiveText(state, active.id),
  };
}

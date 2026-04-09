import { lifeSimQuestDefinitions } from "@/game/life-sim/data/quests";
import { getLifeSimLocale, t } from "@/game/life-sim/data/localization";
import type { LifeSimQuestId, LifeSimState, LocalizedText } from "@/game/life-sim/types";

function objectiveTextByQuest(state: LifeSimState, questId: LifeSimQuestId): LocalizedText {
  switch (questId) {
    case "first-harvest":
      return state.storyFlags.harvestedFirstCrop
        ? { ko: "첫 수확을 마쳤습니다.", en: "First harvest completed." }
        : { ko: "농장 밭을 갈고 씨앗을 심어 새벽 순무를 수확하세요.", en: "Till, plant, and harvest a dawn turnip on the farm." };
    case "mine-recon":
      return state.storyFlags.enteredMine
        ? { ko: "광산 정찰을 마쳤습니다.", en: "Mine recon completed." }
        : { ko: "마을 동쪽 경로를 따라 정화 광산으로 들어가 보세요.", en: "Enter the Purifier Mine through the eastern route." };
    case "repair-lantern":
      return state.storyFlags.repairedLantern
        ? { ko: "정화 등불 복구를 마쳤습니다.", en: "Purity lantern restored." }
        : { ko: "광산 자원을 모아 정화 등불을 제작하고 정비공에게 가져가세요.", en: "Craft a purity lantern and bring it to the mechanic." };
    case "restore-bridge":
      return state.storyFlags.restoredBridge
        ? { ko: "북쪽 다리를 복구했습니다. 이제 개척지로 진입할 수 있습니다.", en: "The northern bridge is restored. You can now enter the Northern Reach." }
        : { ko: "복구 키트를 제작해 농장 북쪽 다리 표식 앞에서 사용하세요.", en: "Craft a repair kit and use it at the northern bridge marker." };
    case "north-reach":
      return state.storyFlags.surveyedNorthReach
        ? { ko: "북부 개척지 조사를 마쳤습니다.", en: "Northern Reach survey completed." }
        : { ko: "북부 개척지로 들어가 공명 파편을 채집하고 전초기지 후보지를 확인하세요.", en: "Enter the Northern Reach, mine a resonance shard, and inspect the outpost site." };
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
      title: t({ ko: "현재 퀘스트 완료", en: "Current quests complete" }),
      objective: t({
        ko: "다음 장이 열릴 때까지 정착지와 공명을 더 키워 두세요.",
        en: "Grow your settlement and resonance while waiting for the next chapter.",
      }),
    };
  }

  const definition = lifeSimQuestDefinitions[active.id];
  return {
    title: t(definition.title),
    objective: getQuestObjectiveText(state, active.id),
  };
}

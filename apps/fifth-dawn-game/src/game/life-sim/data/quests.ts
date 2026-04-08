import type { LifeSimQuestDefinition, LifeSimQuestId, LifeSimQuestState } from "@/game/life-sim/types";

export const lifeSimQuestDefinitions: Record<LifeSimQuestId, LifeSimQuestDefinition> = {
  "first-harvest": {
    id: "first-harvest",
    title: { ko: "첫 수확", en: "First Harvest" },
    description: { ko: "순무를 길러 첫 작물을 수확하세요.", en: "Grow and harvest your first crop." },
    rewardText: { ko: "새벽 수프 1개 + 공명 5", en: "1 Dawn Broth + 5 Resonance" },
  },
  "mine-recon": {
    id: "mine-recon",
    title: { ko: "광산 정찰", en: "Mine Recon" },
    description: {
      ko: "정화 광산으로 들어가 자원과 위험 구역을 확인하세요.",
      en: "Enter the purifier mine and recover resources.",
    },
    rewardText: { ko: "정화 등불 레시피 + 공명 10", en: "Purity Lantern recipe + 10 Resonance" },
  },
  "repair-lantern": {
    id: "repair-lantern",
    title: { ko: "등불 복구", en: "Restore the Lantern" },
    description: {
      ko: "정화 등불을 제작해 정비공에게 보여주세요.",
      en: "Craft a Purity Lantern and show it to the mechanic.",
    },
    rewardText: { ko: "공명 15 + 다리 복구 키트 레시피", en: "15 Resonance + Bridge Kit recipe" },
  },
  "restore-bridge": {
    id: "restore-bridge",
    title: { ko: "무너진 통로", en: "Broken Passage" },
    description: {
      ko: "복구 키트를 제작해 농장 북쪽 다리를 되살리세요.",
      en: "Craft a repair kit and restore the northern bridge.",
    },
    rewardText: { ko: "공명 20 + 다음 지역 단서", en: "20 Resonance + next region clue" },
  },
};

export const initialLifeSimQuests: LifeSimQuestState[] = Object.keys(lifeSimQuestDefinitions).map((id) => ({
  id: id as LifeSimQuestId,
  status: "available",
}));

import type { LifeSimQuestDefinition, LifeSimQuestId, LifeSimQuestState } from "@/game/life-sim/types";

export const lifeSimQuestDefinitions: Record<LifeSimQuestId, LifeSimQuestDefinition> = {
  "first-harvest": {
    id: "first-harvest",
    title: { ko: "첫 수확", en: "First Harvest" },
    description: { ko: "순무를 길러 첫 작물을 수확하세요.", en: "Raise and harvest your first crop." },
    rewardText: { ko: "새벽 수프 조리법", en: "Dawn Broth recipe" },
  },
  "mine-recon": {
    id: "mine-recon",
    title: { ko: "광산 정찰", en: "Mine Recon" },
    description: { ko: "정화 광산에 들어가 광석이나 고철을 확보하세요.", en: "Enter the purifier ruins and recover resources." },
    rewardText: { ko: "정화 등불 설계도", en: "Purity Lantern blueprint" },
  },
  "repair-lantern": {
    id: "repair-lantern",
    title: { ko: "등불 복구", en: "Restore the Lantern" },
    description: { ko: "정화 등불을 제작해 정비공에게 보여 주세요.", en: "Craft a Purity Lantern and bring it to the Mechanic." },
    rewardText: { ko: "관계도 상승", en: "Relationship boost" },
  },
  "restore-bridge": {
    id: "restore-bridge",
    title: { ko: "무너진 통로", en: "Broken Passage" },
    description: { ko: "복구 교량 키트를 만들어 외곽 통로를 복원하세요.", en: "Craft a bridge kit to restore the outer passage." },
    rewardText: { ko: "공명 점화", en: "Resonance unlock" },
  },
};

export const initialLifeSimQuests: LifeSimQuestState[] = Object.keys(lifeSimQuestDefinitions).map((id) => ({
  id: id as LifeSimQuestId,
  status: "available",
}));

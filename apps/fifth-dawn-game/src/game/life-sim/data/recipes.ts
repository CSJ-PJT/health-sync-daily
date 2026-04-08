import type { LifeSimRecipeDefinition, LifeSimRecipeId } from "@/game/life-sim/types";

export const lifeSimRecipes: Record<LifeSimRecipeId, LifeSimRecipeDefinition> = {
  "dawn-broth": {
    id: "dawn-broth",
    resultItemId: "dawn-broth",
    resultAmount: 1,
    ingredients: [
      { itemId: "turnip", amount: 2 },
    ],
    title: { ko: "새벽 수프", en: "Dawn Broth" },
    description: { ko: "순무 2개로 수프를 만들어 기력을 회복합니다.", en: "Cook turnips into a restorative broth." },
  },
  "purity-lantern": {
    id: "purity-lantern",
    resultItemId: "purity-lantern",
    resultAmount: 1,
    ingredients: [
      { itemId: "ore-fragment", amount: 2 },
      { itemId: "scrap-bundle", amount: 1 },
    ],
    title: { ko: "정화 등불", en: "Purity Lantern" },
    description: { ko: "광석과 고철로 오래된 정화 장치를 조립합니다.", en: "Assemble a purifier device from ore and scrap." },
  },
  "bridge-kit": {
    id: "bridge-kit",
    resultItemId: "bridge-kit",
    resultAmount: 1,
    ingredients: [
      { itemId: "scrap-bundle", amount: 2 },
      { itemId: "ore-fragment", amount: 1 },
    ],
    title: { ko: "복구 교량 키트", en: "Restoration Bridge Kit" },
    description: { ko: "무너진 통로를 복원하는 공학 키트를 제작합니다.", en: "Craft a kit used to restore damaged passages." },
  },
};

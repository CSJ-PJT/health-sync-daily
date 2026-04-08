import type { LifeSimRecipeDefinition, LifeSimRecipeId } from "@/game/life-sim/types";

export const lifeSimRecipes: Record<LifeSimRecipeId, LifeSimRecipeDefinition> = {
  "dawn-broth": {
    id: "dawn-broth",
    resultItemId: "dawn-broth",
    resultAmount: 1,
    ingredients: [{ itemId: "turnip", amount: 2 }],
    title: { ko: "새벽 수프", en: "Dawn Broth" },
    description: { ko: "순무 두 개를 끓여 기력을 회복하는 수프를 만듭니다.", en: "Cook turnips into a restorative broth." },
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
    description: {
      ko: "광산 자원으로 오래된 정화 장치를 다시 조립합니다.",
      en: "Assemble an old purifier device from mine resources.",
    },
  },
  "bridge-kit": {
    id: "bridge-kit",
    resultItemId: "bridge-kit",
    resultAmount: 1,
    ingredients: [
      { itemId: "scrap-bundle", amount: 2 },
      { itemId: "ore-fragment", amount: 1 },
    ],
    title: { ko: "다리 복구 키트", en: "Bridge Repair Kit" },
    description: {
      ko: "무너진 통로와 다리를 복구할 때 사용하는 공학 키트입니다.",
      en: "A repair kit used to restore damaged passages and bridges.",
    },
  },
};

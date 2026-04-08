import type { LifeSimItemDefinition, LifeSimItemId } from "@/game/life-sim/types";

export const lifeSimItems: Record<LifeSimItemId, LifeSimItemDefinition> = {
  hoe: {
    id: "hoe",
    name: { ko: "밭갈이 괭이", en: "Field Hoe" },
    description: { ko: "메마른 흙을 갈아 씨앗을 심을 수 있게 합니다.", en: "Turns dry soil into tillable land." },
    stackable: false,
    category: "tool",
  },
  "watering-can": {
    id: "watering-can",
    name: { ko: "물뿌리개", en: "Watering Can" },
    description: { ko: "씨앗과 작물이 하루 성장할 수 있도록 물을 줍니다.", en: "Waters crops for daily growth." },
    stackable: false,
    category: "tool",
  },
  pickaxe: {
    id: "pickaxe",
    name: { ko: "광채 곡괭이", en: "Gleam Pickaxe" },
    description: { ko: "광석과 고철 더미를 채굴할 수 있습니다.", en: "Breaks ore veins and scrap bundles." },
    stackable: false,
    category: "tool",
  },
  "turnip-seeds": {
    id: "turnip-seeds",
    name: { ko: "순무 씨앗", en: "Dawn Turnip Seeds" },
    description: { ko: "물을 주며 며칠 키우면 순무로 자랍니다.", en: "Grows into turnips after a few watered days." },
    stackable: true,
    category: "seed",
  },
  turnip: {
    id: "turnip",
    name: { ko: "새벽 순무", en: "Dawn Turnip" },
    description: { ko: "마을 사람에게 건네거나 보관할 수 있는 작물입니다.", en: "A humble crop to trade or keep." },
    stackable: true,
    category: "crop",
  },
  "ore-fragment": {
    id: "ore-fragment",
    name: { ko: "광석 파편", en: "Ore Fragment" },
    description: { ko: "광맥 깊은 곳에서 나온 오래된 광석 조각입니다.", en: "A shard mined from deep, old veins." },
    stackable: true,
    category: "resource",
  },
  "scrap-bundle": {
    id: "scrap-bundle",
    name: { ko: "정화 부품 묶음", en: "Purifier Scrap Bundle" },
    description: { ko: "정비사가 반길 만한 수리 재료입니다.", en: "Repair parts prized by the Mechanic." },
    stackable: true,
    category: "resource",
  },
  "purity-lantern": {
    id: "purity-lantern",
    name: { ko: "정화 등불", en: "Purity Lantern" },
    description: { ko: "오래된 기반 시설을 정화하는 상징적인 도구입니다.", en: "A symbolic device used to cleanse old infrastructure." },
    stackable: true,
    category: "artifact",
  },
};

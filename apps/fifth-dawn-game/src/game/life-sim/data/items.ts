import type { LifeSimItemDefinition, LifeSimItemId } from "@/game/life-sim/types";

export const lifeSimItems: Record<LifeSimItemId, LifeSimItemDefinition> = {
  hoe: {
    id: "hoe",
    name: { ko: "개척 괭이", en: "Field Hoe" },
    description: { ko: "메마른 흙을 갈아 씨앗을 심을 밭으로 바꿉니다.", en: "Turns dry soil into tillable land." },
    stackable: false,
    category: "tool",
  },
  "watering-can": {
    id: "watering-can",
    name: { ko: "물뿌리개", en: "Watering Can" },
    description: { ko: "씨앗과 작물에 물을 주어 다음 날 자라게 합니다.", en: "Waters crops for daily growth." },
    stackable: false,
    category: "tool",
  },
  pickaxe: {
    id: "pickaxe",
    name: { ko: "정화 곡괭이", en: "Purifier Pickaxe" },
    description: { ko: "광산과 개척지에서 광석과 잔해를 캘 수 있습니다.", en: "Breaks ore veins and salvage piles." },
    stackable: false,
    category: "tool",
  },
  "turnip-seeds": {
    id: "turnip-seeds",
    name: { ko: "새벽 순무 씨앗", en: "Dawn Turnip Seeds" },
    description: { ko: "며칠 동안 물을 주면 새벽 순무로 자라납니다.", en: "Grows into turnips after a few watered days." },
    stackable: true,
    category: "seed",
  },
  turnip: {
    id: "turnip",
    name: { ko: "새벽 순무", en: "Dawn Turnip" },
    description: { ko: "거래와 선물에 두루 쓰이는 기본 작물입니다.", en: "A humble crop to trade or gift." },
    stackable: true,
    category: "crop",
  },
  "ore-fragment": {
    id: "ore-fragment",
    name: { ko: "광석 파편", en: "Ore Fragment" },
    description: { ko: "정화 광산 깊은 층에서 캐낸 오래된 광맥 조각입니다.", en: "A shard mined from deep old veins." },
    stackable: true,
    category: "resource",
  },
  "scrap-bundle": {
    id: "scrap-bundle",
    name: { ko: "고철 묶음", en: "Scrap Bundle" },
    description: { ko: "정비공이 복구 재료로 아끼는 오래된 부품 꾸러미입니다.", en: "Repair parts prized by the Mechanic." },
    stackable: true,
    category: "resource",
  },
  "resonance-shard": {
    id: "resonance-shard",
    name: { ko: "공명 파편", en: "Resonance Shard" },
    description: {
      ko: "북부 개척지의 균열 지대에서 발견되는 공명 결정 조각입니다.",
      en: "A resonance crystal shard found in the Northern Reach.",
    },
    stackable: true,
    category: "resource",
  },
  "purity-lantern": {
    id: "purity-lantern",
    name: { ko: "정화 등불", en: "Purity Lantern" },
    description: {
      ko: "오래된 기반 시설을 안정화하고 정화 흐름을 되살리는 상징 장치입니다.",
      en: "A symbolic device used to cleanse old infrastructure.",
    },
    stackable: true,
    category: "artifact",
  },
  "dawn-broth": {
    id: "dawn-broth",
    name: { ko: "새벽 수프", en: "Dawn Broth" },
    description: { ko: "순무를 끓여 만든 회복식입니다. 기력을 조금 회복합니다.", en: "A restorative soup that recovers energy." },
    stackable: true,
    category: "consumable",
  },
  "bridge-kit": {
    id: "bridge-kit",
    name: { ko: "다리 복구 키트", en: "Bridge Repair Kit" },
    description: { ko: "무너진 통로를 복구할 때 사용하는 조립 키트입니다.", en: "A crafted kit used to restore damaged passages." },
    stackable: true,
    category: "crafted",
  },
};

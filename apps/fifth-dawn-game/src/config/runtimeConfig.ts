export type FifthDawnBootMode = "cloud" | "local" | "degraded";

export type FifthDawnRuntimeConfig = {
  supabaseUrl: string | null;
  supabasePublishableKey: string | null;
  cloudEnabled: boolean;
  bootMode: FifthDawnBootMode;
  statusTitle: string;
  statusMessage: string;
};

let cachedConfig: FifthDawnRuntimeConfig | null = null;
let didLogRuntimeConfig = false;

function normalize(value: unknown) {
  if (typeof value !== "string") return null;
  const trimmed = value.trim();
  return trimmed ? trimmed : null;
}

export function getRuntimeConfig(): FifthDawnRuntimeConfig {
  if (cachedConfig) {
    return cachedConfig;
  }

  const supabaseUrl = normalize(import.meta.env.VITE_SUPABASE_URL);
  const supabasePublishableKey = normalize(import.meta.env.VITE_SUPABASE_PUBLISHABLE_KEY);
  const cloudEnabled = Boolean(supabaseUrl && supabasePublishableKey);

  cachedConfig = cloudEnabled
    ? {
        supabaseUrl,
        supabasePublishableKey,
        cloudEnabled: true,
        bootMode: "cloud",
        statusTitle: "클라우드 모드",
        statusMessage: "파생 보너스와 클라우드 저장을 사용할 수 있습니다.",
      }
    : {
        supabaseUrl: null,
        supabasePublishableKey: null,
        cloudEnabled: false,
        bootMode: "local",
        statusTitle: "로컬 모드",
        statusMessage: "클라우드 설정이 없어 로컬 저장만 사용합니다.",
      };

  if (!didLogRuntimeConfig) {
    didLogRuntimeConfig = true;
    console.info(`[Deep Stake] Boot mode: ${cachedConfig.bootMode}${cloudEnabled ? " (cloud enabled)" : " (cloud disabled)"}`);
  }

  return cachedConfig;
}

export function getSafeSupabaseConfig() {
  const config = getRuntimeConfig();
  if (!config.cloudEnabled) return null;
  return {
    supabaseUrl: config.supabaseUrl!,
    supabasePublishableKey: config.supabasePublishableKey!,
  };
}

export function isCloudEnabled() {
  return getRuntimeConfig().cloudEnabled;
}

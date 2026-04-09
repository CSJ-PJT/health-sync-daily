import { createClient, type SupabaseClient } from "@supabase/supabase-js";
import { getRuntimeConfig, getSafeSupabaseConfig } from "@/config/runtimeConfig";

let cachedClient: SupabaseClient | null | undefined;

export function getSupabaseClient() {
  if (cachedClient !== undefined) {
    return cachedClient;
  }

  const safeConfig = getSafeSupabaseConfig();
  if (!safeConfig) {
    const runtimeConfig = getRuntimeConfig();
    console.info(`[Deep Stake] ${runtimeConfig.statusMessage}`);
    cachedClient = null;
    return cachedClient;
  }

  cachedClient = createClient(safeConfig.supabaseUrl, safeConfig.supabasePublishableKey, {
    auth: {
      storage: localStorage,
      persistSession: true,
      autoRefreshToken: true,
    },
  });

  return cachedClient;
}

export function hasSupabaseClient() {
  return Boolean(getSupabaseClient());
}

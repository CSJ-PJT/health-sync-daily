import type { GameAccountLink, GameLinkBundle, GameLinkMission, GameLinkProfile, GameLinkReward } from "@health-sync/shared-types";

type SupabaseLikeClient = {
  rpc: (fn: string, args?: Record<string, unknown>) => Promise<{ data: unknown; error: unknown }>;
};

export async function connectGameAccount(
  supabase: SupabaseLikeClient,
  profileId: string,
  userId: string,
  gameAccountId: string,
) {
  const { data, error } = await supabase.rpc("connect_game_account", {
    target_profile_id: profileId,
    target_user_id: userId,
    requested_game_account_id: gameAccountId,
  });

  if (error) {
    throw error;
  }

  return data as { linkToken: string; linkStatus: string };
}

export async function disconnectGameAccount(supabase: SupabaseLikeClient, profileId: string) {
  const { error } = await supabase.rpc("disconnect_game_account", {
    target_profile_id: profileId,
  });

  if (error) {
    throw error;
  }
}

export async function refreshGameLinkBundle(supabase: SupabaseLikeClient, profileId: string) {
  const { data, error } = await supabase.rpc("refresh_game_link_profile", {
    target_profile_id: profileId,
  });

  if (error) {
    throw error;
  }

  return data as GameLinkProfile;
}

export async function getHealthSideGameLinkBundle(supabase: SupabaseLikeClient, profileId: string) {
  const { data, error } = await supabase.rpc("fetch_health_game_link_bundle", {
    target_profile_id: profileId,
  });

  if (error) {
    throw error;
  }

  return data as {
    accountLink: GameAccountLink | null;
    profile: GameLinkProfile | null;
    missions: GameLinkMission[];
    rewards: GameLinkReward[];
  };
}

export async function getGameSideLinkBundle(supabase: SupabaseLikeClient, linkToken: string) {
  const { data, error } = await supabase.rpc("fetch_game_link_bundle", {
    supplied_link_token: linkToken,
  });

  if (error) {
    throw error;
  }

  return data as GameLinkBundle;
}

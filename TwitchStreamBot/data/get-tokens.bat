rem This file exists so that I can manage all the scopes and update as needed.
twitch token -u -s "bits:read channel:bot channel:edit:commercial channel:manage:ads channel:manage:broadcast channel:manage:polls channel:manage:predictions channel:manage:raids channel:manage:redemptions channel:manage:schedule channel:manage:videos channel:manage:vips channel:read:ads channel:read:editors channel:read:hype_train channel:read:polls channel:read:predictions channel:read:redemptions channel:read:subscriptions channel:read:vips chat:edit chat:read clips:edit moderation:read moderator:manage:announcements moderator:manage:automod moderator:manage:automod_settings moderator:manage:banned_users moderator:manage:blocked_terms moderator:manage:chat_messages moderator:manage:chat_settings moderator:manage:shield_mode moderator:manage:shoutouts moderator:manage:unban_requests moderator:manage:warnings moderator:read:automod_settings moderator:read:banned_users moderator:read:blocked_terms moderator:read:chat_messages moderator:read:chat_settings moderator:read:chatters moderator:read:followers moderator:read:moderators moderator:read:shield_mode moderator:read:shoutouts moderator:read:unban_requests moderator:read:vips moderator:read:warnings user:read:chat user:read:email whispers:read" > channel-token.txt 2>&1
twitch token -u -s "channel:moderate chat:edit chat:read moderator:manage:announcements moderator:manage:shoutouts moderator:manage:warnings user:bot user:read:chat user:write:chat whispers:read" > bot-token.txt 2>&1
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Humanizer;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;

namespace NeKzBot.Services.Notifications.Auditor
{
    public class AuditorNotificationService : NotificationService
    {
        private DiscordSocketClient _client;

        public AuditorNotificationService(IConfiguration config, LiteDatabase dataBase, DiscordSocketClient client)
            : base(config, dataBase)
        {
            _client = client;
        }

        public override Task Initialize()
        {
            _ = base.Initialize();

            _userName = "Auditor";
            _userAvatar = string.Empty;
            _sleepTime = 1 * 60 * 1000;
            _retryTime = 1 * 60 * 1000;

            var db = GetTaskCache<AuditData>()
                .GetAwaiter()
                .GetResult();

            var cache = db
                .FindAll();

            if (cache is null)
            {
                _ = LogWarning("Creating new cache");
                db.Insert(new AuditData());
            }
            return Task.CompletedTask;
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();
            await Task.Delay(3000, _cancellation!.Token);

        task_start:
            try
            {
                while (_isRunning)
                {
                    var watch = Stopwatch.StartNew();

                    var auditDb = await GetTaskCache<AuditData>();
                    var auditors = auditDb.FindAll();

                    var subDb = await GetSubscribers();
                    var subscribers = subDb
                        .FindAll()
                        .ToList();

                    _ = LogInfo($"{subscribers.Count} subs found");

                    if (subscribers.Count == 0) goto retry;

                    void DeleteSub(SubscriptionData sub, string message)
                    {
                        if (subDb.Delete(d => d.WebhookId == sub.WebhookId) != 1)
                            _ = LogWarning($"Tried to delete subscription for {sub.GuildId} but failed");
                        else
                            _ = LogWarning($"{message} for {sub.GuildId}. Ended service");
                    }

                    foreach (var sub in subscribers)
                    {
                        if (_client.ConnectionState != Discord.ConnectionState.Connected)
                            throw new Exception("Bot is not connected to Discord at the moment");

                        var guild = _client.GetGuild(sub.GuildId);
                        if (guild is null)
                        {
                            DeleteSub(sub, "Unable to find guild");
                            continue;
                        }

                        var audits = (await (guild as IGuild).GetAuditLogsAsync(11)).Where(audit =>
                        {
                            var user = guild.GetUser(audit.User.Id);
                            return (user != null) ? !user.IsBot && user.GuildPermissions.Has(GuildPermission.Administrator) : false;
                        });

                        _ = LogInfo($"count: {audits.Count()}, guild: {sub.GuildId}");

                        var auditor = auditors.FirstOrDefault(x => x.GuildId == sub.GuildId);
                        if (auditor is null)
                        {
                            _ = LogInfo($"Inserting new audits for guild {sub.GuildId}!");

                            auditDb.Insert(new AuditData()
                            {
                                GuildId = sub.GuildId,
                                AuditIds = audits.Select(x => x.Id),
                            });
                            continue;
                        }

                        var sending = audits.Where(audit => !auditor.AuditIds.Contains(audit.Id)).ToList();

                        _ = LogInfo($"sending: {sending.Count} guild: {sub.GuildId}");

                        if (sending.Count > 0)
                        {
                            _ = LogInfo($"Found {sending.Count} new notifications to send");

                            if (sending.Count > 25)
                                throw new Exception("Webhook rate limit exceeded!");

                            sending.Reverse();

                            var embeds = new List<Embed>();
                            foreach (var toSend in sending)
                            {
                                embeds.Add(await BuildEmbedAsync(toSend as RestAuditLogEntry, guild));
                            }

                            try
                            {
                                using var wc = new DiscordWebhookClient(sub.WebhookId, sub.WebhookToken);

                                await wc.SendMessageAsync
                                (
                                    string.Empty,
                                    embeds: embeds,
                                    username: _userName,
                                    avatarUrl: _userAvatar
                                );
                            }
                            catch (InvalidOperationException ex)
                                when (ex.Message.StartsWith("Could not find a webhook"))
                            {
                                DeleteSub(sub, "Unable to send hook");
                            }
                        }

                        auditor.AuditIds = auditor.AuditIds.Concat(audits.Select(x => x.Id));

                        if (!auditDb.Update(auditor))
                            _ = LogWarning("Failed to update cache");
                    }

                retry:
                    var delay = (int)(_sleepTime - watch.ElapsedMilliseconds);
                    if (delay < 0)
                        _ = LogWarning($"Task took too long: {delay}ms");

                    await Task.Delay(delay, _cancellation.Token);
                }
            }
            catch (Exception ex)
            {
                _ = LogException(ex);

                if (!(ex is TaskCanceledException))
                {
                    await StopAsync();
                    await base.StartAsync();
                    await Task.Delay((int)_retryTime);
                    goto task_start;
                }
            }

            _ = LogWarning("Task ended");
        }

        public override Task SendAsync(IEnumerable<object> notifications)
        {
            throw new InvalidOperationException("No.");
        }

        private Task<Embed> BuildEmbedAsync(RestAuditLogEntry? audit, SocketGuild guild)
        {
            if (audit is null)
                throw new NullReferenceException("Audit object was null");

            var changes = new List<string>();

            void AddPropChange(object source, string property)
            {
                object? GetPropValue(object? target, string propName)
                {
                    if (target is null)
                        throw new NullReferenceException("Target object is null");

                    var prop = target.GetType().GetProperty(propName);
                    if (prop is null)
                        throw new Exception($"Prop with name {propName} does not exist for this object!");

                    return prop.GetValue(target, null);
                }

                var before = GetPropValue(GetPropValue(source, "Before"), property);
                var after = GetPropValue(GetPropValue(source, "After"), property);
                if (before != after)
                    changes.Add($"{property}: {before ?? null} -> {after ?? null}");
            }
            void AddChannel(ulong id)
            {
                var channel = guild.GetChannel(id);
                changes.Add($"Channel: {(channel is SocketVoiceChannel ? channel.Name : $"<#{id}>")}");
            }
            void AddUser(ulong id)
            {
                changes.Add($"User: <@{id}>");
            }
            void AddRole(ulong id)
            {
                var role = guild.GetRole(id);
                if (role != null) changes.Add($"Role: <@&{id}>");
            }
            void AddEmote(ulong id)
            {
                var emote = guild.GetEmoteAsync(id).GetAwaiter().GetResult();
                if (emote != null) changes.Add($"Emote: {emote.Name} <:{emote.Name}:{emote.Id}>");
            }
            string ToAllowDenyList(IEnumerable<ChannelPermission> allow, IEnumerable<ChannelPermission> deny)
            {
                return (allow.Any() ? "Allow: " + string.Join(",", allow) : string.Empty)
                    + (deny.Any() ? "Deny: " + string.Join(",", deny) : string.Empty);
            }

            switch (audit.Data)
            {
                case GuildUpdateAuditLogData a:
                    if (a.Before.AfkChannelId != a.After.AfkChannelId)
                    {
                        var before = a.Before.AfkChannelId.HasValue ? guild.GetChannel(a.Before.AfkChannelId.Value)?.Name : "null";
                        var after = a.After.AfkChannelId.HasValue ? guild.GetChannel(a.After.AfkChannelId.Value)?.Name : "null";
                        changes.Add($"Afk Channel: {before} -> {after}");
                    }
                    AddPropChange(a, "AfkTimeout");
                    AddPropChange(a, "ExplicitContentFilter");
                    AddPropChange(a, "DefaultMessageNotifications");
                    AddPropChange(a, "IconHash");
                    AddPropChange(a, "MfaLevel");
                    AddPropChange(a, "Name");
                    AddPropChange(a, "Owner");
                    AddPropChange(a, "RegionId");
                    AddPropChange(a, "VerificationLevel");
                    AddPropChange(a, "ExplicitContentFilter");
                    if (a.Before.SystemChannelId != a.After.SystemChannelId)
                    {
                        var before = a.Before.SystemChannelId.HasValue ? guild.GetChannel(a.Before.SystemChannelId.Value)?.Name : "null";
                        var after = a.After.SystemChannelId.HasValue ? guild.GetChannel(a.After.SystemChannelId.Value)?.Name : "null";
                        changes.Add($"System Channel: {before} -> {after}");
                    }
                    if (a.Before.EmbedChannelId != a.After.EmbedChannelId)
                    {
                        var before = a.Before.EmbedChannelId.HasValue ? guild.GetChannel(a.Before.EmbedChannelId.Value)?.Name : "null";
                        var after = a.After.EmbedChannelId.HasValue ? guild.GetChannel(a.After.EmbedChannelId.Value)?.Name : "null";
                        changes.Add($"Embed Channel: {before} -> {after}");
                    }
                    AddPropChange(a, "IsEmbeddable");
                    break;
                case ChannelCreateAuditLogData a:
                    AddChannel(a.ChannelId);
                    changes.Add($"Type: {a.ChannelType}");
                    break;
                case ChannelUpdateAuditLogData a:
                    AddChannel(a.ChannelId);
                    AddPropChange(a, "Bitrate");
                    AddPropChange(a, "Name");
                    AddPropChange(a, "Topic");
                    AddPropChange(a, "SlowModeInterval");
                    AddPropChange(a, "IsNsfw");
                    break;
                case ChannelDeleteAuditLogData a:
                    changes.Add($"Channel: {a.ChannelName}");
                    changes.Add($"Type: {a.ChannelType}");
                    break;
                case OverwriteCreateAuditLogData a:
                    if (a.Overwrite.TargetType == PermissionTarget.Role)
                        AddRole(a.Overwrite.TargetId);
                    else if (a.Overwrite.TargetType == PermissionTarget.User)
                        AddUser(a.Overwrite.TargetId);
                    AddChannel(a.ChannelId);
                    changes.Add($"Permissions: {ToAllowDenyList(a.Overwrite.Permissions.ToAllowList(), a.Overwrite.Permissions.ToDenyList())}");
                    break;
                case OverwriteUpdateAuditLogData a:
                    if (a.OverwriteType == PermissionTarget.Role)
                        AddRole(a.OverwriteTargetId);
                    else if (a.OverwriteType == PermissionTarget.User)
                        AddUser(a.OverwriteTargetId);
                    AddChannel(a.ChannelId);
                    var allowOld = a.OldPermissions.ToAllowList();
                    var denyOld = a.OldPermissions.ToDenyList();
                    var allowNew = a.NewPermissions.ToAllowList();
                    var denyNew = a.NewPermissions.ToDenyList();
                    changes.Add($"Old Permissions: {ToAllowDenyList(allowOld.Where(x => !allowNew.Contains(x)), denyOld.Where(x => !denyNew.Contains(x)))}");
                    changes.Add($"New Permissions:  {ToAllowDenyList(allowNew.Where(x => !allowOld.Contains(x)), denyNew.Where(x => !denyOld.Contains(x)))}");
                    changes.Add($"Overwrite Type: {a.OverwriteType}");
                    break;
                case OverwriteDeleteAuditLogData a:
                    if (a.Overwrite.TargetType == PermissionTarget.Role)
                        AddRole(a.Overwrite.TargetId);
                    else if (a.Overwrite.TargetType == PermissionTarget.User)
                        AddUser(a.Overwrite.TargetId);
                    AddChannel(a.ChannelId);
                    changes.Add($"Permissions: {ToAllowDenyList(a.Overwrite.Permissions.ToAllowList(), a.Overwrite.Permissions.ToDenyList())}");
                    break;
                case KickAuditLogData a:
                    changes.Add($"User: <@{a.Target.Id}>");
                    break;
                case PruneAuditLogData a:
                    changes.Add($"Members Removed: {a.MembersRemoved}");
                    changes.Add($"Prune Days: {a.PruneDays}");
                    break;
                case BanAuditLogData a:
                    changes.Add($"User: <@{a.Target.Id}>");
                    break;
                case UnbanAuditLogData a:
                    changes.Add($"User: <@{a.Target.Id}>");
                    break;
                case MemberUpdateAuditLogData a:
                    changes.Add($"User: <@{a.Target.Id}>");
                    AddPropChange(a, "Deaf");
                    AddPropChange(a, "Mute");
                    AddPropChange(a, "Nickname");
                    break;
                case MemberRoleAuditLogData a:
                    changes.Add($"User: <@{a.Target.Id}>");
                    changes.Add($"Roles: {string.Join(",", a.Roles.Where(x => x.Added).Select(x => x.Name))}");
                    break;
                case RoleCreateAuditLogData a:
                    changes.Add($"Name: {a.Properties.Name}");
                    changes.Add($"Color: {a.Properties.Color}");
                    changes.Add($"Hoist: {a.Properties.Hoist}");
                    changes.Add($"Mentionable: {a.Properties.Mentionable}");
                    if (a.Properties.Permissions.HasValue)
                        changes.Add($"Permissions: {string.Join(", ", a.Properties.Permissions.Value.ToList())}");
                    break;
                case RoleUpdateAuditLogData a:
                    AddRole(a.RoleId);
                    AddPropChange(a, "Name");
                    AddPropChange(a, "Color");
                    AddPropChange(a, "Hoist");
                    AddPropChange(a, "Mentionable");
                    if (a.Before.Permissions.HasValue
                        && a.After.Permissions.HasValue
                        && a.Before.Permissions.Value.RawValue != a.After.Permissions.Value.RawValue)
                    {
                        var before = a.Before.Permissions.Value.ToList();
                        var after = a.After.Permissions.Value.ToList();
                        var removed = before.Where(p => !after.Contains(p));
                        var added = after.Where(p => !before.Contains(p));
                        if (removed.Any())
                            changes.Add($"Removed Perms: {string.Join(", ", removed)}");
                        if (added.Any())
                            changes.Add($"Added Perms: {string.Join(", ", added)}");
                    }
                    break;
                case RoleDeleteAuditLogData a:
                    changes.Add($"Name: {a.Properties.Name}");
                    break;
                case InviteCreateAuditLogData a:
                    AddChannel(a.ChannelId);
                    changes.Add($"Code: [{a.Code}](https://discord.gg/{a.Code})");
                    changes.Add($"Creator: <@{a.Creator.Id}>");
                    changes.Add($"Max Age: {a.MaxAge}");
                    changes.Add($"Max Uses: {a.MaxUses}");
                    changes.Add($"Temporary: {a.Temporary}");
                    break;
                case InviteUpdateAuditLogData a:
                    if (a.Before.ChannelId != a.After.ChannelId)
                    {
                        var before = a.Before.ChannelId.HasValue ? guild.GetChannel(a.Before.ChannelId.Value) : default;
                        var after = a.After.ChannelId.HasValue ? guild.GetChannel(a.After.ChannelId.Value) : default;
                        changes.Add($"Channel: {(before != default ? $"<#{before.Id}>" : "null")} -> {(after != default ? $"<#{after.Id}>" : "null")}");
                    }
                    AddPropChange(a, "Code");
                    AddPropChange(a, "MaxAge");
                    AddPropChange(a, "MaxUses");
                    AddPropChange(a, "Temporary");
                    break;
                case InviteDeleteAuditLogData a:
                    AddChannel(a.ChannelId);
                    changes.Add($"Code: [{a.Code}](https://discord.gg/{a.Code})");
                    changes.Add($"Creator: {a.Creator.Username}#{a.Creator.Discriminator}");
                    changes.Add($"Max Age: {a.MaxAge}");
                    changes.Add($"Max Uses: {a.MaxUses}");
                    changes.Add($"Temporary: {a.Temporary}");
                    changes.Add($"Uses: {a.Uses}");
                    break;
                case WebhookCreateAuditLogData a:
                    AddChannel(a.ChannelId);
                    changes.Add($"Name: {a.Name}");
                    break;
                case WebhookUpdateAuditLogData a:
                    changes.Add($"Webhook: {a.Webhook.Name}");
                    if (a.Before.ChannelId != a.After.ChannelId)
                    {
                        var before = a.Before.ChannelId.HasValue ? guild.GetChannel(a.Before.ChannelId.Value) : default;
                        var after = a.After.ChannelId.HasValue ? guild.GetChannel(a.After.ChannelId.Value) : default;
                        changes.Add($"Channel: {(before != default ? $"<#{before.Id}>" : "null")} -> {(after != default ? $"<#{after.Id}>" : "null")}");
                    }
                    AddPropChange(a, "Avatar");
                    AddPropChange(a, "Name");
                    break;
                case WebhookDeleteAuditLogData a:
                    changes.Add($"Webhook: {a.Name}");
                    AddChannel(a.ChannelId);
                    break;
                case EmoteCreateAuditLogData a:
                    AddEmote(a.EmoteId);
                    break;
                case EmoteUpdateAuditLogData a:
                    AddEmote(a.EmoteId);
                    changes.Add($"Name: {a.OldName} -> {a.NewName}");
                    break;
                case EmoteDeleteAuditLogData a:
                    changes.Add($"Name: {a.Name}");
                    break;
                case MessageDeleteAuditLogData a:
                    AddUser(a.AuthorId);
                    AddChannel(a.ChannelId);
                    changes.Add($"Message Count: {a.MessageCount}");
                    break;
            }

            if (!string.IsNullOrEmpty(audit.Reason))
                changes.Add($"Reason: {audit.Reason}");

            var embed = new EmbedBuilder
            {
                Title = audit.Action.ToString().Humanize(LetterCasing.Title),
                Description = string.Join("\n", changes),
                Color = new Color(229, 227, 87),
                Timestamp = DateTime.Now,
                Author = new EmbedAuthorBuilder()
                {
                    Name = audit.User.Username,
                    IconUrl = audit.User.GetAvatarUrl()
                }
            };

            return Task.FromResult(embed.Build());
        }
    }
}

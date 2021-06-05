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

            //_ = _dataBase.DropCollection("AuditNotificationService");
            //_ = _dataBase.DropCollection("AuditNotificationService_cache");

            //_ = DropTaskCache().GetAwaiter().GetResult();

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

                    foreach (var sub in subscribers)
                    {
                        if (_client.ConnectionState != Discord.ConnectionState.Connected)
                            throw new Exception("Bot is not connected to Discord at the moment");

                        try
                        {
                            //_ = LogInfo($"{sub.GuildId} guild");
                            var guild = _client.GetGuild(sub.GuildId);
                            if (guild is null)
                            {
                                sub.GuildErrors += 1;

                                if (sub.GuildErrors < 60)
                                {
                                    subDb.Update(sub);
                                    _ = LogWarning($"Unable to find guild. GuildErrors = {sub.GuildErrors}");
                                }
                                else
                                {
                                    if (subDb.Delete(d => d.WebhookId == sub.WebhookId) != 1)
                                        _ = LogWarning($"Tried to delete subscription for {sub.GuildId} but failed");
                                    else
                                        _ = LogWarning($"Unable to find guild. Ended subscription service for {sub.GuildId}.");
                                }

                                continue;
                            }

                            sub.GuildErrors = 0;
                            subDb.Update(sub);

                            var logs = await (guild as IGuild).GetAuditLogsAsync(11);
                            var audits = new List<IAuditLogEntry>();

                            foreach (var log in logs)
                            {
                                var user = guild.GetUser(log.User.Id) as IGuildUser;

                                if (user is null)
                                {
                                    //_ = LogWarning($"Unable to find user {log.User.Id}");
                                    continue;
                                }

                                var hasPerms = user.GuildPermissions.Has(GuildPermission.Administrator);
                                //_ = LogInfo($"user {user.Id} (bot: {user.IsBot}, admin: {hasPerms})");

                                if (user.IsBot == false && hasPerms == true)
                                {
                                    audits.Add(log);
                                }
                            }

                            _ = LogInfo($"count: {audits.Count}, guild: {sub.GuildId}");

                            if (audits.Count == 0)
                                continue;

                            var auditor = auditors.FirstOrDefault(x => x.GuildId == sub.GuildId);
                            if (auditor is null)
                            {
                                _ = LogInfo($"Inserting new audits for guild {sub.GuildId}!");

                                auditDb.Insert(new AuditData()
                                {
                                    GuildId = sub.GuildId,
                                    AuditIds = audits.Select(audit => audit.Id),
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
                                        avatarUrl: _userAvatar,
                                        allowedMentions: AllowedMentions.None
                                    );

                                    sub.WebhookErrors = 0;
                                    subDb.Update(sub);
                                }
                                catch (InvalidOperationException ex)
                                    when (ex.Message.StartsWith("Could not find a webhook"))
                                {
                                    if (sub.WebhookErrors < 60)
                                    {
                                        subDb.Update(sub);
                                        _ = LogWarning($"Unable to send hook. WebhookErrors = {sub.WebhookErrors}");
                                    }
                                    else
                                    {
                                        if (subDb.Delete(d => d.WebhookId == sub.WebhookId) != 1)
                                            _ = LogWarning($"Tried to delete subscription for {sub.GuildId} but failed");
                                        else
                                            _ = LogWarning($"Unable to send hook. Ended subscription service for {sub.GuildId}.");
                                    }
                                }
                            }

                            auditor.AuditIds = audits.Select(audit => audit.Id);

                            if (!auditDb.Update(auditor))
                                _ = LogWarning("Failed to update cache");
                        }
                        catch (Exception ex)
                        {
                            _ = LogException(ex);
                            continue;
                        }
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
            void AddMessage(ulong id, ulong channelId)
            {
                var channel = guild.GetChannel(channelId) as ITextChannel;
                if (channel is null) return;

                var message = default(IMessage);
                try {
                    message = channel.GetMessageAsync(id).GetAwaiter().GetResult();
                    if (message is null) return;
                } catch {
                    changes.Add("Unable to get original message :(");
                    return;
                }

                if (!string.IsNullOrEmpty(message.Content))
                    changes.Add($"Message: *{message.Content}*");
                changes.Add($"[Jump]({message.GetJumpUrl()})");
            }
            void AddEmote(ulong id)
            {
                var emote = guild.GetEmoteAsync(id).GetAwaiter().GetResult();
                if (emote != null) changes.Add($"Emote: {emote.Name} <:{emote.Name}:{emote.Id}>");
            }
            string ToAllowDenyListPerms(OverwritePermissions permissions)
            {
                var allow = permissions.ToAllowList();
                var deny = permissions.ToDenyList();
                return (allow.Any() ? "Allow -> " + string.Join(",", allow) : string.Empty)
                    + (deny.Any() ? "; Deny -> " + string.Join(",", deny) : string.Empty);
            }
            string ToAllowDenyList(IEnumerable<ChannelPermission> allow, IEnumerable<ChannelPermission> deny)
            {
                return (allow.Any() ? "Allow -> " + string.Join(",", allow) : string.Empty)
                    + (deny.Any() ? "; Deny -> " + string.Join(",", deny) : string.Empty);
            }
            Func<Overwrite, string> overwriteSelect = x =>
            {
                var result = string.Empty;
                if (x.TargetType == PermissionTarget.Role)
                {
                    var role = guild.GetRole(x.TargetId);
                    if (role != null) result += $"<@&{x.TargetId}>";
                }
                else if (x.TargetType == PermissionTarget.User)
                {
                    var user = guild.GetUser(x.TargetId);
                    if (user != null) result += $"<@{x.TargetId}>";
                }

                if (result == string.Empty)
                    result += x.TargetId.ToString();

                var perms = ToAllowDenyListPerms(x.Permissions);
                if (perms != string.Empty)
                    result += ": " + perms;

                return result;
            };

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
                    if (a.ChannelType == ChannelType.Voice)
                    {
                        changes.Add($"Channel: {a.ChannelName}");
                        changes.Add($"Type: {a.ChannelType}");
                        changes.Add($"Bitrate: {a.Bitrate}");
                        changes.Add($"SlowModeInterval: {a.SlowModeInterval}");
                    }
                    else if (a.ChannelType == ChannelType.Category)
                    {
                        changes.Add($"Channel: {a.ChannelName}");
                        changes.Add($"Type: {a.ChannelType}");
                    }
                    else if (a.ChannelType == ChannelType.Text || a.ChannelType == ChannelType.News)
                    {
                        AddChannel(a.ChannelId);
                        changes.Add($"Type: {a.ChannelType}");
                        changes.Add($"IsNsfw: {a.IsNsfw}");
                    }
                    if (a.Overwrites.Any())
                    {
                        var overwrites = a.Overwrites.Select(overwriteSelect);
                        changes.Add($"Overwrites: {string.Join("\n", overwrites)}");
                    }
                    break;
                case ChannelUpdateAuditLogData a:
                    AddChannel(a.ChannelId);
                    AddPropChange(a, "Bitrate");
                    AddPropChange(a, "Name");
                    AddPropChange(a, "Topic");
                    AddPropChange(a, "SlowModeInterval");
                    AddPropChange(a, "IsNsfw");
                    AddPropChange(a, "ChannelType");
                    break;
                case ChannelDeleteAuditLogData a:
                    if (a.ChannelType == ChannelType.Voice)
                    {
                        changes.Add($"Channel: {a.ChannelName}");
                        changes.Add($"Type: {a.ChannelType}");
                        changes.Add($"Bitrate: {a.Bitrate}");
                        changes.Add($"SlowModeInterval: {a.SlowModeInterval}");
                    }
                    else if (a.ChannelType == ChannelType.Category)
                    {
                        changes.Add($"Channel: {a.ChannelName}");
                        changes.Add($"Type: {a.ChannelType}");
                    }
                    else if (a.ChannelType == ChannelType.Text || a.ChannelType == ChannelType.News)
                    {
                        AddChannel(a.ChannelId);
                        changes.Add($"Type: {a.ChannelType}");
                        changes.Add($"IsNsfw: {a.IsNsfw}");
                    }
                    if (a.Overwrites.Any())
                    {
                        var overwrites = a.Overwrites.Select(overwriteSelect);
                        changes.Add($"Overwrites: {string.Join("\n", overwrites)}");
                    }
                    break;
                case OverwriteCreateAuditLogData a:
                    if (a.Overwrite.TargetType == PermissionTarget.Role)
                        AddRole(a.Overwrite.TargetId);
                    else if (a.Overwrite.TargetType == PermissionTarget.User)
                        AddUser(a.Overwrite.TargetId);
                    AddChannel(a.ChannelId);
                    var perms = ToAllowDenyListPerms(a.Overwrite.Permissions);
                    if (perms != string.Empty) changes.Add($"Permissions: {perms}");
                    break;
                case OverwriteUpdateAuditLogData a:
                    {
                        if (a.OverwriteType == PermissionTarget.Role)
                            AddRole(a.OverwriteTargetId);
                        else if (a.OverwriteType == PermissionTarget.User)
                            AddUser(a.OverwriteTargetId);
                        AddChannel(a.ChannelId);
                        var allowOld = a.OldPermissions.ToAllowList();
                        var denyOld = a.OldPermissions.ToDenyList();
                        var allowNew = a.NewPermissions.ToAllowList();
                        var denyNew = a.NewPermissions.ToDenyList();
                        var oldPerms = ToAllowDenyList(allowOld.Where(x => !allowNew.Contains(x)), denyOld.Where(x => !denyNew.Contains(x)));
                        var newPerms = ToAllowDenyList(allowNew.Where(x => !allowOld.Contains(x)), denyNew.Where(x => !denyOld.Contains(x)));
                        if (oldPerms != string.Empty) changes.Add($"Old Permissions: {oldPerms}");
                        if (newPerms != string.Empty) changes.Add($"New Permissions: {newPerms}");
                        changes.Add($"Overwrite Type: {a.OverwriteType}");
                    }
                    break;
                case OverwriteDeleteAuditLogData a:
                    if (a.Overwrite.TargetType == PermissionTarget.Role)
                        AddRole(a.Overwrite.TargetId);
                    else if (a.Overwrite.TargetType == PermissionTarget.User)
                        AddUser(a.Overwrite.TargetId);
                    AddChannel(a.ChannelId);
                    changes.Add($"Permissions: {ToAllowDenyListPerms(a.Overwrite.Permissions)}");
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
                    {
                        changes.Add($"User: <@{a.Target.Id}>");
                        var added = a.Roles.Where(x => x.Added);
                        var removed = a.Roles.Where(x => !x.Added);
                        if (added.Any())
                            changes.Add($"Added: {string.Join(",", added.Select(x => x.Name))}");
                        if (removed.Any())
                            changes.Add($"Removed: {string.Join(",", removed.Select(x => x.Name))}");
                    }
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
                    AddUser(a.Target.Id);
                    AddChannel(a.ChannelId);
                    changes.Add($"Message Count: {a.MessageCount}");
                    break;
                case BotAddAuditLogData a:
                    changes.Add($"Bot: <@{a.Target.Id}>");
                    break;
                case MemberMoveAuditLogData a:
                    AddChannel(a.ChannelId);
                    changes.Add($"Member Count: {a.MemberCount}");
                    break;
                case MemberDisconnectAuditLogData a:
                    changes.Add($"Member Count: {a.MemberCount}");
                    break;
                case MessageBulkDeleteAuditLogData a:
                    AddChannel(a.ChannelId);
                    changes.Add($"Member Count: {a.MessageCount}");
                    break;
                case MessagePinAuditLogData a:
                    AddChannel(a.ChannelId);
                    AddUser(a.Target.Id);
                    AddMessage(a.MessageId, a.ChannelId);
                    break;
                case MessageUnpinAuditLogData a:
                    AddChannel(a.ChannelId);
                    AddUser(a.Target.Id);
                    AddMessage(a.MessageId, a.ChannelId);
                    break;
            }

            if (!string.IsNullOrEmpty(audit.Reason))
                changes.Add($"Reason: {audit.Reason}");

            var embed = new EmbedBuilder()
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

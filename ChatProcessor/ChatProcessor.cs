﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using ChatProcessor.API;
using ChatProcessor.Utils;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Core.Attributes;

namespace ChatProcessor;

[MinimumApiVersion(253)]
public class ChatProcessor : BasePlugin
{
    public override string ModuleName => "ChatProcessor";
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "API for chat manipulation";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private ChatProcessorApi ChatProcessorApi = null!;

    internal static IStringLocalizer? Stringlocalizer;

    public override void Load(bool hotReload)
    {
        if (!CoreConfig.SilentChatTrigger.Any())
        {
            throw new Exception("CoreConfig.SilentChatTrigger is empty");
        }

        if (!CoreConfig.PublicChatTrigger.Any())
        {
            throw new Exception("CoreConfig.PublicChatTrigger is empty");
        }

        Stringlocalizer = Localizer;

        ChatProcessorApi = new ChatProcessorApi(this);
        Capabilities.RegisterPluginCapability(_pluginCapability, () => ChatProcessorApi);

        AddCommandListener("say", OnPlayerChat, HookMode.Pre);
        AddCommandListener("say_team", OnPlayerChat, HookMode.Pre);
    }

    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid)
        {
            return HookResult.Handled;
        }

        string command = commandInfo.GetArg(0);
        string message = commandInfo.GetArg(1);

        if (string.IsNullOrEmpty(message))
        {
            return HookResult.Handled;
        }

        message = ColorTags.Remove(message);

        if (string.IsNullOrEmpty(message))
        {
            return HookResult.Handled;
        }

        if (CoreConfig.SilentChatTrigger.Any(trigger => message.StartsWith(trigger)))
        {
            return HookResult.Continue;
        }

        foreach (var trigger in CoreConfig.PublicChatTrigger)
        {
            if (message.StartsWith(trigger))
            {
                string fakeMessage = message.Replace(trigger, CoreConfig.SilentChatTrigger.First());

                // Hack. Required for hidden command execution (say /{cmd} OR say_team /{cmd}).
                Server.NextFrame(() => Server.NextFrame(() => player.ExecuteClientCommandFromServer($"{command} \"{fakeMessage}\"")));

                break;
            }
        }

        string name = ColorTags.Remove(player.PlayerName);

        int flags = (int)ChatFlags.None;

        if (command.Contains("say_team"))
        {
            flags |= (int)ChatFlags.Team;
        }

        if (!player.PawnIsAlive)
        {
            flags |= (int)ChatFlags.Dead;
        }

        List<CCSPlayerController> recipients;

        IEnumerable<CCSPlayerController> playerEntities = Utilities.GetPlayers().Where(player => player is
        {
            IsValid: true,
            IsBot: false
        });

        if ((flags & (int)ChatFlags.Team) != 0)
        {
            recipients = [];

            foreach (var playerEntity in playerEntities)
            {
                if (playerEntity.TeamNum == player.TeamNum)
                {
                    recipients.Add(playerEntity);
                }
            }
        }
        else
        {
            recipients = playerEntities.ToList();
        }

        ChatProcessorApi.TriggerMessagePre(player, ref name, ref message, ref recipients, ref flags);

        name = $"{ColorTags.teamColorTag}{name}";

        Server.NextFrame(() => OnPlayerChatPost(player, name, message, recipients, flags));

        return HookResult.Handled;
    }

    private void OnPlayerChatPost(CCSPlayerController sender, string senderName, string message, List<CCSPlayerController> recipients, int flags)
    {
        if (recipients.Count == 0)
        {
            return;
        }

        string place = sender.PlayerPawn.Value != null ? sender.PlayerPawn.Value.LastPlaceName : string.Empty;
        bool withPlace = false;

        string formatMessage = LangKey.CHAT_ALL;
        if ((flags & (int)ChatFlags.Team) != 0)
        {
            switch (sender.Team)
            {
                case CsTeam.Spectator:
                    formatMessage = LangKey.CHAT_SPEC;
                    break;

                case CsTeam.Terrorist:
                    if ((flags & (int)ChatFlags.Dead) != 0)
                    {
                        formatMessage = LangKey.CHAT_T_DEAD;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(place))
                        {
                            formatMessage = LangKey.CHAT_T;
                        }
                        else
                        {
                            withPlace = true;
                            formatMessage = LangKey.CHAT_T_LOC;
                        }
                    }
                    break;

                case CsTeam.CounterTerrorist:
                    if ((flags & (int)ChatFlags.Dead) != 0)
                    {
                        formatMessage = LangKey.CHAT_CT_DEAD;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(place))
                        {
                            formatMessage = LangKey.CHAT_CT;
                        }
                        else
                        {
                            withPlace = true;
                            formatMessage = LangKey.CHAT_CT_LOC;
                        }
                    }
                    break;
            }
        }

        else if ((flags & (int)ChatFlags.Dead) != 0 && (sender.Team == CsTeam.Terrorist || sender.Team == CsTeam.CounterTerrorist))
        {
            formatMessage = LangKey.CHAT_ALL_DEAD;
        }

        foreach (CCSPlayerController recipient in recipients)
        {
            formatMessage = withPlace ? Localizer.ForPlayer(recipient, formatMessage, senderName, message, Localizer.ForPlayer(recipient, place)) : Localizer.ForPlayer(recipient, formatMessage, senderName, message);

            recipient.PrintToChat(ColorTags.Replace(formatMessage, recipient.Team));
            recipient.PrintToConsole(ColorTags.Remove(formatMessage));
        }

        ChatProcessorApi.TriggerMessagePost(sender, senderName, message, recipients, flags);
    }
}

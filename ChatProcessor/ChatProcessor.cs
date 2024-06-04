using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using ChatProcessor.API;
using ChatProcessor.Utils;

namespace ChatProcessor;

public class ChatProcessor : BasePlugin
{
    public override string ModuleName => "ChatProcessor";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "API for chat manipulation";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private readonly List<string> _silentChatTriggers = CoreConfig.SilentChatTrigger.ToList();
    private readonly List<string> _publicChatTriggers = CoreConfig.PublicChatTrigger.ToList();
    private bool _hasSilentChatTrigger;
    private bool _hasPublicChatTrigger;

    private ChatProcessorApi ChatProcessorApi = null!;

    public override void Load(bool hotReload)
    {
        ChatProcessorApi = new ChatProcessorApi(this);
        Capabilities.RegisterPluginCapability(_pluginCapability, () => ChatProcessorApi);

        AddCommandListener("say", OnPlayerChat);
        AddCommandListener("say_team", OnPlayerChat);

        _hasSilentChatTrigger = _silentChatTriggers.Count > 0;
        _hasPublicChatTrigger = _publicChatTriggers.Count > 0;
    }

    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo commandInfo)
    {
        string command = commandInfo.GetArg(0);
        string message = commandInfo.GetArg(1);

        if (string.IsNullOrEmpty(message))
        {
            return HookResult.Handled;
        }

        message = Tags.RemoveColorTags(message);

        if (string.IsNullOrEmpty(message))
        {
            return HookResult.Handled;
        }

        if (player == null || !player.IsValid)
        {
            return HookResult.Handled;
        }

        if (_hasSilentChatTrigger)
        {
            foreach (var trigger in _silentChatTriggers)
            {
                if (message.StartsWith(trigger))
                {
                    return HookResult.Continue;
                }
            }

            foreach (var trigger in _publicChatTriggers)
            {
                if (message.StartsWith(trigger))
                {
                    string fakeMessage = message.Replace(trigger, _silentChatTriggers.First());

                    // Hack.
                    Server.NextFrame(() => Server.NextFrame(() => player.ExecuteClientCommandFromServer($"{command} \"{fakeMessage}\"")));

                    break;
                }
            }
        }
        else if (_hasPublicChatTrigger)
        {
            foreach (var trigger in _publicChatTriggers)
            {
                if (message.StartsWith(trigger))
                {
                    string fakeMessage = message.Replace(trigger, string.Empty);

                    // Hack.
                    Server.NextFrame(() => Server.NextFrame(() => player.ExecuteClientCommandFromServer($"css_{fakeMessage}")));

                    break;
                }
            }
        }

        string name = Tags.RemoveColorTags(player.PlayerName);

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
            recipients = new List<CCSPlayerController>();

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

        name = $"{Tags.teamColorTag}{name}";

        Server.NextFrame(() => OnPlayerChatPost(player, name, message, recipients, flags));

        return HookResult.Handled;
    }

    private void OnPlayerChatPost(CCSPlayerController sender, string senderName, string message, List<CCSPlayerController> recipients, int flags)
    {
        string place = sender.PlayerPawn.Value != null ? sender.PlayerPawn.Value.LastPlaceName : string.Empty;
        bool withPlace = false;

        string formatMessage = "Cstrike_Chat_All";
        if ((flags & (int)ChatFlags.Team) != 0)
        {
            switch (sender.Team)
            {
                case CsTeam.Spectator:
                    formatMessage = "Cstrike_Chat_Spec";
                    break;

                case CsTeam.Terrorist:
                    if ((flags & (int)ChatFlags.Dead) != 0)
                    {
                        formatMessage = "Cstrike_Chat_T_Dead";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(place))
                        {
                            formatMessage = "Cstrike_Chat_T";
                        }
                        else
                        {
                            withPlace = true;
                            formatMessage = "Cstrike_Chat_T_Loc";
                        }
                    }
                    break;

                case CsTeam.CounterTerrorist:
                    if ((flags & (int)ChatFlags.Dead) != 0)
                    {
                        formatMessage = "Cstrike_Chat_CT_Dead";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(place))
                        {
                            formatMessage = "Cstrike_Chat_CT";
                        }
                        else
                        {
                            withPlace = true;
                            formatMessage = "Cstrike_Chat_CT_Loc";
                        }
                    }
                    break;
            }
        }

        else if ((flags & (int)ChatFlags.Dead) != 0 && (sender.Team == CsTeam.Terrorist || sender.Team == CsTeam.CounterTerrorist))
        {
            formatMessage = "Cstrike_Chat_AllDead";
        }

        foreach (CCSPlayerController recipient in recipients)
        {
            formatMessage = withPlace ? Localize(recipient, formatMessage, senderName, message, Localize(recipient, place)) : Localize(recipient, formatMessage, senderName, message);

            recipient.PrintToChat(Tags.ReplaceColorTags(formatMessage, recipient.Team));
            recipient.PrintToConsole(Tags.RemoveColorTags(formatMessage));
        }

        ChatProcessorApi.TriggerMessagePost(sender, senderName, message, recipients, flags);
    }

    public string Localize(CCSPlayerController player, string key)
    {
        if (player == null || player.IsValid == false)
            return string.Empty;

        using WithTemporaryCulture temporaryCulture = new WithTemporaryCulture(player.GetLanguage());
        return Localizer[key];
    }

    public string Localize(CCSPlayerController player, string key, params object[] args)
    {
        if (player == null || player.IsValid == false)
            return string.Empty;

        using WithTemporaryCulture temporaryCulture = new WithTemporaryCulture(player.GetLanguage());
        return Localizer[key, args];
    }
}
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using ChatProcessor.API;
using ChatProcessor.Utils;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Core.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace ChatProcessor;

[MinimumApiVersion(318)]
public class ChatProcessor : BasePlugin
{
    public override string ModuleName => "ChatProcessor";
    public override string ModuleVersion => "1.1.1";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "API for chat manipulation";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private ChatProcessorApi ChatProcessorApi = null!;

    internal static IStringLocalizer? Stringlocalizer;

    public override void Load(bool hotReload)
    {
        if (!CoreConfig.SilentChatTrigger.Any())
        {
            throw new ArgumentNullException(nameof(CoreConfig.SilentChatTrigger), "CoreConfig.SilentChatTrigger is empty");
        }

        if (!CoreConfig.PublicChatTrigger.Any())
        {
            throw new ArgumentNullException(nameof(CoreConfig.PublicChatTrigger), "CoreConfig.PublicChatTrigger is empty");
        }

        Stringlocalizer = Localizer;

        ChatProcessorApi = new ChatProcessorApi(this);
        Capabilities.RegisterPluginCapability(_pluginCapability, () => ChatProcessorApi);

        AddCommandListener("say", OnPlayerChat, HookMode.Pre);
        AddCommandListener("say_team", OnPlayerChat, HookMode.Pre);
    }

    /// <summary>
    /// Handles the player chat event.
    /// </summary>
    /// <param name="player">The player who sent the chat message.</param>
    /// <param name="commandInfo">The command information containing the chat message.</param>
    /// <returns>A HookResult indicating how the event was handled.</returns>
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

        // Remove color tags from the message
        message = ColorTags.Remove(message);

        if (string.IsNullOrEmpty(message))
        {
            return HookResult.Handled;
        }

        // Check if the message starts with any silent chat trigger
        if (CoreConfig.SilentChatTrigger.Any(trigger => message.StartsWith(trigger)))
        {
            return HookResult.Continue;
        }

        string? trigger = CoreConfig.PublicChatTrigger.FirstOrDefault(message.StartsWith);
        if (trigger != null)
        {
            string fakeMessage = message.Replace(trigger, CoreConfig.SilentChatTrigger.First());
            // Hack. Required for hidden command execution (say /{cmd} OR say_team /{cmd}).
            Server.NextFrame(() => player.ExecuteClientCommandFromServer($"{command} \"{fakeMessage}\""));
        }

        // Remove color tags from the player's name
        string name = ColorTags.Remove(player.PlayerName);

        ChatFlags flags = ChatFlags.None;

        if (command.Equals("say_team"))
        {
            flags |= ChatFlags.Team;
        }

        if (!player.PawnIsAlive)
        {
            flags |= ChatFlags.Dead;
        }

        // Mark as say_team
        bool isTeamChat = flags.HasFlag(ChatFlags.Team);

        // Get the list of recipients based on the team
        List<CCSPlayerController> recipients = GetRecipients(isTeamChat ? player.Team : CsTeam.None);

        // Trigger pre-message processing
        ChatProcessorApi.TriggerMessagePre(player, ref name, ref message, ref recipients, ref flags);

        // If there are no recipients, exit the method
        if (recipients.Count == 0)
        {
            return HookResult.Handled;
        }

        // Add team color tag to the player's name
        string senderName = $"{ColorTags.TeamColor}{name}";

        // Get the last known place name of the sender
        string place = player.PlayerPawn.Value?.LastPlaceName ?? string.Empty;

        bool isDead = flags.HasFlag(ChatFlags.Dead);
        bool hasPlace = !string.IsNullOrEmpty(place) && isTeamChat && !isDead && IsPlayerTeam(player.Team);

        string formatMessage = GetFormatMessage(player.Team, isTeamChat, isDead, hasPlace);

        // Send the formatted message to each recipient
        foreach (CCSPlayerController recipient in recipients)
        {
            string finalMessage = hasPlace 
                ? Localizer.ForPlayer(recipient, formatMessage, senderName, message, Localizer.ForPlayer(recipient, place)) 
                : Localizer.ForPlayer(recipient, formatMessage, senderName, message);

            recipient.PrintToChat(ColorTags.Replace(finalMessage, recipient.Team));
            recipient.PrintToConsole(ColorTags.Remove(finalMessage));
        }

        // Trigger post-message processing
        ChatProcessorApi.TriggerMessagePost(player, senderName, message, recipients, flags);

        return HookResult.Handled;
    }

    /// <summary>
    /// Determines the chat format message based on the team, chat type, and player status.
    /// </summary>
    /// <param name="team">The team of the player (e.g., Terrorist, CounterTerrorist, Spectator).</param>
    /// <param name="isTeamChat">Indicates if the chat is team-specific.</param>
    /// <param name="isDead">Indicates if the player is dead.</param>
    /// <param name="hasPlace">Indicates if the player has a specific location.</param>
    /// <returns>A string representing the chat format message.</returns>
    private string GetFormatMessage(CsTeam team, bool isTeamChat, bool isDead, bool hasPlace)
    {
        if (isTeamChat)
        {
            return team switch
            {
                CsTeam.Spectator => LangKey.Chat.Spec,
                CsTeam.Terrorist => isDead ? LangKey.Chat.TDead : (hasPlace ? LangKey.Chat.TLoc : LangKey.Chat.T),
                CsTeam.CounterTerrorist => isDead ? LangKey.Chat.CtDead : (hasPlace ? LangKey.Chat.CtLoc : LangKey.Chat.Ct),
                _ => LangKey.Chat.All
            };
        }

        return (IsPlayerTeam(team) && isDead) ? LangKey.Chat.AllDead : LangKey.Chat.All;
    }

    /// <summary>
    /// Checks if the team is either terrorist or counter-terrorist.
    /// </summary>
    /// <param name="team">The team of the player.</param>
    /// <returns>True if the team is terrorist or counter-terrorist; otherwise, false.</returns>
    private bool IsPlayerTeam(CsTeam team)
    {
        return (team == CsTeam.Terrorist || team == CsTeam.CounterTerrorist);
    }

    /// <summary>
    /// Gets the list of recipients based on the team.
    /// </summary>
    /// <param name="team">The team of the player. Default is CsTeam.None.</param>
    /// <returns>A list of CCSPlayerController representing the recipients.</returns>
    private List<CCSPlayerController> GetRecipients(CsTeam team = CsTeam.None)
    {
        return Utilities.GetPlayers()
            .Where(player => IsValidPlayer(player) && (team == CsTeam.None || player.Team == team))
            .ToList();
    }

    private bool IsValidPlayer([NotNullWhen(true)] CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot;
    }
}

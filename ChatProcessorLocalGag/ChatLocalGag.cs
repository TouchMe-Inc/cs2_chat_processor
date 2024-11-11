﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Core.Attributes;

namespace ChatProcessor;

[MinimumApiVersion(285)]
public class ChatLocalGag : BasePlugin
{
    public override string ModuleName => "ChatLocalGag";
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "Adds the ability to local gag for players!";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private readonly Dictionary<ulong, List<ulong>> _gags = [];

    private IChatProcessor? _api;

    internal static IStringLocalizer? Stringlocalizer;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        Stringlocalizer = Localizer;

        _api = _pluginCapability.Get();

        if (_api == null) return;

        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        _api.RegisterHandlerPre(OnChatMessagePre);
    }

    public override void Unload(bool hotReload)
    {
        _api?.DeregisterHandlerPre(OnChatMessagePre);
    }

    [ConsoleCommand("localgag", "Opens a menu with a list of players to block a player's chat.")]
    public void OnLocalGagCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || !caller.IsValid)
        {
            return;
        }

        OpenPlayerMenu(caller);
    }

    private void OpenPlayerMenu(CCSPlayerController player)
    {
        var PlayerMenu = new ChatMenu(Localizer["menu.title"]);

        IEnumerable<CCSPlayerController> playerEntities = GetValidPlayers();

        bool hasMutes = _gags.ContainsKey(player.SteamID);

        foreach (var playerEntity in playerEntities)
        {
            string itemText = GetMenuItemText(player, playerEntity, hasMutes);
            PlayerMenu.AddMenuOption(itemText, (_, _) =>
            {
                ToggleMute(player, playerEntity, hasMutes);

                OpenPlayerMenu(player);
            });
        }

        PlayerMenu.ExitButton = true;

        PlayerMenu.Open(player);
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!IsValidPlayer(player)) return HookResult.Continue;

        _gags.Remove(player.SteamID);

        return HookResult.Continue;
    }

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {

        if (_gags.ContainsKey(sender.SteamID))
        {
            RemoveMutedRecipients(sender, ref recipients);

            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    private void RemoveMutedRecipients(CCSPlayerController sender, ref List<CCSPlayerController> recipients)
    {
        int recipient = 0;

        while (recipient < recipients.Count)
        {
            if (IsRecipientMuted(recipients[recipient], sender.SteamID))
            {
                recipients.RemoveAt(recipient);
            }
            else
            {
                recipient++;
            }
        }
    }

    private string GetMenuItemText(CCSPlayerController player, CCSPlayerController playerEntity, bool hasMutes)
    {
        return hasMutes && _gags[player.SteamID].Contains(playerEntity.SteamID)
            ? Localizer["menu.item.player.muted", playerEntity.PlayerName]
            : Localizer["menu.item.player.unmuted", playerEntity.PlayerName];
    }

    private void ToggleMute(CCSPlayerController player, CCSPlayerController playerEntity, bool hasMutes)
    {
        List<ulong> blockList = hasMutes ? _gags[player.SteamID] : new List<ulong>();

        if (blockList.Contains(playerEntity.SteamID))
        {
            blockList.Remove(playerEntity.SteamID);
            player.PrintToChat(Localizer["message.unmuted", playerEntity.PlayerName]);
        }
        else
        {
            blockList.Add(playerEntity.SteamID);
            player.PrintToChat(Localizer["message.muted", playerEntity.PlayerName]);
        }

        _gags[player.SteamID] = blockList;
    }

    private IEnumerable<CCSPlayerController> GetValidPlayers()
    {
        return Utilities.GetPlayers().Where(IsValidPlayer);
    }

    private bool IsRecipientMuted(CCSPlayerController recipient, ulong senderSteamID)
    {
        return _gags.TryGetValue(recipient.SteamID, out List<ulong>? mutedList) && mutedList.Contains(senderSteamID);
    }

    private bool IsValidPlayer(CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
    }
}

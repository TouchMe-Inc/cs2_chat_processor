using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Localization;

namespace ChatProcessor;

public class ChatLocalMute : BasePlugin
{
    public override string ModuleName => "ChatLocalMute";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "Adds the ability to local mute for players!";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private Dictionary<ulong, List<ulong>> _mutes = [];

    private IChatProcessor? ChatProcessorApi;

    internal static IStringLocalizer? Stringlocalizer;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        Stringlocalizer = Localizer;

        ChatProcessorApi = _pluginCapability.Get();

        if (ChatProcessorApi == null) return;

        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        ChatProcessorApi.RegisterHandlerPre(OnChatMessagePre);
    }

    [ConsoleCommand("localmute", "Opens a menu with a list of players to block a player's chat.")]
    public void OnLocalMuteCommand(CCSPlayerController? caller, CommandInfo command)
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

        IEnumerable<CCSPlayerController> playerEntities = Utilities.GetPlayers().Where(player => player is
        {
            IsBot: false,
            IsValid: true
        });

        bool has_mutes = _mutes.ContainsKey(player.SteamID);

        foreach (var playerEntity in playerEntities)
        {
            PlayerMenu.AddMenuOption(has_mutes && _mutes[player.SteamID].Contains(playerEntity.SteamID) ? Localizer["menu.item.player.muted", playerEntity.PlayerName] : Localizer["menu.item.player.unmuted", playerEntity.PlayerName],
                (_, _) =>
                {
                    List<ulong> blockList = has_mutes ? _mutes[player.SteamID] : [];

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

                    _mutes[player.SteamID] = blockList;

                    OpenPlayerMenu(player);
                });
        }

        PlayerMenu.ExitButton = true;

        PlayerMenu.Open(player);
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;

        _mutes.Remove(player.SteamID);

        return HookResult.Continue;
    }

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {
        int recipient = 0;

        while (recipient < recipients.Count)
        {
            if (_mutes.TryGetValue(recipients[recipient].SteamID, out List<ulong>? value) && value.Contains(sender.SteamID))
            {
                recipients.Remove(recipients[recipient]);
            }
            else
            {
                recipient++;
            }
        }

        return HookResult.Handled;
    }
}

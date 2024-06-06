using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using System.Numerics;
using CounterStrikeSharp.API;

namespace ChatMute;

public class ChatLocalMute : BasePlugin
{
    public override string ModuleName => "ChatLocalMute";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "A simple plugin!";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private IChatProcessor? ChatProcessorApi;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        ChatProcessorApi = _pluginCapability.Get();

        if (ChatProcessorApi == null) return;

        ChatProcessorApi.RegisterHandlerPre(OnChatMessagePre);
        ChatProcessorApi.RegisterHandlerPost(OnChatMessagePost);
    }

    [ConsoleCommand("localmute", "Locally hide player messages.")]
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
        var PlayerMenu = new ChatMenu("Player Menu");

        IEnumerable<CCSPlayerController> playerEntities = Utilities.GetPlayers().Where(player => player is
        {
            IsValid: true
        });

        foreach (var playerEntity in playerEntities)
        {
            PlayerMenu.AddMenuOption(playerEntity.PlayerName, (_, _) => { Console.WriteLine($"Selected {playerEntity.PlayerName}"); });
        }

        PlayerMenu.ExitButton = true;

        PlayerMenu.Open(player);
    }

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {
        return HookResult.Handled;
    }

    private void OnChatMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, int flags)
    {
    }
}

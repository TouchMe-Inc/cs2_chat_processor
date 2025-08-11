using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace ChatProcessor;

[MinimumApiVersion(318)]
public class ChatSpecViewEx : BasePlugin
{
    public override string ModuleName => "ChatSpecViewEx";
    public override string ModuleVersion => "1.1.1";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "Allow spectators to read team messages";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private IChatProcessor? _api;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = _pluginCapability.Get();

        if (_api == null) return;

        _api.RegisterHandlerPre(OnChatMessagePre);
    }

    public override void Unload(bool hotReload)
    {
        _api?.DeregisterHandlerPre(OnChatMessagePre);
    }

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message,
                                        ref List<CCSPlayerController> recipients, ref ChatFlags flags)
    {
        if (!flags.HasFlag(ChatFlags.Team) || sender.Team == CsTeam.Spectator)
        {
            return HookResult.Continue;
        }

        AddSpectatorsToRecipients(ref recipients);

        return HookResult.Handled;
    }

    private void AddSpectatorsToRecipients(ref List<CCSPlayerController> recipients)
    {
        IEnumerable<CCSPlayerController> spectators = Utilities.GetPlayers()
            .Where(player => IsValidPlayer(player) && player.Team == CsTeam.Spectator);
        recipients.AddRange(spectators);
    }

    private bool IsValidPlayer([NotNullWhen(true)] CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot;
    }
}

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Attributes;

namespace ChatProcessor;

[MinimumApiVersion(285)]
public class ChatSpecViewEx : BasePlugin
{
    public override string ModuleName => "ChatSpecViewEx";
    public override string ModuleVersion => "1.1.0";
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
                                        ref List<CCSPlayerController> recipients, ref int flags)
    {
        if (IsTeamChat(flags) && sender.Team != CsTeam.Spectator)
        {
            AddSpectatorsToRecipients(ref recipients);

            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    private bool IsTeamChat(int flags)
    {
        return (flags & (int)ChatFlags.Team) != 0;
    }

    private void AddSpectatorsToRecipients(ref List<CCSPlayerController> recipients)
    {
        IEnumerable<CCSPlayerController> spectators = Utilities.GetPlayers()
            .Where(player => player.IsValid && !player.IsBot && player.Team == CsTeam.Spectator);
        recipients.AddRange(spectators);
    }
}

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace ChatProcessor;

public class ChatSpecViewEx : BasePlugin
{
    public override string ModuleName => "ChatSpecViewEx";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "Allow spectators to read team messages";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");

    private IChatProcessor? ChatProcessorApi;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        ChatProcessorApi = _pluginCapability.Get();

        if (ChatProcessorApi == null) return;

        ChatProcessorApi.RegisterHandlerPre(OnChatMessagePre);
    }

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {
        if ((flags & (int)ChatFlags.Team) != 0 && sender.Team != CsTeam.Spectator)
        {
            IEnumerable<CCSPlayerController> playerEntities = Utilities.GetPlayers().Where(player => player is
            {
                IsValid: true,
                IsBot: false,
                Team: CsTeam.Spectator
            });

            recipients.Concat(playerEntities);

            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}

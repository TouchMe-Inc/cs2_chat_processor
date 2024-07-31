using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core;
using ChatProcessor.API;
using CounterStrikeSharp.API.Core.Capabilities;
using Microsoft.Extensions.Logging;

namespace ChatProcessor;

[MinimumApiVersion(253)]
public class ChatTags : BasePlugin
{
    public override string ModuleName => "ChatTags";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "...";

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

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {
        // TODO
        Logger.LogInformation($"Sender SteamId = {sender.SteamID}");

        return HookResult.Continue;
    }
}

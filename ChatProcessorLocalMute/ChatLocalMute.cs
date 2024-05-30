using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;

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

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {
        name = "KEK";
        
        return HookResult.Handled;
    }

    private void OnChatMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, int flags)
    {
    }
}

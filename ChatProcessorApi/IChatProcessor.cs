using CounterStrikeSharp.API.Core;

namespace ChatProcessor.API;

/// <summary>
/// Api for ChatProcessor.
/// </summary>
public interface IChatProcessor
{
    delegate HookResult MessagePreCallback(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags);

    delegate void MessagePostCallback(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, int flags);

    void RegisterHandlerPre(MessagePreCallback handler);

    void DeregisterHandlerPre(MessagePreCallback handler);

    void RegisterHandlerPost(MessagePostCallback handler);

    void DeregisterHandlerPost(MessagePostCallback handler);
}

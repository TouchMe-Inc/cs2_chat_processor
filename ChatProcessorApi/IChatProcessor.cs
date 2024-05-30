using CounterStrikeSharp.API.Core;

namespace ChatProcessor.API;

/// <summary>
/// Api for ChatProcessor.
/// </summary>
public interface IChatProcessor
{
    delegate HookResult MessagePreCallback(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags);

    delegate void MessagePostCallback(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, int flags);

    /// <summary>
    /// Registers a chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterHandlerPre(MessagePreCallback handler);

    /// <summary>
    /// De-registers a chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    void DeregisterHandlerPre(MessagePreCallback handler);

    /// <summary>
    /// Registers a chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterHandlerPost(MessagePostCallback handler);

    /// <summary>
    /// De-registers a chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    void DeregisterHandlerPost(MessagePostCallback handler);
}

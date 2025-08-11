using CounterStrikeSharp.API.Core;
using ChatProcessor.API;

namespace ChatProcessor;

public class ChatProcessorApi : IChatProcessor
{
    private readonly ChatProcessor _chatProcessor;

    private readonly List<IChatProcessor.MessageCallbackPre> _messagePreHandlers = [];
    private readonly List<IChatProcessor.MessageCallbackPost> _messagePostHandlers = [];

    public ChatProcessorApi(ChatProcessor chatProcessor)
    {
        _chatProcessor = chatProcessor;
    }

    public void RegisterHandlerPre(IChatProcessor.MessageCallbackPre handler)
    {
        _messagePreHandlers.Add(handler);
    }

    public void DeregisterHandlerPre(IChatProcessor.MessageCallbackPre handler)
    {
        _messagePreHandlers.Remove(handler);
    }

    public void RegisterHandlerPost(IChatProcessor.MessageCallbackPost handler)
    {
        _messagePostHandlers.Add(handler);
    }

    public void DeregisterHandlerPost(IChatProcessor.MessageCallbackPost handler)
    {
        _messagePostHandlers.Remove(handler);
    }

    public void TriggerMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref ChatFlags flags)
    {
        foreach (var handler in _messagePreHandlers)
        {
            var savedName = name;
            var savedMessage = message;
            var savedRecipients = new List<CCSPlayerController>(recipients);
            var savedFlags = flags;

            switch (handler.Invoke(sender, ref name, ref message, ref recipients, ref flags))
            {
                case HookResult.Stop: return;

                case HookResult.Continue:
                {
                    name = savedName;
                    message = savedMessage;
                    recipients = savedRecipients;
                    flags = savedFlags;
                    break;
                }
            }
        }
    }

    public void TriggerMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, ChatFlags flags)
    {
        foreach (var handler in _messagePostHandlers)
        {
            handler.Invoke(sender, name, message, recipients, flags);
        }
    }
}

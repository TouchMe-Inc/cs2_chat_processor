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

    public void TriggerMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {
        foreach (var handler in _messagePreHandlers)
        {
            string bameCopy = name;
            string messageCopy = message;
            List<CCSPlayerController> recipientsCopy = recipients;
            int flagsCopy = flags;

            HookResult hookResult = handler.Invoke(sender, ref name, ref message, ref recipients, ref flags);

            if (hookResult == HookResult.Stop)
            {
                return;
            }
            else if (hookResult == HookResult.Continue)
            {
                name = bameCopy;
                message = messageCopy;
                recipients = recipientsCopy;
                flags = flagsCopy;
            }
        }
    }
    public void TriggerMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, int flags)
    {
        foreach (var handler in _messagePostHandlers)
        {
            handler.Invoke(sender, name, message, recipients, flags);
        }
    }
}

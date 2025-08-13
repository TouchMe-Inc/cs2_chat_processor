# About ChatProcessor

API for chat manipulation.

> [!IMPORTANT]
> The plugin hides original messages and reproduces them via player.PrintToChat, preserving the location tag (for team chat), and also sends a copy of the chat to the console

Ready implementation: 
* Locally gag the player ([Here](https://github.com/TouchMe-Inc/cs2_chat_processor/tree/dev/src/Modules/ChatProcessor.LocalGag/));
* Display team chat for spectators ([Here](https://github.com/TouchMe-Inc/cs2_chat_processor/tree/dev/src/Modules/ChatProcessor.SpecViewEx/));
* Add a signature to the nickname ([Here](https://github.com/TouchMe-Inc/cs2_chat_processor/tree/dev/src/Modules/ChatProcessor.Tags/));

## Development

First let's add a field:
```c#
private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor");
```

Next we need to get an API instance, with which we will catch messages:
```c#
public override void OnAllPluginsLoaded(bool hotReload)
{
  // ... 
  ChatProcessorApi = _pluginCapability.Get();

  if (ChatProcessorApi == null) return;

  // Add handler PRE
  ChatProcessorApi.RegisterHandlerPre(OnChatMessagePre);

  // OR/AND

  // Add handler POST
  ChatProcessorApi.RegisterHandlerPost(OnChatMessagePost);
  // ...    
}
```

Add `MessageCallbackPre` (Allows you to edit nickname, message and recipients):
```c#
private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref ChatFlags flags)
{
  // You code here
  return HookResult.Handled;
}
```

Add `MessageCallbackPost` (Allows you to see the total values ​​of nickname, message and recipients):
```c#
private void OnChatMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, ChatFlags flags)
{
  // You code here
}
```

# Required
[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) >= 1.318

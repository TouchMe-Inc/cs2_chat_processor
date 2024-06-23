# About ChatProcessor
API for chat manipulation.

> [!IMPORTANT]
> The plugin hides original messages and reproduces them via player.PrintToChat, preserving the location tag (for team chat), and also sends a copy of the chat to the console


This toolkit allows you to: 
* locally mute the player ([here](https://github.com/TouchMe-Inc/cs2_chat_processor/blob/main/ChatProcessorLocalMute/ChatLocalMute.cs));
* display team chat for spectators ([here](https://github.com/TouchMe-Inc/cs2_chat_processor/blob/main/ChatProcessorSpecViewEx/ChatSpecViewEx.cs));
* hide or change player nickname/messages;
* add a signature to the nickname (for example, admin/vip);
* etc.

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

Add [MessageCallbackPre](https://github.com/TouchMe-Inc/cs2_chat_processor/blob/5374d14a8eeca2f3e3b52fbe160b887e7784a855/ChatProcessorApi/IChatProcessor.cs#L10) (Allows you to edit nickname, message and recipients):
```c#
private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
{
  // You code here
  return HookResult.Handled;
}
```

Add [MessageCallbackPost](https://github.com/TouchMe-Inc/cs2_chat_processor/blob/5374d14a8eeca2f3e3b52fbe160b887e7784a855/ChatProcessorApi/IChatProcessor.cs#L13) (Allows you to see the total values ​​of nickname, message and recipients):
```c#
private void OnChatMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, int flags)
{
  // You code here
}
```

Example: [ChatLocalMute](https://github.com/TouchMe-Inc/cs2_chat_processor/blob/main/ChatProcessorLocalMute/ChatLocalMute.cs)

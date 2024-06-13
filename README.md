# About ChatProcessor
API for chat manipulation.

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

  // Add handler PRE
  ChatProcessorApi.RegisterHandlerPost(OnChatMessagePost);
  // ...    
}
```

Add MessagePreCallback:
```c#
private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
{
  // You code here
  return HookResult...
}
```

Add MessagePostCallback:
```c#
private void OnChatMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, int flags)
{
  // You code here
}
```

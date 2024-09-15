using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core;
using ChatProcessor.API;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using ChatProcessorTags.Config;

namespace ChatProcessor;

[MinimumApiVersion(253)]
public class ChatTags : BasePlugin, IPluginConfig<TagsConfig>
{
    public override string ModuleName => "ChatTags";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "TouchMe";
    public override string ModuleDescription => "Adds chat tags that can be assigned via permission/group or SteamID64";

    public TagsConfig Config { get; set; } = new TagsConfig();

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

    public void OnConfigParsed(TagsConfig config)
    {
        if (!config.Group.All(group => group.Key.StartsWith('#')))
        {
            throw new Exception("Incorrect group format");
        }

        if (!config.Permission.All(permission => permission.Key.StartsWith('@')))
        {
            throw new Exception("Incorrect permissions format");
        }

        Config = config;
    }

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref int flags)
    {
        Tag tag;

        if ((tag = Config.SteamID.FirstOrDefault(tag => tag.Key == sender.SteamID.ToString()).Value) != null ||
            (tag = Config.Group.FirstOrDefault(tag => AdminManager.PlayerInGroup(sender, tag.Key)).Value) != null ||
            (tag = Config.Permission.FirstOrDefault(tag => AdminManager.PlayerHasPermissions(sender, tag.Key)).Value) != null)
        {
            name = $"{tag.Template} {name}";
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}

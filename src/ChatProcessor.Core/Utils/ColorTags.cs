using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;

namespace ChatProcessor.Utils;

public static class ColorTags
{
    public const string TeamColor = "{TeamColor}";

    private static readonly Dictionary<string, string> _colorTagMap;

    static ColorTags()
    {
        _colorTagMap = typeof(ChatColors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .GroupBy(f => f.Name.ToLowerInvariant())
            .Select(g => g.First())
            .ToDictionary(
                f => $"{{{f.Name}}}",
                f => f.GetValue(null)?.ToString() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase
            );
    }

    public static string Replace(string message, CsTeam team = CsTeam.None)
    {
        if (message.Contains(TeamColor, StringComparison.OrdinalIgnoreCase))
        {
            var teamColor = (team == CsTeam.Terrorist || team == CsTeam.CounterTerrorist)
                ? ChatColors.ForTeam(team)
                : ChatColors.White;

            message = ReplaceIgnoreCase(message, TeamColor, teamColor.ToString());
        }

        foreach (var kv in _colorTagMap)
        {
            message = ReplaceIgnoreCase(message, kv.Key, kv.Value);
        }

        return message;
    }

    public static string Remove(string message)
    {
        message = ReplaceIgnoreCase(message, TeamColor, string.Empty);

        foreach (var kv in _colorTagMap)
        {
            message = ReplaceIgnoreCase(message, kv.Key, string.Empty);
        }

        return message;
    }

    private static string ReplaceIgnoreCase(string input, string oldValue, string newValue)
    {
        return input.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
    }
}

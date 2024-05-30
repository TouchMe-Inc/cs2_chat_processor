﻿using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;

namespace ChatProcessor.Utils;

public static class Tags
{
    public static string teamColorTag = "__TeamColor__";

    public static string ReplaceColorTags(string message, CsTeam team = CsTeam.None)
    {
        if (message.Contains(teamColorTag, StringComparison.OrdinalIgnoreCase))
        {
            var teamColor = ChatColors.White;

            if (team == CsTeam.Terrorist || team == CsTeam.CounterTerrorist)
            {
                teamColor = ChatColors.ForTeam(team);
            }

            message = message.Replace(teamColorTag, teamColor.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        foreach (var field in typeof(ChatColors).GetFields())
        {
            string pattern = $"__{field.Name}__";
            if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                message = message.Replace(pattern, field.GetValue(null)?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }
        }

        return message;
    }

    public static string RemoveColorTags(string message)
    {
        if (message.Contains(teamColorTag, StringComparison.OrdinalIgnoreCase))
        {
            message = message.Replace(teamColorTag, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        foreach (FieldInfo field in typeof(ChatColors).GetFields())
        {
            string pattern = $"__{field.Name}__";
            if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                message = message.Replace(pattern, string.Empty, StringComparison.OrdinalIgnoreCase);
            }
        }

        return message;
    }
}

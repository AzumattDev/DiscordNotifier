using System;
using WardIsLove.Util.DiscordMessenger;

namespace DiscordNotifier;

public class Utils
{
    public static void LogToDiscord(string title, string characterName, int playerCount)
    {
        new DiscordMessage()
            .SetUsername($"DN v{DiscordNotifierPlugin.ModVersion}")
            .SetAvatar("https://cdn.akamai.steamstatic.com/steam/apps/2080690/ss_b3c667c680bfb987b41e2c16ed85289448f496a6.1920x1080.jpg?t=1693808856")
            .AddEmbed()
            .SetTimestamp(DateTime.UtcNow)
            .SetAuthor($"")
            .SetTitle(title)
            .SetColor(15258703)
            .AddField("Player Name", Escaper($"> {characterName}"), true)
            .AddField(playerCount > 1 ? "Players" : "Player", $"> {playerCount}", true)
            .Build()
            .SendMessageAsync(DiscordNotifierPlugin.discordWebhook.Value);
    }

    static string Escaper(string StrIn)
    {
        return StrIn.Replace("\"", "\\\"");
    }
}
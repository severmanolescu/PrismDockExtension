using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PristDockExtension;

internal sealed partial class PristDockExtensionPage : ListPage
{
    private const string DbPath = @"C:\Users\sever\AppData\Roaming\prism\data\tracker.db";

    private const string IconsFolder = @"C:\Users\sever\AppData\Roaming\prism\icons";


    public PristDockExtensionPage()
    {
        Icon = new IconInfo("\uE823");
        Title = "Prism Time Tracker";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var sessions = ReadFromDb();

        if (sessions.Count == 0)
            return [new ListItem(new NoOpCommand()) { Title = "No data for today" }];

        var items = new List<IListItem>();
        foreach (var s in sessions)
        {
            var time = TimeSpan.FromMilliseconds(s.TotalMs);
            var formatted = time.TotalHours >= 1
                ? $"{(int)time.TotalHours}h {time.Minutes}m"
                : $"{time.Minutes}m {time.Seconds}s";

            var displayName = string.Join(" ",
                s.AppName.Split('_')
                       .Select(w => char.ToUpper(w[0]) + w[1..]));

            items.Add(new ListItem(new CopyTextCommand(s.AppName))
            {
                Title = s.AppName,
                Subtitle = formatted,
                Icon = s.IconPath != null
                    ? new IconInfo(Path.Combine(IconsFolder, Path.GetFileName(s.IconPath)))
                    : new IconInfo("\uE7EF"),
            });
        }

        return items.ToArray();
    }

    private static List<AppSession> ReadFromDb()
    {
        var results = new List<AppSession>();

        try
        {
            var todayStart = new DateTimeOffset(DateTime.Today).ToUnixTimeMilliseconds();
            var todayEnd = todayStart + 86_400_000L;

            using var connection = new SqliteConnection($"Data Source={DbPath};Mode=ReadOnly");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT a.name, SUM(s.duration) as total_ms, a.icon_path
                FROM sessions s
                JOIN apps a ON a.id = s.app_id
                WHERE s.start_time >= $start
                  AND s.start_time <  $end
                  AND s.duration   >  0
                  AND a.hidden     =  0
                GROUP BY s.app_id
                ORDER BY total_ms DESC
                """;
            cmd.Parameters.AddWithValue("$start", todayStart);
            cmd.Parameters.AddWithValue("$end", todayEnd);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new AppSession(
                    reader.GetString(0),
                    reader.GetInt64(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)
                ));
            }
        }
        catch
        {
            // swallow — GetItems() will return empty list gracefully
        }

        return results;
    }

    private record AppSession(string AppName, long TotalMs, string? IconPath);
}
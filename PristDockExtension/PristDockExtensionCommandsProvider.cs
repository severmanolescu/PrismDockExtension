using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Data.Sqlite;
using System;

namespace PristDockExtension;

public partial class PristDockExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public PristDockExtensionCommandsProvider()
    {
        DisplayName = "Prism";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        var totalMs = GetTodayTotalMs();
        var total = TimeSpan.FromMilliseconds(totalMs);
        var label = total.TotalHours >= 1
            ? $"Today: {(int)total.TotalHours}h {total.Minutes}m"
            : $"Today: {total.Minutes}m";

        _commands =
        [
            new CommandItem(new PristDockExtensionPage()) { Title = label },
    ];
    }

    private static long GetTodayTotalMs()
    {
        try
        {
            SQLitePCL.Batteries_V2.Init();
            var todayStart = new DateTimeOffset(DateTime.Today).ToUnixTimeMilliseconds();
            var todayEnd = todayStart + 86_400_000L;

            using var connection = new SqliteConnection(@"Data Source=C:\Users\sever\AppData\Roaming\prism\data\tracker.db;Mode=ReadOnly");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
            SELECT SUM(duration)
            FROM sessions
            WHERE start_time >= $start
              AND start_time <  $end
              AND duration   >  0
            """;
            cmd.Parameters.AddWithValue("$start", todayStart);
            cmd.Parameters.AddWithValue("$end", todayEnd);

            var result = cmd.ExecuteScalar();
            return result is long ms ? ms : 0;
        }
        catch { return 0; }
    }

    public override ICommandItem[] TopLevelCommands() => _commands;
}
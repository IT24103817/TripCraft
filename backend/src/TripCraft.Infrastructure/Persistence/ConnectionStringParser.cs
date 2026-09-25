using Npgsql;

namespace TripCraft.Infrastructure.Persistence;

public static class ConnectionStringParser
{
    /// <summary>
    /// Neon and Render hand out URLs like postgresql://user:pass@host:5432/db?sslmode=require,
    /// but Npgsql wants "Host=...;Username=...". Accept both forms.
    /// </summary>
    public static string ToNpgsql(string databaseUrl)
    {
        if (!databaseUrl.StartsWith("postgres://") && !databaseUrl.StartsWith("postgresql://"))
            return databaseUrl;

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null
        };

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        if (query["sslmode"] is { } sslMode)
            builder.SslMode = Enum.Parse<SslMode>(sslMode, ignoreCase: true);

        return builder.ConnectionString;
    }
}

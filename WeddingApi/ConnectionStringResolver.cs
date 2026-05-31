using Microsoft.Extensions.Configuration;
using Npgsql;

namespace WeddingApi;

public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        foreach (var key in new[] { "DATABASE_URL", "DATABASE_PRIVATE_URL", "DATABASE_PUBLIC_URL" })
        {
            var value = ReadVar(configuration, key);
            if (value is not null)
                return ToNpgsql(value);
        }

        var pgHost = ReadVar(configuration, "PGHOST");
        if (pgHost is not null)
        {
            return BuildFromParts(
                pgHost,
                ReadVar(configuration, "PGPORT") ?? "5432",
                ReadVar(configuration, "PGUSER") ?? "postgres",
                ReadVar(configuration, "PGPASSWORD") ?? "",
                ReadVar(configuration, "PGDATABASE") ?? "railway");
        }

        var fromConfig = configuration.GetConnectionString("DefaultConnection");
        if (fromConfig is not null && !IsLocalhost(fromConfig))
            return ToNpgsql(fromConfig);

        if (environment.IsDevelopment() && fromConfig is not null)
            return ToNpgsql(fromConfig);

        throw new InvalidOperationException(
            "No Railway database connection found. On your API service Variables tab, use " +
            "'Add Reference' from PostgreSQL and add DATABASE_URL (not a typed literal).");
    }

    public static string DescribeHost(string connectionString)
    {
        try
        {
            return new NpgsqlConnectionStringBuilder(connectionString).Host ?? "(unknown)";
        }
        catch
        {
            return "(unknown)";
        }
    }

    private static string? ReadVar(IConfiguration configuration, string key)
    {
        var value = Environment.GetEnvironmentVariable(key) ?? configuration[key];
        if (string.IsNullOrWhiteSpace(value)) return null;

        value = value.Trim().Trim('"').Trim('\'');

        if (value.Contains("${{", StringComparison.Ordinal) || value.Contains("${", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Environment variable {key} is unresolved ({value}). " +
                "In Railway, delete it and re-add via Variables → Add Reference → PostgreSQL → {key}.");
        }

        return value;
    }

    private static string ToNpgsql(string value)
    {
        value = value.Trim().Trim('"').Trim('\'');

        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return new NpgsqlConnectionStringBuilder(value).ConnectionString;
        }

        // Uri scheme "postgres://" is not parsed reliably; use http:// for parsing only.
        var parseUri = value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            ? "http://" + value["postgresql://".Length..]
            : "http://" + value["postgres://".Length..];

        var uri = new Uri(parseUri);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "postgres";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.TrimStart('/').Split('?')[0];
        if (string.IsNullOrEmpty(database))
            database = "railway";

        return BuildFromParts(
            uri.Host,
            uri.Port > 0 ? uri.Port.ToString() : "5432",
            user,
            password,
            database);
    }

    private static string BuildFromParts(
        string host, string port, string user, string password, string database)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(port, out var p) ? p : 5432,
            Username = user,
            Password = password,
            Database = database,
            SslMode = SslMode.Require
        };
        return builder.ConnectionString;
    }

    private static bool IsLocalhost(string connectionString) =>
        connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("127.0.0.1", StringComparison.Ordinal);
}

using Microsoft.Data.SqlClient;

// Usage: dotnet run -- <tunnelUrl> [user] [password] [database]
// Example: dotnet run -- ws://gipsoft-dlmhp-26:8751/sqltunnel gip netspirit iPlusV5
var tunnelUrl = args.Length > 0 ? args[0] : "ws://localhost:8751/sqltunnel";
var user = args.Length > 1 ? args[1] : "gip";
var password = args.Length > 2 ? args[2] : "netspirit";
var database = args.Length > 3 ? args[3] : "iPlusV5";

// TLS test (docs §9.28): accept the relay's self-signed certificate for wss:// tunnels.
if (Environment.GetEnvironmentVariable("SPIKE_ACCEPT_UNTRUSTED_CERT") == "1")
    AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.AcceptUntrustedTunnelCertificates", true);

// Data Source is a placeholder; the tunnel URL drives the transport.
var builder = new SqlConnectionStringBuilder
{
    DataSource = "sqltunnel",
    TunnelUrl = tunnelUrl,
    UserID = user,
    Password = password,
    InitialCatalog = database,
    Encrypt = false,
    TrustServerCertificate = true,
    ConnectTimeout = 15,
    // TEMPORARY MARS test (SQL Tunnel debugging): the browser client connects with
    // multipleactiveresultsets=True; verify the MARS demux path over the relay.
    MultipleActiveResultSets = Environment.GetEnvironmentVariable("SPIKE_MARS") == "1"
};

Console.WriteLine($"Connecting via tunnel: {tunnelUrl}");

bool syncMode = Environment.GetEnvironmentVariable("SPIKE_SYNC") == "1";

await using var conn = new SqlConnection(builder.ConnectionString);
if (syncMode)
{
    conn.Open(); // sync path — matches the browser client (EF sync)
    Console.WriteLine("Connected (sync).");
    using var cmd = new SqlCommand("SELECT @@VERSION", conn);
    Console.WriteLine($"@@VERSION: {cmd.ExecuteScalar()}");
    using var cmd2 = new SqlCommand("SELECT COUNT(*) FROM sys.databases", conn);
    Console.WriteLine($"sys.databases count: {cmd2.ExecuteScalar()}");
}
else
{
    await conn.OpenAsync();

    Console.WriteLine($"Connected. Server: {conn.DataSource}, Database: {conn.Database}");

    await using var cmd = new SqlCommand("SELECT @@VERSION", conn);
    var version = await cmd.ExecuteScalarAsync();
    Console.WriteLine($"@@VERSION: {version}");

    await using var cmd2 = new SqlCommand("SELECT COUNT(*) FROM sys.databases", conn);
    Console.WriteLine($"sys.databases count: {await cmd2.ExecuteScalarAsync()}");
}

Console.WriteLine("SPIKE OK");

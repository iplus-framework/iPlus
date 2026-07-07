using System;
using System.Threading;
using System.Threading.Tasks;

namespace gip.iplus.client.avui;

public sealed class ExternalLoginCredentials
{
    public ExternalLoginCredentials(string userName, string password, string source)
    {
        UserName = userName ?? string.Empty;
        Password = password ?? string.Empty;
        Source = source ?? string.Empty;
    }

    public string UserName { get; }

    public string Password { get; }

    public string Source { get; }
}

public interface IExternalLoginProvider
{
    string Name { get; }

    Task<ExternalLoginCredentials> TryGetCredentialsAsync(CancellationToken cancellationToken);
}
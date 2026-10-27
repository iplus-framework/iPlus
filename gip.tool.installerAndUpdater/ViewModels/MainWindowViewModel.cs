// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gip.tool.installerAndUpdater.Models;
using gip.tool.installerAndUpdater.Services;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace gip.tool.installerAndUpdater.ViewModels
{
    /// <summary>
    /// Drives the install/update wizard.
    /// </summary>
    public sealed class MainWindowViewModel : ReactiveObject
    {
        private readonly InstallService _installService = new InstallService();
        private readonly SqlServerService _sqlService = new SqlServerService();
        private readonly GitHubReleaseService _githubService = new GitHubReleaseService();

        private CancellationTokenSource? _cancellation;

        public MainWindowViewModel()
        {
            Products = new[]
            {
                new Choice<ProductKind>(ProductKind.Framework, "iPlus framework"),
                new Choice<ProductKind>(ProductKind.Mes, "iPlus-MES")
            };
            Operations = new[]
            {
                new Choice<OperationKind>(OperationKind.Install, "Install (fresh)"),
                new Choice<OperationKind>(OperationKind.Update, "Update (keep user files)")
            };
            Sources = new[]
            {
                new Choice<SourceKind>(SourceKind.GitHubRelease, "GitHub release (download)"),
                new Choice<SourceKind>(SourceKind.LocalZip, "Local zip file"),
                new Choice<SourceKind>(SourceKind.LocalFolder, "Local folder")
            };
            AuthModes = new[]
            {
                new Choice<SqlAuthMode>(SqlAuthMode.SqlServerAuthentication, "SQL Server authentication"),
                new Choice<SqlAuthMode>(SqlAuthMode.WindowsAuthentication, "Windows authentication")
            };

            SuggestedServers = new ObservableCollection<string>(SqlServerService.GetSuggestedServerNames());
            Releases = new ObservableCollection<GitHubRelease>();
            Log = new ObservableCollection<string>();

            TargetPath = GetDefaultTargetPath();
            DatabaseName = "iPlusV5";
            ServerName = SuggestedServers.FirstOrDefault() ?? "localhost";
            PlatformFolder = PayloadService.GetCurrentPlatformFolder(ProductKind.Framework);
            Product = Products[0];
            Operation = Operations[0];
            Source = Sources[0];
            AuthMode = AuthModes[0];

            RunCommand = ReactiveCommand.CreateFromTask(RunAsync, this.WhenAnyValue(x => x.IsBusy, b => !b));
            CancelCommand = ReactiveCommand.Create(Cancel, this.WhenAnyValue(x => x.IsBusy));
            LoadReleasesCommand = ReactiveCommand.CreateFromTask(LoadReleasesAsync, this.WhenAnyValue(x => x.IsBusy, b => !b));
            TestConnectionCommand = ReactiveCommand.CreateFromTask(TestConnectionAsync, this.WhenAnyValue(x => x.IsBusy, b => !b));
            BrowseSourceCommand = ReactiveCommand.CreateFromTask(BrowseSourceAsync);
            BrowseTargetCommand = ReactiveCommand.CreateFromTask(BrowseTargetAsync);
            BrowseScriptCommand = ReactiveCommand.CreateFromTask(BrowseScriptAsync);
        }

        #region Choices

        public IReadOnlyList<Choice<ProductKind>> Products { get; }

        public IReadOnlyList<Choice<OperationKind>> Operations { get; }

        public IReadOnlyList<Choice<SourceKind>> Sources { get; }

        public IReadOnlyList<Choice<SqlAuthMode>> AuthModes { get; }

        public ObservableCollection<string> SuggestedServers { get; }

        public ObservableCollection<GitHubRelease> Releases { get; }

        public ObservableCollection<string> Log { get; }

        private Choice<ProductKind>? _product;
        public Choice<ProductKind>? Product
        {
            get => _product;
            set
            {
                this.RaiseAndSetIfChanged(ref _product, value);
                ProductKind kind = value?.Value ?? ProductKind.Framework;
                DatabaseName = kind == ProductKind.Mes ? "iPlusMESV5" : "iPlusV5";
                PlatformFolder = PayloadService.GetCurrentPlatformFolder(kind);
                this.RaisePropertyChanged(nameof(GitHubRepository));
            }
        }

        private Choice<OperationKind>? _operation;
        public Choice<OperationKind>? Operation
        {
            get => _operation;
            set => this.RaiseAndSetIfChanged(ref _operation, value);
        }

        private Choice<SourceKind>? _source;
        public Choice<SourceKind>? Source
        {
            get => _source;
            set
            {
                this.RaiseAndSetIfChanged(ref _source, value);
                this.RaisePropertyChanged(nameof(IsGitHubSource));
                this.RaisePropertyChanged(nameof(IsLocalSource));
            }
        }

        private SourceKind SourceKindValue => Source?.Value ?? SourceKind.GitHubRelease;

        public bool IsGitHubSource => SourceKindValue == SourceKind.GitHubRelease;

        public bool IsLocalSource => SourceKindValue != SourceKind.GitHubRelease;

        /// <summary>
        /// Repository used for downloads; derived from the selected product.
        /// </summary>
        public string GitHubRepository => (Product?.Value ?? ProductKind.Framework) == ProductKind.Mes
            ? "iplus-framework/iPlusMES"
            : "iplus-framework/iPlus";

        private GitHubRelease? _selectedRelease;
        public GitHubRelease? SelectedRelease
        {
            get => _selectedRelease;
            set => this.RaiseAndSetIfChanged(ref _selectedRelease, value);
        }

        private string _sourcePath = string.Empty;
        public string SourcePath
        {
            get => _sourcePath;
            set => this.RaiseAndSetIfChanged(ref _sourcePath, value);
        }

        private string _targetPath = string.Empty;
        public string TargetPath
        {
            get => _targetPath;
            set => this.RaiseAndSetIfChanged(ref _targetPath, value);
        }

        private string _platformFolder = string.Empty;
        public string PlatformFolder
        {
            get => _platformFolder;
            set => this.RaiseAndSetIfChanged(ref _platformFolder, value);
        }

        #endregion

        #region SQL

        private string _serverName = string.Empty;
        public string ServerName
        {
            get => _serverName;
            set => this.RaiseAndSetIfChanged(ref _serverName, value);
        }

        private Choice<SqlAuthMode>? _authMode;
        public Choice<SqlAuthMode>? AuthMode
        {
            get => _authMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _authMode, value);
                this.RaisePropertyChanged(nameof(IsSqlAuthentication));
            }
        }

        public bool IsSqlAuthentication => (AuthMode?.Value ?? SqlAuthMode.SqlServerAuthentication)
            == SqlAuthMode.SqlServerAuthentication;

        private string _userName = "sa";
        public string UserName
        {
            get => _userName;
            set => this.RaiseAndSetIfChanged(ref _userName, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string _databaseName = "iPlusV5";
        public string DatabaseName
        {
            get => _databaseName;
            set => this.RaiseAndSetIfChanged(ref _databaseName, value);
        }

        private string _connectionStatus = string.Empty;
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
        }

        #endregion

        #region Options

        private bool _applyInitialInstallScript = true;
        public bool ApplyInitialInstallScript
        {
            get => _applyInitialInstallScript;
            set => this.RaiseAndSetIfChanged(ref _applyInitialInstallScript, value);
        }

        private string _initialInstallScriptPath = string.Empty;
        public string InitialInstallScriptPath
        {
            get => _initialInstallScriptPath;
            set => this.RaiseAndSetIfChanged(ref _initialInstallScriptPath, value);
        }

        private bool _createDesktopShortcut = true;
        public bool CreateDesktopShortcut
        {
            get => _createDesktopShortcut;
            set => this.RaiseAndSetIfChanged(ref _createDesktopShortcut, value);
        }

        private bool _startAfterInstall = true;
        public bool StartAfterInstall
        {
            get => _startAfterInstall;
            set => this.RaiseAndSetIfChanged(ref _startAfterInstall, value);
        }

        #endregion

        #region State

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                this.RaiseAndSetIfChanged(ref _isBusy, value);
                this.RaisePropertyChanged(nameof(IsIdle));
            }
        }

        public bool IsIdle => !IsBusy;

        private double _progressFraction;
        public double ProgressFraction
        {
            get => _progressFraction;
            set => this.RaiseAndSetIfChanged(ref _progressFraction, value);
        }

        private bool _isProgressIndeterminate;
        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            set => this.RaiseAndSetIfChanged(ref _isProgressIndeterminate, value);
        }

        private string _statusMessage = "Ready.";
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        private string _resultMessage = string.Empty;
        public string ResultMessage
        {
            get => _resultMessage;
            set => this.RaiseAndSetIfChanged(ref _resultMessage, value);
        }

        private bool _hasResult;
        public bool HasResult
        {
            get => _hasResult;
            set => this.RaiseAndSetIfChanged(ref _hasResult, value);
        }

        private bool _resultIsSuccess;
        public bool ResultIsSuccess
        {
            get => _resultIsSuccess;
            set => this.RaiseAndSetIfChanged(ref _resultIsSuccess, value);
        }

        #endregion

        #region Commands

        public ReactiveCommand<RxVoid, RxVoid> RunCommand { get; }

        public ReactiveCommand<RxVoid, RxVoid> CancelCommand { get; }

        public ReactiveCommand<RxVoid, RxVoid> LoadReleasesCommand { get; }

        public ReactiveCommand<RxVoid, RxVoid> TestConnectionCommand { get; }

        public ReactiveCommand<RxVoid, RxVoid> BrowseSourceCommand { get; }

        public ReactiveCommand<RxVoid, RxVoid> BrowseTargetCommand { get; }

        public ReactiveCommand<RxVoid, RxVoid> BrowseScriptCommand { get; }

        #endregion

        private async Task RunAsync()
        {
            HasResult = false;
            ResultMessage = string.Empty;
            Log.Clear();

            InstallOptions options = new InstallOptions
            {
                Product = Product?.Value ?? ProductKind.Framework,
                Operation = Operation?.Value ?? OperationKind.Install,
                Source = SourceKindValue,
                SourcePath = SourcePath,
                GitHubRepository = GitHubRepository,
                GitHubTag = SelectedRelease?.TagName ?? string.Empty,
                TargetPath = TargetPath,
                PlatformFolder = PlatformFolder,
                ApplyInitialInstallScript = ApplyInitialInstallScript,
                InitialInstallScriptPath = InitialInstallScriptPath,
                CreateDesktopShortcut = CreateDesktopShortcut,
                StartAfterInstall = StartAfterInstall,
                Sql = new SqlConnectionSettings
                {
                    ServerName = ServerName,
                    AuthMode = AuthMode?.Value ?? SqlAuthMode.SqlServerAuthentication,
                    UserName = UserName,
                    Password = Password,
                    DatabaseName = DatabaseName
                }
            };

            _cancellation = new CancellationTokenSource();
            IsBusy = true;

            Progress<ProgressReport> progress = new Progress<ProgressReport>(report =>
            {
                StatusMessage = report.Message;
                if (report.Fraction < 0)
                {
                    IsProgressIndeterminate = true;
                }
                else
                {
                    IsProgressIndeterminate = false;
                    ProgressFraction = report.Fraction;
                }

                Log.Add(report.Message);
            });

            try
            {
                InstallResult result = await _installService.RunAsync(options, progress, _cancellation.Token);

                ResultIsSuccess = result.Success;
                ResultMessage = result.Message
                    + (string.IsNullOrEmpty(result.InstalledVersion)
                        ? string.Empty
                        : System.Environment.NewLine + "Version: " + result.InstalledVersion);
                HasResult = true;
                StatusMessage = result.Success ? "Finished." : "Failed.";
            }
            finally
            {
                IsBusy = false;
                IsProgressIndeterminate = false;
                _cancellation.Dispose();
                _cancellation = null;
            }
        }

        private void Cancel()
        {
            _cancellation?.Cancel();
            StatusMessage = "Cancelling…";
        }

        private async Task LoadReleasesAsync()
        {
            IsBusy = true;
            StatusMessage = "Loading releases from GitHub…";

            try
            {
                List<GitHubRelease> releases = await _githubService.GetReleasesAsync(
                    GitHubRepository, includePrereleases: true, CancellationToken.None);

                Releases.Clear();
                foreach (GitHubRelease release in releases)
                    Releases.Add(release);

                SelectedRelease = Releases.FirstOrDefault();
                StatusMessage = Releases.Count == 0
                    ? "No releases found."
                    : Releases.Count + " release(s) found.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Could not load releases: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task TestConnectionAsync()
        {
            IsBusy = true;
            ConnectionStatus = "Connecting…";

            try
            {
                SqlConnectionSettings settings = new SqlConnectionSettings
                {
                    ServerName = ServerName,
                    AuthMode = AuthMode?.Value ?? SqlAuthMode.SqlServerAuthentication,
                    UserName = UserName,
                    Password = Password,
                    DatabaseName = DatabaseName
                };

                (bool ok, string error) = await _sqlService.TestConnectionAsync(settings, CancellationToken.None);

                if (ok)
                {
                    string version = await _sqlService.GetServerVersionAsync(settings, CancellationToken.None);
                    ConnectionStatus = "Connected: " + version;
                }
                else
                {
                    ConnectionStatus = "Failed: " + error;
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Failed: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BrowseSourceAsync()
        {
            string? picked = SourceKindValue == SourceKind.LocalFolder
                ? await PickFolderAsync("Select the release folder")
                : await PickFileAsync("Select the release zip", "*.zip");

            if (!string.IsNullOrEmpty(picked))
                SourcePath = picked;
        }

        private async Task BrowseTargetAsync()
        {
            string? picked = await PickFolderAsync("Select the installation folder");
            if (!string.IsNullOrEmpty(picked))
                TargetPath = picked;
        }

        private async Task BrowseScriptAsync()
        {
            string? picked = await PickFileAsync("Select the initial-install script", "*.sql");
            if (!string.IsNullOrEmpty(picked))
                InitialInstallScriptPath = picked;
        }

        /// <summary>
        /// Opens a folder picker. Implemented in the view because it needs the
        /// active window; the view model only exposes the result.
        /// </summary>
        public Func<string, Task<string?>>? FolderPicker { get; set; }

        /// <summary>
        /// Opens a file picker.
        /// </summary>
        public Func<string, string, Task<string?>>? FilePicker { get; set; }

        private async Task<string?> PickFolderAsync(string title)
        {
            return FolderPicker == null ? null : await FolderPicker(title);
        }

        private async Task<string?> PickFileAsync(string title, string filter)
        {
            return FilePicker == null ? null : await FilePicker(title, filter);
        }

        private static string GetDefaultTargetPath()
        {
            string baseFolder = OperatingSystem.IsWindows()
                ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
                : System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

            return Path.Combine(baseFolder, "iPlus");
        }
    }
}

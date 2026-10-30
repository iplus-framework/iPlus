// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using gip.core.licensing;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel.Licensing
{
    /// <summary>
    /// Software-based licensing (offline signed license files).
    /// </summary>
    /// <remarks>
    /// This partial class adds offline, signed license file support to the
    /// existing <see cref="License"/> class. The public API surface of the V4
    /// class is preserved: <see cref="IsPackageLicensed"/>,
    /// <see cref="IsComponentLicensed"/>, <see cref="ActivateLicense"/>,
    /// <see cref="GenerateLicenseFile"/> and the remote-login code exchange keep
    /// working.
    ///
    /// Resolution order for a package license:
    /// <list type="number">
    /// <item>Signed license files that were activated into the database
    ///       (<c>VBSystem</c> rows with <c>SystemName = "VBP"</c>) — the V4
    ///       compatible path, now validated with RSA signatures.</item>
    /// <item>Signed license files dropped into the license folder
    ///       (<c>LicenseFolder</c>) — validated on the fly.</item>
    /// </list>
    /// </remarks>
    public sealed partial class License
    {
        #region Properties

        private string _LicenseFolder;

        /// <summary>
        /// Folder that is scanned for <c>*.gip</c> license files.
        /// Defaults to <c>&lt;application base&gt;/Licenses</c>.
        /// </summary>
        public string LicenseFolder
        {
            get
            {
                if (_LicenseFolder == null)
                    _LicenseFolder = Path.Combine(AppContext.BaseDirectory, "Licenses");
                return _LicenseFolder;
            }
            set
            {
                _LicenseFolder = value;
            }
        }

        private List<LicensePayload> _FileLicenses;

        /// <summary>
        /// All license files found in <see cref="LicenseFolder"/> that carry a
        /// valid signature. Invalid files are skipped (and reported through
        /// <see cref="LastLicenseErrors"/>).
        /// </summary>
        public IReadOnlyList<LicensePayload> FileLicenses
        {
            get
            {
                if (_FileLicenses == null)
                    LoadFileLicenses();
                return _FileLicenses;
            }
        }

        private List<string> _LastLicenseErrors;

        /// <summary>
        /// Diagnostics for license files that could not be loaded.
        /// </summary>
        public IReadOnlyList<string> LastLicenseErrors
        {
            get
            {
                if (_LastLicenseErrors == null)
                    _LastLicenseErrors = new List<string>();
                return _LastLicenseErrors;
            }
        }

        /// <summary>
        /// <c>true</c> when at least one valid license file is available
        /// (either activated in the database or present in the license folder).
        /// </summary>
        public bool HasFileLicense
        {
            get
            {
                return FileLicenses.Count > 0 || HasActivatedFileLicense();
            }
        }

        #endregion

        #region Loading

        /// <summary>
        /// Re-reads the license folder. Call after dropping a new license file.
        /// </summary>
        public void ReloadFileLicenses()
        {
            _FileLicenses = null;
            _ActivatedLicensePayloads = null;
            _LastLicenseErrors = null;
            InvalidateComponentLicenseCache();
            OnPropertyChanged(nameof(FileLicenses));
            OnPropertyChanged(nameof(HasFileLicense));
            OnPropertyChanged(nameof(HasFrameworkRuntimeLicense));
            OnPropertyChanged(nameof(LicensedMaxUsers));
            OnPropertyChanged(nameof(IsCommercialRuntime));
            ReportLicenseTypeChanged();
        }

        private void LoadFileLicenses()
        {
            _FileLicenses = new List<LicensePayload>();
            _LastLicenseErrors = new List<string>();

            string folder = LicenseFolder;
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return;

            LicenseVerifier verifier;
            try
            {
                verifier = CreateVerifierWithIssuerChain();
            }
            catch (InvalidOperationException e)
            {
                _LastLicenseErrors.Add(e.Message);
                return;
            }

            LicenseBindingContext context = CreateBindingContext();

            foreach (string file in Directory.EnumerateFiles(folder, "*" + LicenseFormat.FileExtension, SearchOption.TopDirectoryOnly))
            {
                try
                {
                    LicenseValidationResult result = verifier.VerifyFile(file, context);
                    if (result.IsValid)
                    {
                        _FileLicenses.Add(result.Payload!);
                    }
                    else
                    {
                        _LastLicenseErrors.Add(Path.GetFileName(file) + ": " + result.Error + " — " + result.Message);
                    }
                }
                catch (Exception e)
                {
                    _LastLicenseErrors.Add(Path.GetFileName(file) + ": " + e.Message);
                }
            }
        }

        /// <summary>
        /// Creates a verifier that knows the framework key and every developer key
        /// that is trusted by a <see cref="LicenseKind.DeveloperIssuer"/> license.
        /// </summary>
        /// <remarks>
        /// Two-level PKI chain: gip signs the <c>DeveloperIssuer</c> license, which
        /// embeds the developer's public key (<see cref="LicensePayload.IssuerPublicKey"/>).
        /// The developer signs <c>Product</c> licenses with the matching private key
        /// and stamps <see cref="LicensePayload.IssuerKeyId"/>. This method first
        /// collects the trusted issuer keys (from license files, activated database
        /// rows and <c>Issuers/*.pem</c>), then builds a verifier that accepts them.
        /// </remarks>
        private LicenseVerifier CreateVerifierWithIssuerChain()
        {
            Dictionary<string, RSA> issuerKeys = new Dictionary<string, RSA>(StringComparer.OrdinalIgnoreCase);

            // 1. Developer keys embedded in DeveloperIssuer licenses (signed by gip).
            LicenseVerifier frameworkOnly = FrameworkLicenseKeys.CreateVerifier();
            LicenseBindingContext context = CreateBindingContext();

            foreach (LicensePayload payload in EnumerateRawLicensePayloads(frameworkOnly, context))
            {
                if (payload.Kind != LicenseKind.DeveloperIssuer)
                    continue;
                if (string.IsNullOrEmpty(payload.IssuerKeyId) || string.IsNullOrEmpty(payload.IssuerPublicKey))
                    continue;

                try
                {
                    issuerKeys[payload.IssuerKeyId] = LicenseKeys.ImportPublicKey(payload.IssuerPublicKey);
                }
                catch (CryptographicException)
                {
                    // A malformed embedded key must not break license loading.
                }
            }

            // 2. Developer keys dropped into <LicenseFolder>/Issuers/*.pem.
            string issuerFolder = Path.Combine(LicenseFolder, "Issuers");
            if (Directory.Exists(issuerFolder))
            {
                foreach (string pem in Directory.EnumerateFiles(issuerFolder, "*.pem", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        string text = File.ReadAllText(pem);
                        string keyId = LicenseKeys.ComputeKeyIdFromText(text);
                        if (!issuerKeys.ContainsKey(keyId))
                            issuerKeys[keyId] = LicenseKeys.ImportPublicKey(text);
                    }
                    catch (Exception)
                    {
                        // Ignore unreadable key files.
                    }
                }
            }

            if (issuerKeys.Count == 0)
                return frameworkOnly;

            return FrameworkLicenseKeys.CreateVerifier(keyId =>
            {
                RSA key;
                return issuerKeys.TryGetValue(keyId, out key) ? key : null;
            });
        }

        /// <summary>
        /// Enumerates every license payload that can be verified with the framework
        /// key alone (license folder + activated database rows). Used to discover
        /// trusted developer keys before the full verification pass.
        /// </summary>
        private IEnumerable<LicensePayload> EnumerateRawLicensePayloads(LicenseVerifier frameworkOnly, LicenseBindingContext context)
        {
            string folder = LicenseFolder;
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*" + LicenseFormat.FileExtension, SearchOption.TopDirectoryOnly))
                {
                    LicensePayload payload = TryVerifyFile(frameworkOnly, file, context);
                    if (payload != null)
                        yield return payload;
                }
            }

            List<VBSystem> rows;
            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                // byte[].Length is not translatable by EF Core, so the length check
                // is done in memory. VBSystem only holds a handful of rows.
                rows = _Database.VBSystem
                    .Where(c => c.SystemName == SN_PackIssuer || c.SystemName == SN_PackLicense)
                    .AsEnumerable()
                    .Where(c => c.SystemInternal != null && c.SystemInternal.Length > 0)
                    .ToList();
            }

            foreach (VBSystem row in rows)
            {
                LicensePayload payload = TryVerifyText(frameworkOnly, Encoding.UTF8.GetString(row.SystemInternal), context);
                if (payload != null)
                    yield return payload;
            }
        }

        private static LicensePayload TryVerifyFile(LicenseVerifier verifier, string path, LicenseBindingContext context)
        {
            try
            {
                LicenseValidationResult result = verifier.VerifyFile(path, context);
                return result.IsValid ? result.Payload : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static LicensePayload TryVerifyText(LicenseVerifier verifier, string text, LicenseBindingContext context)
        {
            try
            {
                LicenseValidationResult result = verifier.Verify(text, context);
                return result.IsValid ? result.Payload : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Builds the binding context (database + machine fingerprint) for the
        /// current installation.
        /// </summary>
        public LicenseBindingContext CreateBindingContext()
        {
            LicenseBindingContext context = new LicenseBindingContext
            {
                MachineFingerprint = MachineFingerprint.ComputeCurrent()
            };

            try
            {
                string dbCode = GetDBCode(_Database);
                if (!string.IsNullOrEmpty(dbCode))
                    context.DatabaseFingerprint = DatabaseFingerprint.FromLegacyChecksumCode(dbCode);
            }
            catch (Exception e)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("License", "CreateBindingContext", e.Message);
            }

            return context;
        }

        #endregion

        #region Package checks (file based)

        /// <summary>
        /// Returns <c>true</c> when the given package is covered by a valid
        /// license file (activated in the database or present in the license folder).
        /// </summary>
        public bool IsPackageLicensedByFile(ACPackage acPackage)
        {
            if (acPackage == null)
                return false;

            // System packages are always available (framework core).
            if (acPackage.ACPackageName == Const.PackName_VarioSystem
                || acPackage.ACPackageName == Const.PackName_System)
                return true;

            foreach (LicensePayload payload in FileLicenses)
            {
                if (payload.CoversPackage(acPackage.ACPackageName))
                    return true;
            }

            return IsPackageLicensedByActivatedLicense(acPackage.ACPackageName);
        }

        /// <summary>
        /// Returns <c>true</c> when the current installation holds a developer
        /// license (issuer or end-user development rights) from a license file.
        /// </summary>
        public bool IsDeveloperByFile
        {
            get
            {
                return FileLicenses.Any(p =>
                    p.Kind == LicenseKind.DeveloperIssuer || p.Kind == LicenseKind.DeveloperEndUser);
            }
        }

        /// <summary>
        /// Returns <c>true</c> when the current installation holds a developer
        /// license with the right to issue product licenses.
        /// </summary>
        public bool IsDeveloperWithIssuingRightsByFile
        {
            get
            {
                return FileLicenses.Any(p => p.Kind == LicenseKind.DeveloperIssuer);
            }
        }

        #endregion

        #region Activation

        /// <summary>
        /// Activates a signed license file: validates it against the current
        /// database/machine binding and stores it in the database
        /// (<c>VBSystem</c>), so that it survives restarts and is visible to
        /// other clients.
        /// </summary>
        /// <param name="licenseText">Complete license file content.</param>
        /// <param name="error">Failure description when the method returns <c>false</c>.</param>
        public bool ActivateLicenseFile(string licenseText, out string error)
        {
            error = string.Empty;

            LicenseVerifier verifier;
            try
            {
                verifier = CreateVerifierWithIssuerChain();
            }
            catch (InvalidOperationException e)
            {
                error = e.Message;
                return false;
            }

            LicenseValidationResult result = verifier.Verify(licenseText, CreateBindingContext());
            if (!result.IsValid)
            {
                error = result.Error + ": " + result.Message;
                return false;
            }

            LicensePayload payload = result.Payload!;

            VBSystem sys = new VBSystem
            {
                VBSystemID = Guid.NewGuid(),
                SystemName = payload.Kind == LicenseKind.Product || payload.Kind == LicenseKind.FrameworkRuntime
                    ? SN_PackLicense
                    : SN_PackIssuer,
                CustomerName = payload.CustomerName,
                Company = payload.IssuerName,
                ProjectNo = payload.ProjectNo,
                // Store the raw signed file so that it can be re-verified later
                // without trusting the database row.
                SystemInternal = Encoding.UTF8.GetBytes(licenseText),
                SystemInternal2 = payload.LicenseId.ToString(),
                SystemCommonPublic = payload.DatabaseFingerprint ?? string.Empty,
                // NOT NULL in the database; only the V4 signature scheme used it.
                SystemCommon = Array.Empty<byte>()
            };

            try
            {
                using (ACMonitor.Lock(_Database.QueryLock_1X000))
                {
                    // Dedupe on the license id, NOT on customer name: two different
                    // developers may license the same customer, and re-activating the
                    // same file must replace the row instead of duplicating it.
                    string licenseId = payload.LicenseId.ToString();
                    VBSystem existing = _Database.VBSystem
                        .FirstOrDefault(c => c.SystemInternal2 == licenseId);
                    if (existing != null)
                        _Database.Remove(existing);
                }

                _Database.VBSystem.Add(sys);
                _Database.ACSaveChanges();
            }
            catch (Exception e)
            {
                error = "Could not store the license in the database: " + e.Message;
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("License", "ActivateLicenseFile", e.Message);
                return false;
            }

            NeedRestart = true;
            // A freshly activated license may resolve the problem: restart the
            // grace clock so the user gets the full grace period again if it does not.
            ClearLicenseGracePeriod();
            ReloadFileLicenses();
            return true;
        }

        /// <summary>
        /// Removes an activated license (a <c>VBSystem</c> row). The license takes effect until the
        /// next restart; removing the framework runtime license turns the installation back into
        /// an open edition.
        /// </summary>
        public bool RemoveActivatedLicense(VBSystem licenseRow, out string error)
        {
            error = string.Empty;
            if (licenseRow == null)
            {
                error = "No license selected.";
                return false;
            }

            try
            {
                using (ACMonitor.Lock(_Database.QueryLock_1X000))
                {
                    _Database.Remove(licenseRow);
                }
                _Database.ACSaveChanges();
            }
            catch (Exception e)
            {
                error = "Could not remove the license from the database: " + e.Message;
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("License", "RemoveActivatedLicense", e.Message);
                return false;
            }

            NeedRestart = true;
            ReloadFileLicenses();
            return true;
        }

        /// <summary>
        /// Activates a license file from disk.
        /// </summary>
        public bool ActivateLicenseFileFromPath(string path, out string error)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                error = "License file not found: " + path;
                return false;
            }

            return ActivateLicenseFile(LicenseFile.Read(path), out error);
        }

        /// <summary>
        /// Returns <c>true</c> when at least one license file was activated into
        /// the database.
        /// </summary>
        public bool HasActivatedFileLicense()
        {
            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                return _Database.VBSystem
                    .Where(c => c.SystemName == SN_PackIssuer || c.SystemName == SN_PackLicense)
                    .AsEnumerable()
                    .Any(c => c.SystemInternal != null && c.SystemInternal.Length > 0);
            }
        }

        private bool IsPackageLicensedByActivatedLicense(string packageName)
        {
            List<VBSystem> rows;
            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                rows = _Database.VBSystem
                    .Where(c => c.SystemName == SN_PackLicense && c.SystemInternal != null)
                    .ToList();
            }

            if (rows.Count == 0)
                return false;

            LicenseVerifier verifier;
            try
            {
                verifier = CreateVerifierWithIssuerChain();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            LicenseBindingContext context = CreateBindingContext();
            foreach (VBSystem row in rows)
            {
                try
                {
                    string text = Encoding.UTF8.GetString(row.SystemInternal);
                    LicenseValidationResult result = verifier.Verify(text, context);
                    if (result.IsValid && result.Payload!.CoversPackage(packageName))
                        return true;
                }
                catch (Exception)
                {
                    // A corrupt row must not break package loading.
                }
            }

            return false;
        }

        #endregion

        #region Framework runtime license

        /// <summary>
        /// Returns the active framework runtime license (issued by gip, bound to
        /// this database, granting <see cref="LicensePayload.MaxUsers"/> concurrent
        /// users), or <c>null</c> when none is present.
        /// </summary>
        /// <remarks>
        /// Looks in the license folder first, then in the license files that were
        /// activated into the database.
        /// </remarks>
        public LicensePayload FrameworkRuntimeLicense
        {
            get
            {
                LicensePayload runtime = FileLicenses.FirstOrDefault(p => p.Kind == LicenseKind.FrameworkRuntime);
                if (runtime != null)
                    return runtime;
                return ActivatedLicensePayloads.FirstOrDefault(p => p.Kind == LicenseKind.FrameworkRuntime);
            }
        }

        private List<LicensePayload> _ActivatedLicensePayloads;

        /// <summary>
        /// All license files that were activated into the database and still pass
        /// verification against the current database/machine binding.
        /// </summary>
        public IReadOnlyList<LicensePayload> ActivatedLicensePayloads
        {
            get
            {
                if (_ActivatedLicensePayloads == null)
                    LoadActivatedLicensePayloads();
                return _ActivatedLicensePayloads;
            }
        }

        private void LoadActivatedLicensePayloads()
        {
            _ActivatedLicensePayloads = new List<LicensePayload>();

            List<VBSystem> rows;
            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                rows = _Database.VBSystem
                    .Where(c => c.SystemName == SN_PackIssuer || c.SystemName == SN_PackLicense)
                    .AsEnumerable()
                    .Where(c => c.SystemInternal != null && c.SystemInternal.Length > 0)
                    .ToList();
            }

            if (rows.Count == 0)
                return;

            LicenseVerifier verifier;
            try
            {
                verifier = CreateVerifierWithIssuerChain();
            }
            catch (InvalidOperationException)
            {
                return;
            }

            LicenseBindingContext context = CreateBindingContext();
            foreach (VBSystem row in rows)
            {
                try
                {
                    string text = Encoding.UTF8.GetString(row.SystemInternal);
                    LicenseValidationResult result = verifier.Verify(text, context);
                    if (result.IsValid)
                        _ActivatedLicensePayloads.Add(result.Payload!);
                }
                catch (Exception)
                {
                    // A corrupt row must not break package loading.
                }
            }
        }

        /// <summary>
        /// <c>true</c> when a valid framework runtime license is present.
        /// A product license is only effective when this is <c>true</c>.
        /// </summary>
        public bool HasFrameworkRuntimeLicense
        {
            get
            {
                return FrameworkRuntimeLicense != null;
            }
        }

        /// <summary>
        /// Maximum number of concurrent users granted by the framework runtime
        /// license. <c>0</c> means unlimited (no runtime license present).
        /// </summary>
        public int LicensedMaxUsers
        {
            get
            {
                LicensePayload runtime = FrameworkRuntimeLicense;
                if (runtime == null || !runtime.MaxUsers.HasValue)
                    return 0;
                return runtime.MaxUsers.Value;
            }
        }

        /// <summary>
        /// <c>true</c> when the installation runs under a commercial runtime
        /// license. Used by the GPL/commercial mixing rule.
        /// </summary>
        /// <remarks>
        /// The framework runtime license <b>is</b> the commercial edition: gip sells
        /// it per concurrent user. Without it the installation runs under the open
        /// (GPL-3) edition, where every package is allowed.
        /// </remarks>
        public bool IsCommercialRuntime
        {
            get
            {
                return HasFrameworkRuntimeLicense;
            }
        }

        /// <summary>
        /// Applies the GPL/commercial mixing rule to a package.
        /// </summary>
        /// <remarks>
        /// A package may run under a commercial runtime license only when its
        /// license expression contains <c>Commercial</c> or is permissive
        /// (MIT/Apache/BSD). Pure copyleft packages (GPL-3) are blocked, because
        /// mixing GPL code with commercially licensed iPlus would violate the GPL.
        /// Commercial-only packages additionally need a product license.
        /// Unknown expressions fail open so that a package is never blocked just
        /// because its metadata could not be read.
        ///
        /// A <b>developer installation</b> (a <see cref="LicenseKind.DeveloperIssuer"/>
        /// or <see cref="LicenseKind.DeveloperEndUser"/> license file) is a
        /// development environment: everything may run, so that the developer can
        /// build and test both GPL and commercial packages. The mixing rule only
        /// applies to end-customer installations.
        /// </remarks>
        public bool IsPackageAllowedByLicenseExpression(ACPackage acPackage)
        {
            if (acPackage == null)
                return false;

            if (IsDeveloperByFile)
                return true;

            LicenseCompatibility compatibility = PackageLicenseRegistry.GetCompatibility(acPackage.ACPackageName);

            if (!IsCommercialRuntime)
            {
                // Open edition: product licenses are ineffective without a
                // framework runtime license, so only commercial-only packages
                // are blocked.
                return compatibility != LicenseCompatibility.CommercialOnly;
            }

            switch (compatibility)
            {
                case LicenseCompatibility.OpenOnly:
                    // Commercial edition: GPL-3 packages must not be mixed in.
                    return false;
                case LicenseCompatibility.CommercialOnly:
                    return IsPackageLicensedByFile(acPackage);
                default:
                    // Both (dual-licensed / permissive) and Unknown (fail-open)
                    // are covered by the framework runtime license.
                    return true;
            }
        }

        #endregion

        #region Grace period

        /// <summary>
        /// Default grace period before the client starts warning about a missing
        /// or invalid license (V4 parity: 30 minutes).
        /// </summary>
        public static readonly TimeSpan DefaultWarningGracePeriod = LicenseGracePeriod.DefaultWarningPeriod;

        /// <summary>
        /// Default grace period before the server blocks <c>ACUrlCommand</c>
        /// (V4 parity: 1–2 days). The end user gets time to resolve the license
        /// issue before the program stops working.
        /// </summary>
        public static readonly TimeSpan DefaultBlockGracePeriod = LicenseGracePeriod.DefaultBlockPeriod;

        private LicenseGracePeriod _GracePeriod;

        /// <summary>
        /// The two-stage grace period tracker. Created lazily and restored from
        /// <see cref="GracePeriodFilePath"/> so that a restart does not reset the
        /// clock.
        /// </summary>
        public LicenseGracePeriod GracePeriod
        {
            get
            {
                lock (_LPLock)
                {
                    if (_GracePeriod == null)
                    {
                        _GracePeriod = new LicenseGracePeriod(WarningGracePeriod, BlockGracePeriod);
                        _GracePeriod.Restore(ReadPersistedGraceStart());
                    }
                    return _GracePeriod;
                }
            }
        }

        /// <summary>
        /// Point in time when the current license problem was first detected.
        /// <c>null</c> while the installation is properly licensed.
        /// </summary>
        public DateTime? LicenseGraceStartUtc
        {
            get
            {
                return GracePeriod.StartUtc;
            }
        }

        /// <summary>
        /// Grace period after which the client shows a warning.
        /// </summary>
        public TimeSpan WarningGracePeriod
        {
            get { return _WarningGracePeriod; }
            set
            {
                _WarningGracePeriod = value;
                ResetGracePeriodTracker();
            }
        }
        private TimeSpan _WarningGracePeriod = DefaultWarningGracePeriod;

        /// <summary>
        /// Grace period after which the server blocks <c>ACUrlCommand</c>.
        /// </summary>
        public TimeSpan BlockGracePeriod
        {
            get { return _BlockGracePeriod; }
            set
            {
                _BlockGracePeriod = value;
                ResetGracePeriodTracker();
            }
        }
        private TimeSpan _BlockGracePeriod = DefaultBlockGracePeriod;

        private void ResetGracePeriodTracker()
        {
            lock (_LPLock)
            {
                DateTime? start = _GracePeriod != null ? _GracePeriod.StartUtc : null;
                TimeSpan warning = _WarningGracePeriod;
                TimeSpan block = _BlockGracePeriod;
                if (block < warning)
                    block = warning;
                _GracePeriod = new LicenseGracePeriod(warning, block);
                _GracePeriod.Restore(start);
            }
        }

        /// <summary>
        /// <c>true</c> when the warning grace period has elapsed and the client
        /// should show a message. The system is still fully usable.
        /// </summary>
        public bool IsLicenseWarningDue
        {
            get
            {
                return GracePeriod.IsWarningDue();
            }
        }

        /// <summary>
        /// <c>true</c> when the block grace period has elapsed. From this point on
        /// <see cref="IsComponentLicensed"/> returns
        /// <see cref="ComponentLicense.False"/> for unlicensed packages, so
        /// <c>ACUrlCommand</c> returns <c>null</c> and the program stops working.
        /// </summary>
        public bool IsLicenseBlockDue
        {
            get
            {
                return GracePeriod.IsBlockDue();
            }
        }

        /// <summary>
        /// Starts the grace period clock when a license problem is detected.
        /// Idempotent — the first detection wins, so repeated checks do not extend
        /// the grace period. The start time is persisted so that a restart does not
        /// reset the clock.
        /// </summary>
        public void StartLicenseGracePeriod()
        {
            if (GracePeriod.Start())
                WritePersistedGraceStart(GracePeriod.StartUtc);
        }

        /// <summary>
        /// Clears the grace period clock (license problem resolved).
        /// </summary>
        public void ClearLicenseGracePeriod()
        {
            GracePeriod.Clear();
            WritePersistedGraceStart(null);
        }

        /// <summary>
        /// File that stores the grace-period start time across restarts.
        /// Lives next to the license files so that it is removed together with them.
        /// </summary>
        public string GracePeriodFilePath
        {
            get
            {
                return Path.Combine(LicenseFolder, ".licensegrace");
            }
        }

        private DateTime? ReadPersistedGraceStart()
        {
            try
            {
                string path = GracePeriodFilePath;
                if (!File.Exists(path))
                    return null;
                string text = File.ReadAllText(path).Trim();
                long ticks;
                if (!long.TryParse(text, out ticks))
                    return null;
                DateTime value = new DateTime(ticks, DateTimeKind.Utc);
                // Guard against a clock that was set back: never trust a start time
                // in the future.
                if (value > DateTime.UtcNow)
                    return null;
                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void WritePersistedGraceStart(DateTime? startUtc)
        {
            try
            {
                string path = GracePeriodFilePath;
                if (!startUtc.HasValue)
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    return;
                }

                string folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                File.WriteAllText(path, startUtc.Value.Ticks.ToString());
            }
            catch (Exception e)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("License", "WritePersistedGraceStart", e.Message);
            }
        }

        #endregion

        #region Issuing (developer side)
        /// <summary>
        /// Issues a product license file for the given packages, bound to the
        /// current database. Requires a developer license with issuing rights.
        /// </summary>
        /// <param name="privateKeyText">The developer's private key (PEM or legacy XML).</param>
        /// <param name="customerName">Customer the license is issued to.</param>
        /// <param name="projectNo">Project/order number.</param>
        /// <param name="packages">Package names covered by the license.</param>
        /// <param name="outputPath">Path of the <c>*.gip</c> file to write.</param>
        /// <param name="error">Failure description when the method returns <c>false</c>.</param>
        public bool IssueProductLicenseFile(
            string privateKeyText,
            string customerName,
            string projectNo,
            IEnumerable<string> packages,
            string outputPath,
            out string error)
        {
            return IssueProductLicenseFile(
                privateKeyText, customerName, projectNo, packages, outputPath, null, out error);
        }

        /// <summary>
        /// Issues a product license file for the given packages, bound to the
        /// <b>customer's</b> database. Requires a developer license with issuing rights.
        /// </summary>
        /// <remarks>
        /// A product license is issued on the developer's installation but activated
        /// on the customer's database. The fingerprint therefore must describe the
        /// customer's database, not the developer's own one. The customer's
        /// fingerprint is derived from the identification code that
        /// <c>BSOLicenseActivation</c> exports in the activation data file
        /// (see <see cref="DatabaseFingerprint.FromLegacyChecksumCode"/>).
        /// </remarks>
        /// <param name="privateKeyText">The developer's private key (PEM or legacy XML).</param>
        /// <param name="customerName">Customer the license is issued to.</param>
        /// <param name="projectNo">Project/order number.</param>
        /// <param name="packages">Package names covered by the license.</param>
        /// <param name="outputPath">Path of the <c>*.gip</c> file to write.</param>
        /// <param name="databaseFingerprint">
        /// Fingerprint of the customer's database. When empty, the fingerprint of the
        /// current (developer's) database is used.
        /// </param>
        /// <param name="error">Failure description when the method returns <c>false</c>.</param>
        public bool IssueProductLicenseFile(
            string privateKeyText,
            string customerName,
            string projectNo,
            IEnumerable<string> packages,
            string outputPath,
            string databaseFingerprint,
            out string error)
        {
            error = string.Empty;

            if (!IsDeveloperWithIssuingRightsByFile)
            {
                error = "No developer license with issuing rights is active.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(privateKeyText))
            {
                error = "Private key is empty.";
                return false;
            }

            List<string> packageList = packages?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToList() ?? new List<string>();

            if (packageList.Count == 0)
            {
                error = "At least one package is required.";
                return false;
            }

            try
            {
                // Bind the license to the customer's database. When no fingerprint was
                // supplied (e.g. the developer issues a license for their own
                // installation), fall back to the local database.
                string fingerprint = databaseFingerprint;
                if (string.IsNullOrWhiteSpace(fingerprint))
                    fingerprint = CreateBindingContext().DatabaseFingerprint;

                LicensePayload payload = new LicensePayload
                {
                    Kind = LicenseKind.Product,
                    CustomerName = customerName,
                    ProjectNo = projectNo ?? string.Empty,
                    IssuerName = LicensedTo,
                    Packages = packageList,
                    DatabaseFingerprint = fingerprint
                };

                LicenseIssuer issuer = LicenseIssuer.FromPrivateKeyText(privateKeyText);

                // Embed the developer's gip-signed DeveloperIssuer license, so the
                // customer can verify the chain without installing any developer key.
                payload.IssuerLicense = FindIssuerLicenseText(issuer.GetKeyId());
                if (payload.IssuerLicense == null)
                {
                    error = "No DeveloperIssuer license matching this private key (key id "
                        + issuer.GetKeyId() + ") was found in the license folder or database. "
                        + "Select the private key that belongs to your developer license.";
                    return false;
                }

                // Stamp the issuer key id so that the verifier can resolve the
                // developer's public key through the two-level PKI chain.
                string text = issuer.IssueWithKeyId(payload);
                string directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(outputPath, text, new UTF8Encoding(false));
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("License", "IssueProductLicenseFile", e.Message);
                return false;
            }
        }


        /// <summary>
        /// Returns the text of the valid <see cref="LicenseKind.DeveloperIssuer"/> license
        /// whose embedded public key has the given key id, or <c>null</c>.
        /// </summary>
        private string FindIssuerLicenseText(string keyId)
        {
            LicenseVerifier frameworkOnly = FrameworkLicenseKeys.CreateVerifier();
            List<string> candidates = new List<string>();

            string folder = LicenseFolder;
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*" + LicenseFormat.FileExtension, SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        candidates.Add(LicenseFile.Read(file));
                    }
                    catch (Exception)
                    {
                        // Unreadable files are reported by the regular license loading.
                    }
                }
            }

            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                candidates.AddRange(_Database.VBSystem
                    .Where(c => c.SystemName == SN_PackIssuer)
                    .AsEnumerable()
                    .Where(c => c.SystemInternal != null && c.SystemInternal.Length > 0)
                    .Select(c => Encoding.UTF8.GetString(c.SystemInternal)));
            }

            foreach (string text in candidates)
            {
                LicenseValidationResult result = frameworkOnly.Verify(text);
                if (!result.IsValid
                    || result.Payload.Kind != LicenseKind.DeveloperIssuer
                    || string.IsNullOrEmpty(result.Payload.IssuerPublicKey))
                    continue;

                try
                {
                    if (string.Equals(LicenseKeys.ComputeKeyIdFromText(result.Payload.IssuerPublicKey), keyId, StringComparison.OrdinalIgnoreCase))
                        return text;
                }
                catch (CryptographicException)
                {
                }
            }
            return null;
        }

        #endregion
    }
}

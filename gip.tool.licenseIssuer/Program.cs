// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using gip.core.licensing;

namespace gip.tool.licenseIssuer
{
    /// <summary>
    /// Command line front end for <see cref="LicenseIssuer"/> and <see cref="LicenseVerifier"/>.
    /// </summary>
    /// <remarks>
    /// Replaces the dongle-bound V4 license manager for the parts that do not need
    /// a UI: generating key pairs, issuing developer/product licenses and verifying
    /// a license file offline.
    /// </remarks>
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            RootCommand root = new RootCommand(
                "iPlus license issuer — creates and verifies offline, signed license files (*.gip).");

            root.Add(CreateKeyPairCommand());
            root.Add(IssueDeveloperCommand());
            root.Add(IssueRuntimeCommand());
            root.Add(IssueProductCommand());
            root.Add(VerifyCommand());
            root.Add(ShowCommand());
            root.Add(FingerprintCommand());

            return root.Parse(args).InvokeAsync();
        }

        #region Commands

        private static Command CreateKeyPairCommand()
        {
            Option<string> outputDir = new Option<string>("--output-dir", "-o")
            {
                Description = "Directory where the key pair is written.",
                DefaultValueFactory = _ => "."
            };
            Option<string> name = new Option<string>("--name", "-n")
            {
                Description = "Base file name of the key pair (without extension).",
                DefaultValueFactory = _ => "issuer"
            };
            Option<int> keySize = new Option<int>("--key-size")
            {
                Description = "RSA key size in bits.",
                DefaultValueFactory = _ => 2048
            };

            Command command = new Command("create-keypair", "Creates a new RSA key pair (private + public PEM).");
            command.Add(outputDir);
            command.Add(name);
            command.Add(keySize);

            command.SetAction(parseResult =>
            {
                string dir = parseResult.GetValue(outputDir)!;
                string baseName = parseResult.GetValue(name)!;
                int size = parseResult.GetValue(keySize);

                Directory.CreateDirectory(dir);
                using var key = LicenseKeys.CreateKeyPair(size);

                string privatePath = Path.Combine(dir, baseName + "-private.pem");
                string publicPath = Path.Combine(dir, baseName + "-public.pem");
                LicenseKeys.SaveKeyToFile(key, privatePath, privateKey: true);
                LicenseKeys.SaveKeyToFile(key, publicPath, privateKey: false);

                Console.WriteLine("Private key: " + privatePath);
                Console.WriteLine("Public key : " + publicPath);
                Console.WriteLine();
                Console.WriteLine("Keep the private key offline. Register the public key with the");
                Console.WriteLine("framework (embedded resource or IPLUS_LICENSE_PUBLIC_KEY).");
                return 0;
            });

            return command;
        }

        private static Command IssueDeveloperCommand()
        {
            Option<string> privateKey = RequiredKeyOption("--private-key", "Path to the issuer private key (PEM or legacy XML).");
            Option<string> output = RequiredPathOption("--output", "-o", "Path of the license file to write.");
            Option<string> customer = RequiredTextOption("--customer", "Customer / company name.");
            Option<string> projectNo = new Option<string>("--project-no") { Description = "Project or order number." };
            Option<string> issuerName = new Option<string>("--issuer-name") { Description = "Name of the issuing company.", DefaultValueFactory = _ => "gip" };
            Option<string> expires = new Option<string>("--expires") { Description = "Optional expiry date (yyyy-MM-dd)." };
            Option<string> machine = new Option<string>("--machine-fingerprint") { Description = "Optional machine fingerprint to bind the license to." };
            Option<string> remarks = new Option<string>("--remarks") { Description = "Free-form remarks." };
            Option<bool> endUser = new Option<bool>("--end-user") { Description = "Issue an end-user development license instead of an issuer license." };
            Option<string> developerPublicKey = new Option<string>("--developer-public-key")
            {
                Description = "Path to the developer's public key (PEM). Embedded into the license so that the framework can verify the developer's product licenses offline. Required for issuer licenses."
            };

            Option<string[]> packageNamespaces = new Option<string[]>("--package-namespace")
            {
                Description = "Package namespace (first segment of the ACPackageName, e.g. 'acme') or full package name the developer may issue product licenses for. Repeatable. Omit for all packages.",
                AllowMultipleArgumentsPerToken = true
            };

            Command command = new Command("issue-developer", "Issues a developer license (issuer or end-user development rights).");
            command.Add(privateKey);
            command.Add(output);
            command.Add(customer);
            command.Add(projectNo);
            command.Add(issuerName);
            command.Add(expires);
            command.Add(machine);
            command.Add(remarks);
            command.Add(endUser);
            command.Add(developerPublicKey);
            command.Add(packageNamespaces);

            command.SetAction(parseResult =>
            {
                bool isEndUser = parseResult.GetValue(endUser);
                LicensePayload payload = new LicensePayload
                {
                    Kind = isEndUser ? LicenseKind.DeveloperEndUser : LicenseKind.DeveloperIssuer,
                    CustomerName = parseResult.GetValue(customer)!,
                    ProjectNo = parseResult.GetValue(projectNo) ?? string.Empty,
                    IssuerName = parseResult.GetValue(issuerName)!,
                    MachineFingerprint = parseResult.GetValue(machine) ?? string.Empty,
                    Remarks = parseResult.GetValue(remarks) ?? string.Empty
                };

                string[]? namespaces = parseResult.GetValue(packageNamespaces);
                if (namespaces != null)
                    payload.PackageNamespaces = namespaces.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToList();

                if (!TryApplyExpiry(parseResult.GetValue(expires), payload, out string error))
                    return Fail(error);

                // An issuer license must carry the developer's public key, otherwise
                // the framework cannot verify the product licenses that the developer
                // signs with the matching private key.
                if (!isEndUser)
                {
                    string? publicKeyPath = parseResult.GetValue(developerPublicKey);
                    if (string.IsNullOrWhiteSpace(publicKeyPath))
                        return Fail("--developer-public-key is required for issuer licenses.");
                    if (!File.Exists(publicKeyPath))
                        return Fail("Developer public key file not found: " + publicKeyPath);

                    string publicKeyText = File.ReadAllText(publicKeyPath);
                    payload.IssuerPublicKey = publicKeyText;
                    payload.IssuerKeyId = LicenseKeys.ComputeKeyIdFromText(publicKeyText);
                }

                return IssueAndReport(parseResult.GetValue(privateKey)!, parseResult.GetValue(output)!, payload);
            });

            return command;
        }

        private static Command IssueRuntimeCommand()
        {
            Option<string> privateKey = RequiredKeyOption("--private-key", "Path to gip's issuer private key (PEM or legacy XML).");
            Option<string> output = RequiredPathOption("--output", "-o", "Path of the license file to write.");
            Option<string> customer = RequiredTextOption("--customer", "Customer / company name.");
            Option<string> projectNo = new Option<string>("--project-no") { Description = "Project or order number." };
            Option<string> issuerName = new Option<string>("--issuer-name") { Description = "Name of the issuing company.", DefaultValueFactory = _ => "gip" };
            Option<int> maxUsers = new Option<int>("--max-users") { Description = "Number of concurrent users (sessions) granted by the runtime license." };
            Option<string> database = new Option<string>("--database-fingerprint") { Description = "Database fingerprint the license is bound to." };
            Option<string> activationFile = new Option<string>("--activation-file", "-a") { Description = "Customer's activation data file; the database fingerprint is taken from it (alternative to --database-fingerprint)." };
            Option<string> expires = new Option<string>("--expires") { Description = "Optional expiry date (yyyy-MM-dd)." };
            Option<string> remarks = new Option<string>("--remarks") { Description = "Free-form remarks." };

            Command command = new Command("issue-runtime", "Issues a framework runtime license (commercial iPlus edition, per concurrent user).");
            command.Add(privateKey);
            command.Add(output);
            command.Add(customer);
            command.Add(projectNo);
            command.Add(issuerName);
            command.Add(maxUsers);
            command.Add(database);
            command.Add(activationFile);
            command.Add(expires);
            command.Add(remarks);

            command.SetAction(parseResult =>
            {
                int users = parseResult.GetValue(maxUsers);
                if (users <= 0)
                    return Fail("--max-users must be greater than 0 for a runtime license.");

                if (!TryResolveFingerprint(parseResult.GetValue(database), parseResult.GetValue(activationFile), out string fingerprint, out string fingerprintError))
                    return Fail(fingerprintError);

                LicensePayload payload = new LicensePayload
                {
                    Kind = LicenseKind.FrameworkRuntime,
                    CustomerName = parseResult.GetValue(customer)!,
                    ProjectNo = parseResult.GetValue(projectNo) ?? string.Empty,
                    IssuerName = parseResult.GetValue(issuerName)!,
                    DatabaseFingerprint = fingerprint,
                    Remarks = parseResult.GetValue(remarks) ?? string.Empty,
                    MaxUsers = users
                };

                if (!TryApplyExpiry(parseResult.GetValue(expires), payload, out string error))
                    return Fail(error);

                return IssueAndReport(parseResult.GetValue(privateKey)!, parseResult.GetValue(output)!, payload);
            });

            return command;
        }

        private static Command IssueProductCommand()
        {
            Option<string> privateKey = RequiredKeyOption("--private-key", "Path to the issuer private key (PEM or legacy XML).");
            Option<string> output = RequiredPathOption("--output", "-o", "Path of the license file to write.");
            Option<string> customer = RequiredTextOption("--customer", "Customer / company name.");
            Option<string> projectNo = new Option<string>("--project-no") { Description = "Project or order number." };
            Option<string> issuerName = new Option<string>("--issuer-name") { Description = "Name of the issuing company." };
            Option<string[]> packages = new Option<string[]>("--package", "-p")
            {
                Description = "Package name (ACPackageName) covered by the license. Repeatable.",
                AllowMultipleArgumentsPerToken = true
            };
            Option<string> database = new Option<string>("--database-fingerprint") { Description = "Database fingerprint the license is bound to." };
            Option<string> activationFile = new Option<string>("--activation-file", "-a") { Description = "Customer's activation data file; the database fingerprint is taken from it (alternative to --database-fingerprint)." };
            Option<string> machine = new Option<string>("--machine-fingerprint") { Description = "Optional machine fingerprint to bind the license to." };
            Option<string> expires = new Option<string>("--expires") { Description = "Optional expiry date (yyyy-MM-dd)." };
            Option<int> maxUsers = new Option<int>("--max-users") { Description = "Optional maximum number of concurrent users." };
            Option<string> remarks = new Option<string>("--remarks") { Description = "Free-form remarks." };
            Option<string> issuerLicense = new Option<string>("--issuer-license")
            {
                Description = "Path to your DeveloperIssuer license. It is embedded in the product license so that the customer needs no developer key. Strongly recommended."
            };

            Command command = new Command("issue-product", "Issues a product license for a concrete database installation.");
            command.Add(privateKey);
            command.Add(output);
            command.Add(customer);
            command.Add(projectNo);
            command.Add(issuerName);
            command.Add(packages);
            command.Add(database);
            command.Add(activationFile);
            command.Add(machine);
            command.Add(expires);
            command.Add(maxUsers);
            command.Add(remarks);
            command.Add(issuerLicense);

            command.SetAction(parseResult =>
            {
                string[] packageList = parseResult.GetValue(packages) ?? Array.Empty<string>();
                if (packageList.Length == 0)
                    return Fail("At least one --package is required for a product license.");

                if (!TryResolveFingerprint(parseResult.GetValue(database), parseResult.GetValue(activationFile), out string productFingerprint, out string productFingerprintError))
                    return Fail(productFingerprintError);

                LicensePayload payload = new LicensePayload
                {
                    Kind = LicenseKind.Product,
                    CustomerName = parseResult.GetValue(customer)!,
                    ProjectNo = parseResult.GetValue(projectNo) ?? string.Empty,
                    IssuerName = parseResult.GetValue(issuerName) ?? string.Empty,
                    DatabaseFingerprint = productFingerprint,
                    MachineFingerprint = parseResult.GetValue(machine) ?? string.Empty,
                    Remarks = parseResult.GetValue(remarks) ?? string.Empty,
                    Packages = packageList.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToList()
                };

                int users = parseResult.GetValue(maxUsers);
                if (users > 0)
                    payload.MaxUsers = users;

                if (!TryApplyExpiry(parseResult.GetValue(expires), payload, out string error))
                    return Fail(error);

                string? issuerLicensePath = parseResult.GetValue(issuerLicense);
                if (!string.IsNullOrWhiteSpace(issuerLicensePath))
                {
                    if (!File.Exists(issuerLicensePath))
                        return Fail("Issuer license file not found: " + issuerLicensePath);
                    payload.IssuerLicense = File.ReadAllText(issuerLicensePath);
                }

                return IssueAndReport(parseResult.GetValue(privateKey)!, parseResult.GetValue(output)!, payload);
            });

            return command;
        }

        private static Command VerifyCommand()
        {
            Option<string> license = RequiredPathOption("--license", "-l", "Path of the license file to verify.");
            Option<string> publicKey = new Option<string>("--public-key")
            {
                Description = "Public key file. Defaults to the framework key embedded in gip.core.licensing."
            };
            Option<string> database = new Option<string>("--database-fingerprint") { Description = "Database fingerprint to check against." };
            Option<string> machine = new Option<string>("--machine-fingerprint") { Description = "Machine fingerprint to check against." };
            Option<string> package = new Option<string>("--package", "-p") { Description = "Package that must be covered by the license." };
            Option<string> issuerLicense = new Option<string>("--issuer-license")
            {
                Description = "Path to the DeveloperIssuer license that embeds the developer's public key. Required to verify a product license signed by a developer key."
            };

            Command command = new Command("verify", "Verifies a license file offline.");
            command.Add(license);
            command.Add(publicKey);
            command.Add(database);
            command.Add(machine);
            command.Add(package);
            command.Add(issuerLicense);

            command.SetAction(parseResult =>
            {
                string licensePath = parseResult.GetValue(license)!;
                if (!File.Exists(licensePath))
                    return Fail("License file not found: " + licensePath);

                LicenseVerifier verifier;
                string? publicKeyPath = parseResult.GetValue(publicKey);
                string? issuerLicensePath = parseResult.GetValue(issuerLicense);
                if (!string.IsNullOrWhiteSpace(publicKeyPath))
                {
                    if (!File.Exists(publicKeyPath))
                        return Fail("Public key file not found: " + publicKeyPath);
                    verifier = LicenseVerifier.FromPublicKeyFile(publicKeyPath);
                }
                else if (!string.IsNullOrWhiteSpace(issuerLicensePath))
                {
                    // Two-level PKI chain: read the developer's public key out of the
                    // DeveloperIssuer license that gip signed.
                    if (!File.Exists(issuerLicensePath))
                        return Fail("Issuer license file not found: " + issuerLicensePath);

                    LicenseValidationResult issuerResult = FrameworkLicenseKeys.CreateVerifier().VerifyFile(issuerLicensePath);
                    if (!issuerResult.IsValid)
                        return Fail("Issuer license is not valid: " + issuerResult.Error + " — " + issuerResult.Message);
                    if (issuerResult.Payload!.Kind != LicenseKind.DeveloperIssuer)
                        return Fail("Issuer license is not a DeveloperIssuer license.");
                    if (string.IsNullOrEmpty(issuerResult.Payload.IssuerPublicKey))
                        return Fail("Issuer license does not embed a developer public key.");

                    // The verifier keeps a reference to the key, so it must stay alive
                    // for the whole verification (do not dispose it here).
                    RSA developerKey = LicenseKeys.ImportPublicKey(issuerResult.Payload.IssuerPublicKey);
                    verifier = FrameworkLicenseKeys.CreateVerifier(_ => developerKey);
                }
                else
                {
                    verifier = FrameworkLicenseKeys.CreateVerifier();
                }

                LicenseBindingContext context = new LicenseBindingContext
                {
                    DatabaseFingerprint = parseResult.GetValue(database) ?? string.Empty,
                    MachineFingerprint = parseResult.GetValue(machine) ?? string.Empty,
                    RequiredPackage = parseResult.GetValue(package)
                };

                LicenseValidationResult result = verifier.VerifyFile(licensePath, context);
                if (!result.IsValid)
                {
                    Console.Error.WriteLine("INVALID: " + result.Error + " — " + result.Message);
                    return 2;
                }

                PrintPayload(result.Payload!);
                Console.WriteLine("VALID");
                return 0;
            });

            return command;
        }

        private static bool TryReadActivationFile(string path, out System.Xml.Linq.XElement? root, out string error)
        {
            root = null;
            error = string.Empty;
            if (!File.Exists(path))
            {
                error = "Activation file not found: " + path;
                return false;
            }

            try
            {
                root = System.Xml.Linq.XDocument.Load(path).Root;
            }
            catch (System.Xml.XmlException e)
            {
                error = "The activation file is not valid: " + e.Message;
                return false;
            }

            if (string.IsNullOrWhiteSpace(root?.Element("code")?.Value))
            {
                error = "The activation file contains no <code> element.";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Uses the explicit fingerprint, or derives it from the customer's activation file.
        /// Both may be omitted (the license is then not database bound).
        /// </summary>
        private static bool TryResolveFingerprint(string? explicitFingerprint, string? activationFilePath, out string fingerprint, out string error)
        {
            error = string.Empty;
            fingerprint = explicitFingerprint ?? string.Empty;

            if (string.IsNullOrWhiteSpace(activationFilePath))
                return true;

            if (!string.IsNullOrWhiteSpace(explicitFingerprint))
            {
                error = "Use either --database-fingerprint or --activation-file, not both.";
                return false;
            }

            if (!TryReadActivationFile(activationFilePath, out System.Xml.Linq.XElement? root, out error))
                return false;

            fingerprint = DatabaseFingerprint.FromLegacyChecksumCode(root!.Element("code")!.Value);
            return true;
        }

        private static Command FingerprintCommand()
        {
            Option<string> activation = RequiredPathOption("--activation-file", "-a", "Activation data file (*.gip) that the customer exported from the license dialog.");

            Command command = new Command("fingerprint", "Prints the database fingerprint contained in a customer's activation data file.");
            command.Add(activation);

            command.SetAction(parseResult =>
            {
                string path = parseResult.GetValue(activation)!;
                if (!File.Exists(path))
                    return Fail("Activation file not found: " + path);

                if (!TryReadActivationFile(path, out System.Xml.Linq.XElement? root, out string error))
                    return Fail(error);

                Console.WriteLine("Customer      : " + root!.Element("companyName")?.Value);
                Console.WriteLine("Database      : " + root.Element("db")?.Value);
                Console.WriteLine("Server        : " + root.Element("ds")?.Value);
                Console.WriteLine("Fingerprint   : " + DatabaseFingerprint.FromLegacyChecksumCode(root.Element("code")!.Value));
                return 0;
            });

            return command;
        }

        private static Command ShowCommand()
        {
            Option<string> license = RequiredPathOption("--license", "-l", "Path of the license file to display.");

            Command command = new Command("show", "Displays the payload of a license file without verifying it.");
            command.Add(license);

            command.SetAction(parseResult =>
            {
                string licensePath = parseResult.GetValue(license)!;
                if (!File.Exists(licensePath))
                    return Fail("License file not found: " + licensePath);

                if (!LicenseFile.TryParse(LicenseFile.Read(licensePath), out _, out byte[] payloadBytes, out string? error))
                    return Fail(error ?? "License file could not be parsed.");

                LicensePayload payload = LicenseFile.DeserializePayload(System.Text.Encoding.UTF8.GetString(payloadBytes));
                PrintPayload(payload);
                Console.WriteLine("NOTE: payload shown without signature verification. Use 'verify'.");
                return 0;
            });

            return command;
        }

        #endregion

        #region Helpers

        private static Option<string> RequiredKeyOption(string name, string description)
        {
            return new Option<string>(name) { Description = description, Required = true };
        }

        private static Option<string> RequiredPathOption(string name, string alias, string description)
        {
            return new Option<string>(name, alias) { Description = description, Required = true };
        }

        private static Option<string> RequiredTextOption(string name, string description)
        {
            return new Option<string>(name) { Description = description, Required = true };
        }

        private static bool TryApplyExpiry(string? expiresText, LicensePayload payload, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(expiresText))
                return true;

            if (!DateTimeOffset.TryParse(expiresText, out DateTimeOffset expires))
            {
                error = "Invalid --expires value '" + expiresText + "'. Expected yyyy-MM-dd.";
                return false;
            }

            payload.ExpiresUtc = expires.ToUniversalTime();
            return true;
        }

        private static int IssueAndReport(string privateKeyPath, string outputPath, LicensePayload payload)
        {
            if (!File.Exists(privateKeyPath))
                return Fail("Private key file not found: " + privateKeyPath);

            LicenseIssuer issuer = LicenseIssuer.FromPrivateKeyFile(privateKeyPath);

            // Stamp the issuer key id so that the verifier can resolve the matching
            // public key. A DeveloperIssuer license already carries the *developer's*
            // key id (set by the caller) — that one must not be overwritten with
            // gip's key id, otherwise the two-level chain breaks.
            if (string.IsNullOrEmpty(payload.IssuerKeyId))
                payload.IssuerKeyId = issuer.GetKeyId();

            issuer.IssueToFile(payload, outputPath);

            Console.WriteLine("License written to: " + outputPath);
            PrintPayload(payload);
            return 0;
        }

        private static void PrintPayload(LicensePayload payload)
        {
            Console.WriteLine("  License id  : " + payload.LicenseId);
            Console.WriteLine("  Kind        : " + payload.Kind);
            Console.WriteLine("  Customer    : " + payload.CustomerName);
            Console.WriteLine("  Project no  : " + payload.ProjectNo);
            Console.WriteLine("  Issuer      : " + payload.IssuerName);
            Console.WriteLine("  Issued (UTC): " + payload.IssuedUtc.ToString("u"));
            Console.WriteLine("  Expires     : " + (payload.ExpiresUtc.HasValue ? payload.ExpiresUtc.Value.ToString("u") : "never"));
            Console.WriteLine("  Max users   : " + (payload.MaxUsers.HasValue ? payload.MaxUsers.Value.ToString() : "unlimited"));
            Console.WriteLine("  Packages    : " + (payload.Packages.Count == 0 ? "(all)" : string.Join(", ", payload.Packages)));
            if (payload.Kind == LicenseKind.DeveloperIssuer)
                Console.WriteLine("  Namespaces  : " + (payload.PackageNamespaces.Count == 0 ? "(all)" : string.Join(", ", payload.PackageNamespaces)));
            Console.WriteLine("  DB binding  : " + (string.IsNullOrEmpty(payload.DatabaseFingerprint) ? "(none)" : payload.DatabaseFingerprint));
            Console.WriteLine("  Machine     : " + (string.IsNullOrEmpty(payload.MachineFingerprint) ? "(none)" : payload.MachineFingerprint));
            if (!string.IsNullOrEmpty(payload.IssuerKeyId))
                Console.WriteLine("  Issuer key  : " + payload.IssuerKeyId);
            if (!string.IsNullOrEmpty(payload.Remarks))
                Console.WriteLine("  Remarks     : " + payload.Remarks);
        }

        private static int Fail(string message)
        {
            Console.Error.WriteLine("ERROR: " + message);
            return 1;
        }

        #endregion
    }
}

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace gip.iplus.client.avui
{
    /// <summary>
    /// Protects the persisted Entra token cache at rest.
    /// Payload format stored in settings:
    ///   "enc1:&lt;base64(12-byte iv || ciphertext || 16-byte tag)&gt;"  - AES-256-GCM encrypted
    ///   "plain:&lt;json&gt;"                                             - browser (WASM), per-origin isolated storage
    ///   (no prefix)                                                   - legacy plain JSON, decrypted on read
    /// Key material:
    ///   Windows: DPAPI (CurrentUser scope) - no key file needed.
    ///   Android: 32-byte random key file in app-private storage (sandboxed).
    ///   Linux/macOS: 32-byte random key file in user profile dir, chmod 600.
    /// </summary>
    public static class EntraTokenProtector
    {
        private const string EncPrefix = "enc1:";
        private const string PlainPrefix = "plain:";
        private const string KeyFileName = "iplus-entratoken.key";
        private const int KeySizeBytes = 32;
        private const int NonceSizeBytes = 12;
        private const int TagSizeBytes = 16;

        public static string Protect(string json)
        {
            if (String.IsNullOrEmpty(json))
                return json;

            // Browser (WASM): localStorage is already per-origin isolated; encryption adds
            // nothing against XSS and there is no durable private key store. Store plainly.
            if (OperatingSystem.IsBrowser())
                return PlainPrefix + json;

            byte[] key = GetOrCreateKey();
            try
            {
                byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
                byte[] plainBytes = Encoding.UTF8.GetBytes(json);
                byte[] cipherBytes = new byte[plainBytes.Length];
                byte[] tag = new byte[TagSizeBytes];
                using (AesGcm aes = new AesGcm(key, TagSizeBytes))
                {
                    aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
                }
                byte[] payload = new byte[nonce.Length + cipherBytes.Length + tag.Length];
                Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
                Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length, cipherBytes.Length);
                Buffer.BlockCopy(tag, 0, payload, nonce.Length + cipherBytes.Length, tag.Length);
                return EncPrefix + Convert.ToBase64String(payload);
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
            }
        }

        public static string Unprotect(string stored)
        {
            if (String.IsNullOrEmpty(stored))
                return stored;

            // Legacy plain JSON (written before encryption existed).
            if (!stored.StartsWith(EncPrefix) && !stored.StartsWith(PlainPrefix))
                return stored;

            if (stored.StartsWith(PlainPrefix))
                return stored.Substring(PlainPrefix.Length);

            byte[] key = GetOrCreateKey();
            try
            {
                byte[] payload;
                try
                {
                    payload = Convert.FromBase64String(stored.Substring(EncPrefix.Length));
                }
                catch (FormatException)
                {
                    return null;
                }
                if (payload.Length < NonceSizeBytes + TagSizeBytes)
                    return null;

                byte[] nonce = new byte[NonceSizeBytes];
                byte[] cipherBytes = new byte[payload.Length - NonceSizeBytes - TagSizeBytes];
                byte[] tag = new byte[TagSizeBytes];
                Buffer.BlockCopy(payload, 0, nonce, 0, NonceSizeBytes);
                Buffer.BlockCopy(payload, NonceSizeBytes, cipherBytes, 0, cipherBytes.Length);
                Buffer.BlockCopy(payload, NonceSizeBytes + cipherBytes.Length, tag, 0, TagSizeBytes);

                byte[] plainBytes = new byte[cipherBytes.Length];
                using (AesGcm aes = new AesGcm(key, TagSizeBytes))
                {
                    aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
                }
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                // Wrong key / corrupted payload.
                return null;
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
            }
        }

        private static byte[] GetOrCreateKey()
        {
            // Windows: DPAPI protects the token directly; the "key" is derived entropy
            // bound to the current user. We still use AES-GCM with a DPAPI-wrapped key
            // so the payload format stays identical across platforms.
            if (OperatingSystem.IsWindows())
                return GetOrCreateKeyFile(protectWithDpapi: true);

            return GetOrCreateKeyFile(protectWithDpapi: false);
        }

        private static byte[] GetOrCreateKeyFile(bool protectWithDpapi)
        {
            string dir = GetAppDataDir();
            string path = Path.Combine(dir, KeyFileName);
            byte[] raw;
            if (File.Exists(path))
            {
                raw = File.ReadAllBytes(path);
                if (protectWithDpapi)
                    raw = ProtectedData.Unprotect(raw, null, DataProtectionScope.CurrentUser);
                if (raw != null && raw.Length == KeySizeBytes)
                    return raw;
            }

            byte[] key = RandomNumberGenerator.GetBytes(KeySizeBytes);
            byte[] toWrite = protectWithDpapi
                ? ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser)
                : key;
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(path, toWrite);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
                catch
                {
                    // Best effort; Android storage is already sandboxed per-app.
                }
            }
            return key;
        }

        private static string GetAppDataDir()
        {
            if (OperatingSystem.IsAndroid())
            {
                // App-private storage (/data/user/0/<pkg>/files) - sandboxed by the OS.
                string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(String.IsNullOrEmpty(baseDir) ? "." : baseDir, "iplus");
            }
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "iPlus",
                "TokenKeys");
        }
    }
}

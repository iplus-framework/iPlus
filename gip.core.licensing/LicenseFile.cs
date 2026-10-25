// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gip.core.licensing
{
    /// <summary>
    /// Reads and writes the on-disk license file format.
    /// </summary>
    /// <remarks>
    /// The format is deliberately line-oriented and human readable so that a
    /// customer can inspect a license file with a text editor:
    /// <code>
    /// IPLUS-LICENSE
    /// &lt;base64 signature&gt;
    /// { ...json payload... }
    /// </code>
    /// The signature is computed over the exact UTF-8 bytes of the JSON payload
    /// that follow the second line. Because the payload bytes are stored
    /// verbatim there is no canonicalization problem: verification re-reads the
    /// same bytes that were signed.
    /// </remarks>
    public static class LicenseFile
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes a payload to its canonical JSON text.
        /// </summary>
        public static string SerializePayload(LicensePayload payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            return JsonSerializer.Serialize(payload, _jsonOptions);
        }

        /// <summary>
        /// Deserializes a payload from JSON text.
        /// </summary>
        public static LicensePayload DeserializePayload(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new LicenseFormatException("License payload is empty.");

            LicensePayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<LicensePayload>(json, _jsonOptions);
            }
            catch (JsonException e)
            {
                throw new LicenseFormatException("License payload is not valid JSON: " + e.Message, e);
            }

            if (payload == null)
                throw new LicenseFormatException("License payload could not be deserialized.");

            return payload;
        }

        /// <summary>
        /// Builds the complete license file text from a payload and its signature.
        /// </summary>
        public static string Build(LicensePayload payload, byte[] signature)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (signature == null || signature.Length == 0)
                throw new ArgumentException("Signature is empty.", nameof(signature));

            string json = SerializePayload(payload);
            StringBuilder sb = new StringBuilder(json.Length + 256);
            sb.Append(LicenseFormat.Magic).Append('\n');
            sb.Append(Convert.ToBase64String(signature)).Append('\n');
            sb.Append(json);
            return sb.ToString();
        }

        /// <summary>
        /// Splits a license file text into its signature and the exact payload bytes.
        /// </summary>
        public static bool TryParse(string text, out byte[] signature, out byte[] payloadBytes, out string? error)
        {
            signature = Array.Empty<byte>();
            payloadBytes = Array.Empty<byte>();
            error = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "License file is empty.";
                return false;
            }

            // Normalize line endings so that files edited on Windows still parse.
            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            int firstNewline = normalized.IndexOf('\n');
            if (firstNewline < 0)
            {
                error = "License file has no header line.";
                return false;
            }

            string header = normalized.Substring(0, firstNewline).Trim();
            if (!string.Equals(header, LicenseFormat.Magic, StringComparison.Ordinal))
            {
                error = "License file header is not '" + LicenseFormat.Magic + "'.";
                return false;
            }

            int secondNewline = normalized.IndexOf('\n', firstNewline + 1);
            if (secondNewline < 0)
            {
                error = "License file has no signature line.";
                return false;
            }

            string signatureLine = normalized.Substring(firstNewline + 1, secondNewline - firstNewline - 1).Trim();
            try
            {
                signature = Convert.FromBase64String(signatureLine);
            }
            catch (FormatException)
            {
                error = "License signature is not valid base64.";
                return false;
            }

            string payloadText = normalized.Substring(secondNewline + 1);
            payloadBytes = Encoding.UTF8.GetBytes(payloadText);
            return true;
        }

        /// <summary>
        /// Writes a license file to disk.
        /// </summary>
        public static void Write(string path, LicensePayload payload, byte[] signature)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("License path is empty.", nameof(path));

            string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, Build(payload, signature), new UTF8Encoding(false));
        }

        /// <summary>
        /// Reads a license file from disk.
        /// </summary>
        public static string Read(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("License path is empty.", nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("License file not found.", path);
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}

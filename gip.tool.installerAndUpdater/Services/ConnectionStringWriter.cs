// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Xml.Linq;
using gip.tool.installerAndUpdater.Models;

namespace gip.tool.installerAndUpdater.Services
{
    /// <summary>
    /// Writes the V5 <c>ConnectionStrings.config</c> next to the application.
    /// </summary>
    /// <remarks>
    /// V4 wrote EF6 <c>EntityClient</c> connection strings with
    /// <c>metadata=res://*/iPlusV4.csdl|…</c>. V5 uses plain SQL connection
    /// strings named <c>iPlusV5_Entities</c> / <c>iPlusMESV5_Entities</c>.
    /// </remarks>
    public static class ConnectionStringWriter
    {
        public const string FileName = "ConnectionStrings.config";

        public static string GetEntityName(ProductKind product)
        {
            return product == ProductKind.Mes ? "iPlusMESV5_Entities" : "iPlusV5_Entities";
        }

        /// <summary>
        /// Writes (or updates) <c>ConnectionStrings.config</c> in
        /// <paramref name="targetPath"/>.
        /// </summary>
        public static void Write(string targetPath, ProductKind product, SqlConnectionSettings settings)
        {
            string filePath = Path.Combine(targetPath, FileName);
            string entityName = GetEntityName(product);
            string connectionString = settings.BuildApplicationConnectionString();

            XDocument document;
            if (File.Exists(filePath))
            {
                try
                {
                    document = XDocument.Load(filePath);
                }
                catch
                {
                    document = CreateEmptyDocument();
                }
            }
            else
            {
                document = CreateEmptyDocument();
            }

            XElement root = document.Root ?? new XElement("configuration");
            if (document.Root == null)
                document.Add(root);

            XElement? connectionStrings = root.Element("connectionStrings");
            if (connectionStrings == null)
            {
                connectionStrings = new XElement("connectionStrings");
                root.Add(connectionStrings);
            }

            XElement? existing = connectionStrings
                .Elements("add")
                .FirstOrDefault(e => string.Equals(
                    (string?)e.Attribute("name"), entityName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                existing = new XElement("add", new XAttribute("name", entityName));
                connectionStrings.Add(existing);
            }

            existing.SetAttributeValue("connectionString", connectionString);
            existing.SetAttributeValue("providerName", "Microsoft.Data.SqlClient");

            document.Save(filePath);
        }

        /// <summary>
        /// Reads the connection string of the application database, or an empty
        /// string when the file does not exist.
        /// </summary>
        public static string Read(string targetPath, ProductKind product)
        {
            string filePath = Path.Combine(targetPath, FileName);
            if (!File.Exists(filePath))
                return string.Empty;

            try
            {
                XDocument document = XDocument.Load(filePath);
                string entityName = GetEntityName(product);

                XElement? entry = document.Root?
                    .Element("connectionStrings")?
                    .Elements("add")
                    .FirstOrDefault(e => string.Equals(
                        (string?)e.Attribute("name"), entityName, StringComparison.OrdinalIgnoreCase));

                return (string?)entry?.Attribute("connectionString") ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static XDocument CreateEmptyDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("configuration",
                    new XElement("connectionStrings")));
        }
    }
}

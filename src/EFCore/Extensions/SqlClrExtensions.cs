using System;
using System.IO;

using Microsoft.EntityFrameworkCore;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Extensions
{
    /// <summary>
    /// Extension methods for SQL CLR integration.
    /// </summary>
    internal static class SqlClrExtensions
    {
        /// <summary>
        /// Deploys a SQL CLR assembly to the database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="assemblyPath">Path to the SQL CLR assembly file.</param>
        /// <param name="assemblyName">Name of the assembly in SQL Server.</param>
        /// <param name="permissionSet">Permission set for the assembly (SAFE, EXTERNAL_ACCESS, or UNSAFE).</param>
        internal static void DeploySqlClrAssembly(
            this DbContext context,
            string assemblyPath,
            string assemblyName,
            string permissionSet = "SAFE")
        {
            ArgumentNullException.ThrowIfNull(context);

            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentException("Assembly path cannot be null or empty", nameof(assemblyPath));

            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentException("Assembly name cannot be null or empty", nameof(assemblyName));

            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file not found at path: {assemblyPath}");

            // First, ensure CLR is enabled
            const string enableClrSql
                = """
                    IF NOT EXISTS (SELECT 1 FROM sys.configurations WHERE name = 'clr enabled' AND value_in_use = 1)
                    BEGIN
                        EXEC sp_configure 'show advanced options', 1;
                        RECONFIGURE;
                        EXEC sp_configure 'clr enabled', 1;
                        RECONFIGURE;
                    END
                  """;
            context.Database.ExecuteSqlRaw(enableClrSql);

            var assemblyBytes = File.ReadAllBytes(assemblyPath);
            var assemblyHex = BitConverter.ToString(assemblyBytes).Replace("-", "");

            FormattableString sql
                = $"""

                       IF NOT EXISTS (SELECT 1 FROM sys.assemblies WHERE name = {assemblyName})
                       BEGIN
                           DECLARE @assembly VARBINARY(MAX) = 0x{assemblyHex};
                           CREATE ASSEMBLY [{assemblyName}]
                           FROM @assembly
                           WITH PERMISSION_SET = {permissionSet};
                       END
                   """;
            // Deploy the assembly
            context.Database.ExecuteSqlInterpolated(sql);
        }

        /// <summary>
        /// Removes a SQL CLR assembly from the database if it exists.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="assemblyName">Name of the assembly in SQL Server.</param>
        internal static void RemoveSqlClrAssembly(
            this DbContext context,
            string assemblyName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentException("Assembly name cannot be null or empty", nameof(assemblyName));
            FormattableString sql
                = $"""
                       IF EXISTS (SELECT 1 FROM sys.assemblies WHERE name = {assemblyName})
                       BEGIN
                           DROP ASSEMBLY [{assemblyName}];
                       END
                   """;
            context.Database.ExecuteSqlInterpolated(sql);
        }
    }
}
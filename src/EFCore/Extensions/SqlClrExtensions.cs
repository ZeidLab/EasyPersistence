using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Extensions
{
    /// <summary>
    /// Helper methods for SQL CLR integration.
    /// </summary>
    internal static class SqlClrHelper
    {
        /// <summary>
        /// Deploys a SQL CLR assembly to the database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="assemblyName">Name of the assembly in SQL Server.</param>
        /// <param name="permissionSet">Permission set for the assembly (SAFE, EXTERNAL_ACCESS, or UNSAFE).</param>
        public static void DeploySqlClrAssembly(
            this DbContext context,
            string assemblyName,
            string permissionSet = "SAFE")
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            const string assemblyPath = "EasyPersistence.EFCoreSqlClr.dll";

            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file not found at path: {assemblyPath}");

            // First, ensure CLR is enabled
            const string enableClrSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.configurations WHERE name = 'clr enabled' AND value_in_use = 1)
                BEGIN
                    EXEC sp_configure 'show advanced options', 1;
                    RECONFIGURE;
                    EXEC sp_configure 'clr enabled', 1;
                    RECONFIGURE;
                END";
            context.Database.ExecuteSqlRaw(enableClrSql);

            var assemblyBytes = File.ReadAllBytes(assemblyPath);
            var assemblyHex = BitConverter.ToString(assemblyBytes).Replace("-", "");

            // Deploy the assembly
            var sql = $@"
                IF NOT EXISTS (SELECT 1 FROM sys.assemblies WHERE name = '{assemblyName}')
                BEGIN
                    DECLARE @assembly VARBINARY(MAX) = 0x{assemblyHex};
                    CREATE ASSEMBLY [{assemblyName}]
                    FROM @assembly
                    WITH PERMISSION_SET = {permissionSet};
                END";

            context.Database.ExecuteSqlRaw(sql);
        }

        /// <summary>
        /// Initializes the SQL CLR assembly in the database using DbContext.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task InitializeSqlClrAsync(
            this DbContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            const string assemblyName = "EFCoreSqlClr";
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, "EasyPersistence.EFCoreSqlClr.dll");

            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"SQL CLR assembly not found at: {assemblyPath}");

            byte[] assemblyBytes;
            using (var fileStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                assemblyBytes = new byte[fileStream.Length];
                var bytesRead = await fileStream.ReadAsync(assemblyBytes, cancellationToken).ConfigureAwait(false);

                if (bytesRead != fileStream.Length)
                    throw new IOException("Failed to read complete assembly file");
            }

            var assemblyHex = BitConverter.ToString(assemblyBytes).Replace("-", "");
            var sql = $@"
                IF NOT EXISTS (SELECT 1 FROM sys.assemblies WHERE name = '{assemblyName}')
                BEGIN
                    DECLARE @assembly VARBINARY(MAX) = 0x{assemblyHex};
                    CREATE ASSEMBLY [{assemblyName}]
                    FROM @assembly
                    WITH PERMISSION_SET = SAFE;
                END";

            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a SQL CLR assembly from the database if it exists.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="assemblyName">Name of the assembly in SQL Server.</param>
        public static void RemoveSqlClrAssembly(
            this DbContext context,
            string assemblyName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentException("Assembly name cannot be null or empty", nameof(assemblyName));

            var sql = $@"
                IF EXISTS (SELECT 1 FROM sys.assemblies WHERE name = '{assemblyName}')
                BEGIN
                    DROP ASSEMBLY [{assemblyName}];
                END";

            context.Database.ExecuteSqlRaw(sql);
        }
    }
}
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore
{
    /// <summary>
    /// Helper methods for SQL CLR integration.
    /// </summary>
    internal static class SqlClrHelperExtensions
    {
        /// <summary>
        /// Initializes the SQL CLR assembly in the database using the provided <see cref="DbContext"/>.
        /// </summary>
        /// <param name="context">The database context used for SQL execution.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <remarks>
        /// This method enables CLR integration and deploys the required assembly if not already present.
        /// </remarks>
        public static async Task InitializeSqlClrAsync(
            this DbContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            const string assemblyName = "EFCoreSqlClr";
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, "ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.dll");

            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"SQL CLR assembly not found at: {assemblyPath}");

            byte[] assemblyBytes;
#pragma warning disable CAC001
            await using (var fileStream =
                         new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                assemblyBytes = new byte[fileStream.Length];
                var bytesRead = await fileStream.ReadAsync(assemblyBytes, cancellationToken).ConfigureAwait(false);

                if (bytesRead != fileStream.Length)
                    throw new IOException("Failed to read complete assembly file");
            }
#pragma warning restore CAC001

            var assemblyHex =
                BitConverter.ToString(assemblyBytes)
                    .Replace("-", ""); // First enable CLR and set clr strict security to 0
            const string checkConfigSql = @"
                EXEC sp_configure 'show advanced options', 1;
                RECONFIGURE;
                EXEC sp_configure 'clr enabled', 1;
                RECONFIGURE;
                IF EXISTS (SELECT 1 FROM sys.configurations WHERE name = 'clr strict security')
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.configurations WHERE name = 'clr strict security' AND value_in_use = 1)
                    BEGIN
                        EXEC sp_configure 'clr strict security', 0;
                        RECONFIGURE;
                    END
                END";
            await context.Database.ExecuteSqlRawAsync(checkConfigSql, cancellationToken).ConfigureAwait(false);

            // Deploy the assembly
            var sql = $@"
                IF NOT EXISTS (SELECT 1 FROM sys.assemblies WHERE name = '{assemblyName}')
                BEGIN
                    DECLARE @assembly VARBINARY(MAX) = 0x{assemblyHex};
                    CREATE ASSEMBLY [{assemblyName}]
                    FROM @assembly
                    WITH PERMISSION_SET = SAFE;
                END";

            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken).ConfigureAwait(false);

            // Calculate assembly hash and trust it
            var trustAssemblySql = $@"
                DECLARE @hash varbinary(64);
                SELECT @hash = HASHBYTES('SHA2_512', 0x{assemblyHex});
                IF NOT EXISTS (SELECT 1 FROM sys.trusted_assemblies WHERE hash = @hash)
                BEGIN
                    EXEC sp_add_trusted_assembly @hash;
                END";
            await context.Database.ExecuteSqlRawAsync(trustAssemblySql, cancellationToken).ConfigureAwait(false);

            // Create the FuzzySearch function that maps to the CLR method
            const string createFunctionSql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FuzzySearch]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
                BEGIN
                    EXEC sp_executesql N'
                    CREATE FUNCTION [dbo].[FuzzySearch]
                    (
                        @searchTerm NVARCHAR(MAX),
                        @comparedString NVARCHAR(MAX)
                    )
                    RETURNS FLOAT
                    AS EXTERNAL NAME [EFCoreSqlClr].[ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.SqlClrFunctions].[FuzzySearch]
                    ';
                END";

            await context.Database.ExecuteSqlRawAsync(createFunctionSql, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a SQL CLR assembly from the database if it exists.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="assemblyName">Name of the assembly in SQL Server.</param>
        public static Task RemoveSqlClrAssemblyAsync(
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

            return context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
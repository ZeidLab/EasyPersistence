using System.Data.SqlTypes;

using Microsoft.SqlServer.Server;

namespace ZeidLab.ToolBox.EasyPersistence.EfCoreSqlClr;

public static class SqlClrFunctions
{
    [SqlFunction]
    public static SqlInt32 FuzzySearch(SqlString searchTerm, SqlString comparedString)
    {
        return 0;
    }
}
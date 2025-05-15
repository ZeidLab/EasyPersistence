using System;
using System.Data.SqlTypes;

using Microsoft.SqlServer.Server;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr;

public static class SqlClrFunctions
{
    [SqlFunction]
    public static SqlDouble FuzzySearch(SqlString searchTerm, SqlString comparedString)
    {
        if (searchTerm.IsNull || comparedString.IsNull)
            return new SqlDouble(0);

        string term = searchTerm.Value.ToLowerInvariant();
        string compared = comparedString.Value.ToLowerInvariant();        // Very simplified implementation - returns 1.0 for exact match or substring,
        // 0.8 for case-insensitive match, and smaller values based on term length
        if (compared.Contains(term))
            return new SqlDouble(1.0);
        
        if (compared.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            return new SqlDouble(0.8);
            
        // Return a simplified score based on how many characters match the beginning
        // of the compared string (for simple prefix matching)
        int matchedChars = 0;
        for (int i = 0; i < Math.Min(term.Length, compared.Length); i++)
        {
            if (char.ToLowerInvariant(term[i]) == char.ToLowerInvariant(compared[i]))
                matchedChars++;
            else
                break;
        }
        
        if (matchedChars > 0)
        {
            double score = (double)matchedChars / term.Length * 0.7;
            return new SqlDouble(score);
        }
        
        return new SqlDouble(0);
    }
}
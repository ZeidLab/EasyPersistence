using System;
using System.Data.SqlTypes;
using System.Linq;

using Microsoft.SqlServer.Server;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr;

public static class SqlClrFunctions
{
    [SqlFunction]
    public static SqlDouble FuzzySearch(SqlString searchTerm, SqlString comparedString)
    {
        if (string.IsNullOrWhiteSpace(searchTerm.Value)
            || string.IsNullOrWhiteSpace(comparedString.Value)
            || searchTerm.Value.Length > comparedString.Value.Length)
            return new SqlDouble(0);

        // Normalize the strings to ensure consistent comparison
        string term = searchTerm.Value.Normalize();
        string compared = comparedString.Value.Normalize();

        // Quick exact match check case-sensitive
        if (compared.Equals(term, StringComparison.Ordinal))
            return new SqlDouble(1.0);

        // Find all occurrences of term in compared (case-insensitive)
        int position = 0;
        int matchedChars = 0;

        while ((position = compared.IndexOf(term, position, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            matchedChars += term.Length;
            position += term.Length;
        }

        if (position < 0)
        {
            // Calculate n-gram similarity for more accurate matching
            var similarity = NGram.CalculateNGramSimilarity(term.ToLowerInvariant(), compared.ToLowerInvariant());
            return new SqlDouble(NormalizeTheScore(similarity));
        }

        // Normalize to 0.00-0.09 range and add to base score of 0.9
        double score = 0.9 + (((double)matchedChars / compared.Length) * 0.09);

        return new SqlDouble(score);
    }

    private static double NormalizeTheScore(double score)
    {
        double normalized = (score / 10.0) * 9.0;
        double rounded = Math.Round(normalized, 4);
        return rounded < 0.0001 ? 0 : rounded;
    }
}
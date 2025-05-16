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

        // Convert values to lowercase immediately to avoid multiple conversions later
        string term = searchTerm.Value.ToLowerInvariant();
        string compared = comparedString.Value.ToLowerInvariant();

        // Quick exact match check
        if (compared.Contains(term))
            return new SqlDouble(1.0);

        // Length-based optimizations
        if (term.Length == 0 || term.Length > compared.Length)
            return new SqlDouble(0);
        
        // Calculate n-gram similarity for more accurate matching
        return new SqlDouble(CalculateNGramSimilarity(term, compared));
    }

    private static double CalculateNGramSimilarity(string term, string compared)
    {
        double totalWeightedSimilarity = 0;
        double totalWeight = 0;

        // Limit n-gram size to improve performance while maintaining accuracy
        int maxNGramSize = Math.Min(term.Length, 3);

        // Reusable character buffer for n-grams
        char[] ngramBuffer = new char[maxNGramSize];

        for (int n = 1; n <= maxNGramSize; n++)
        {
            // Calculate intersection directly without creating intermediate arrays
            int termNGramCount = term.Length - n + 1;
            int comparedNGramCount = compared.Length - n + 1;

            if (termNGramCount <= 0 || comparedNGramCount <= 0)
                continue;

            int intersection = CountIntersectionDirect(term, compared, n, ngramBuffer);

            // Calculate Dice coefficient
            double diceCoefficient = (2.0 * intersection) / (termNGramCount + comparedNGramCount);

            // Weight by n-gram size relative to term length
            double weight = (double)n / maxNGramSize;
            totalWeightedSimilarity += diceCoefficient * weight;
            totalWeight += weight;
        }

        // Return normalized similarity score with a scaling factor
        return totalWeight > 0 ? (totalWeightedSimilarity / totalWeight) * 0.7 : 0;
    }

    // Optimized method to count intersections directly without creating intermediate arrays
    private static int CountIntersectionDirect(string term, string compared, int ngramSize, char[] buffer)
    {
        int count = 0;
        int termNGramCount = term.Length - ngramSize + 1;
        int comparedNGramCount = compared.Length - ngramSize + 1;

        // Use buffer to avoid string allocations
        for (int i = 0; i < termNGramCount; i++)
        {
            // Copy current term n-gram to buffer
            for (int c = 0; c < ngramSize; c++)
            {
                buffer[c] = term[i + c];
            }

            // Check against all n-grams in compared string
            for (int j = 0; j < comparedNGramCount; j++)
            {
                bool match = true;
                for (int c = 0; c < ngramSize; c++)
                {
                    if (buffer[c] != compared[j + c])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    count++;
                    break; // Found a match for this n-gram, move to next
                }
            }
        }

        return count;
    }
    
}
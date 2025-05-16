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
        int termLength = term.Length;


        while ((position = compared.IndexOf(term, position, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            matchedChars += termLength;
            position += termLength;
        }

        if (position < 0)
        {
            // Calculate n-gram similarity for more accurate matching
            return new SqlDouble(CalculateNGramSimilarity(term, compared));
        }

        // Normalize to 0.00-0.09 range and add to base score of 0.9
        double score = 0.9 + (((double)matchedChars / compared.Length) * 0.09);

        return new SqlDouble(score);
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
        return totalWeight > 0 ? (totalWeightedSimilarity / totalWeight) * 0.9 : 0;
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
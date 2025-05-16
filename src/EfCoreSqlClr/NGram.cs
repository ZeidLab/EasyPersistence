using System;
using System.Collections.Generic;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr;

internal static class NGram
{
       #region Main Similarity Logic

    /// <summary>
    /// Chooses n = 1, 2, or 3 based on term length, decodes to code points,
    /// and applies a fallback from 3 → 2 → 1 grams if no overlap is found.
    /// </summary>
    internal static double CalculateNGramSimilarity(string searchTerm, string comparedText)
    {
        // 1) Decode both strings into full Unicode codepoints,
        //    so surrogate pairs become single ints.
        int[] term = DecodeToCodePoints(searchTerm);
        int[] compared = DecodeToCodePoints(comparedText);

        // 2) Primary n choice: term length 1→1, 2→2, ≥3→3
        int primaryN = term.Length <= 3 ? term.Length : 3;

        // 3) Compute Dice at primary n = 1, 2, or 3
        double score = ComputeDice(term, compared, primaryN);
        if (score > 0 || primaryN == 1)
            return score;

        // 4) Fallback: if 3‑gram yields 0, try 2‑gram
        if (primaryN == 3)
        {
            score = ComputeDice(term, compared, 2);
            if (score > 0 )
                return  (score / 10);
        }

        // 5) Last fallback: 1‑gram
        return (ComputeDice(term, compared, 1) / 100);
    }
    
    #endregion

    #region Dice Coefficient Computation

    /// <summary>
    /// Builds a multiset of n‑grams from cp1, then streams cp2
    /// to count intersections, and returns 2·|I|/(|A|+|B|).
    /// </summary>
    /// <param name="cp1">Array of Unicode code points from search term.</param>
    /// <param name="cp2">Array of Unicode code points from compared string.</param>
    /// <param name="n">The n‑gram size to use (1, 2, or 3).</param>
    private static double ComputeDice(int[] cp1, int[] cp2, int n)
    {
        int count1 = cp1.Length - n + 1;
        int count2 = cp2.Length - n + 1;

        // If either string is too short for this n‑gram, no similarity.
        if (count1 <= 0 || count2 <= 0)
            return 0.0;

        // Determine the max bits needed to pack any code point in cp1
        // (so keys never collide). Up to 21 bits for full Unicode.
        int maxBits = 0;
        foreach (int codePoint in cp1)
        {
            int bits = BitLength(codePoint);
            if (bits > maxBits) maxBits = bits;
        }

        // 1) Build frequency map of cp1's n‑gram keys
        var freq = new Dictionary<ulong, int>(count1);
        for (int i = 0; i < count1; i++)
        {
            ulong key = PackNGram(cp1, i, n, maxBits);
            if (freq.TryGetValue(key, out int c))
                freq[key] = c + 1;
            else
                freq[key] = 1;
        }

        // 2) Stream through cp2 to count intersections
        int intersection = 0;
        for (int j = 0; j < count2; j++)
        {
            ulong key = PackNGram(cp2, j, n, maxBits);
            if (freq.TryGetValue(key, out int c) && c > 0)
            {
                freq[key] = c - 1;
                intersection++;
            }
        }

        // 3) Standard Dice formula
        return (2.0 * intersection) / (count1 + count2);
    }

    #endregion

    #region Unicode Decoding Helpers

    /// <summary>
    /// Converts a UTF‑16 string into an int[] of full Unicode code points.
    /// Properly handles surrogate pairs so each logical character is one code point.
    /// </summary>
    private static int[] DecodeToCodePoints(string s)
    {
        var points = new List<int>(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            // If high surrogate and followed by low, combine into one code point
            if (char.IsHighSurrogate(c)
             && i + 1 < s.Length
             && char.IsLowSurrogate(s[i + 1]))
            {
                points.Add(char.ConvertToUtf32(c, s[++i]));
            }
            else
            {
                // BMP character or isolated surrogate
                points.Add(c);
            }
        }
        return points.ToArray();
    }

    /// <summary>
    /// Returns the number of bits needed to represent 'value'.
    /// E.g. ASCII chars yield 7 bits; BMP up to 16 bits; supplementary up to 21.
    /// </summary>
    private static int BitLength(int value)
    {
        int bits = 0;
        while (value > 0)
        {
            bits++;
            value >>= 1;
        }
        return bits;
    }

    #endregion

    #region n‑gram Packing

    /// <summary>
    /// Packs n consecutive code points from cp[] starting at 'start'
    /// into a single ulong key by shifting left 'shift' bits per code point.
    /// Ensures collision‐free keys for up to 3 code points.
    /// </summary>
    /// <param name="cp">Array of Unicode code points.</param>
    /// <param name="start">Index of first code point in the n‑gram.</param>
    /// <param name="n">Number of code points (1–3).</param>
    /// <param name="shift">Bit‑width per code point (computed from BitLength).</param>
    private static ulong PackNGram(int[] cp, int start, int n, int shift)
    {
        ulong key = 0;
        for (int k = 0; k < n; k++)
        {
            key = (key << shift) | (uint)cp[start + k];
        }
        return key;
    }

    #endregion
}
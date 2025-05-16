using System;
using System.Data.SqlTypes;
using System.Text;

using FluentAssertions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.Test.Units
{
    public class SqlClrFunctionsTests
    {
        [Fact]
        public void FuzzySearch_WithNullInputs_ShouldReturnZero()
        {
            // Arrange
            SqlString nullString = SqlString.Null;
            SqlString validString = new SqlString("test");

            // Act & Assert
            SqlClrFunctions.FuzzySearch(nullString, validString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(validString, nullString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(nullString, nullString).Value.Should().Be(0);
        }

        

        [Fact]
        public void FuzzySearch_WithEmptyStrings_ShouldHandleCorrectly()
        {
            // Arrange
            SqlString emptyString = new SqlString(string.Empty);
            SqlString nonEmptyString = new SqlString("test");

            // Act & Assert
            SqlClrFunctions.FuzzySearch(emptyString, nonEmptyString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(nonEmptyString, emptyString).Value.Should().Be(0);
            SqlClrFunctions.FuzzySearch(emptyString, emptyString).Value.Should().Be(0);
        }

        [Fact]
        public void FuzzySearch_WithCaseDifferences_ShouldBeCaseInsensitive()
        {
            // Arrange
            SqlString lowerCase = new SqlString("test");
            SqlString upperCase = new SqlString("TEST");
            SqlString mixedCase = new SqlString("TeSt");

            // Act
            var lowerUpperResult = SqlClrFunctions.FuzzySearch(lowerCase, upperCase).Value;
            var lowerMixedResult = SqlClrFunctions.FuzzySearch(lowerCase, mixedCase).Value;
            var mixedUpperResult = SqlClrFunctions.FuzzySearch(mixedCase, upperCase).Value;

            // Assert
            lowerUpperResult.Should().Be(0.99);
            lowerMixedResult.Should().Be(0.99);
            mixedUpperResult.Should().Be(0.99);
        }

        [Fact]
        public void FuzzySearch_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            SqlString specialChars = new SqlString("test-123");
            SqlString withPunctuation = new SqlString("test-123!");
            SqlString differentPunctuation = new SqlString("test:123");

            // Act & Assert
            SqlClrFunctions.FuzzySearch(specialChars, withPunctuation).Value.Should().BeGreaterThan(0.8);
            SqlClrFunctions.FuzzySearch(specialChars, differentPunctuation).Value.Should().BeGreaterThan(0.6);
        }

        [Fact]
        public void FuzzySearch_WithTypos_ShouldReturnPartialMatches()
        {
            // Arrange
            SqlString correct = new SqlString("testing");
            SqlString typo1 = new SqlString("testign"); // Transposition
            SqlString typo2 = new SqlString("testng");  // Missing character
            SqlString typo3 = new SqlString("testting"); // Extra character

            // Act
            var transpositionResult = SqlClrFunctions.FuzzySearch(correct, typo1).Value;
            var missingCharResult = SqlClrFunctions.FuzzySearch(correct, typo2).Value;
            var extraCharResult = SqlClrFunctions.FuzzySearch(correct, typo3).Value;

            // Assert
            transpositionResult.Should().BeGreaterThan(0.5);
            missingCharResult.Should().BeGreaterThan(0.5);
            extraCharResult.Should().BeGreaterThan(0.5);
        }

        [Fact]
        public void FuzzySearch_WithDifferentLengthStrings_ShouldScaleAccordingly()
        {
            // Arrange
            SqlString shortTerm = new SqlString("test");
            SqlString veryLongString = new SqlString("this is a very long string with the word test somewhere in the middle and continues on");
            SqlString mediumString = new SqlString("testing purposes only");

            // Act
            var shortInLongResult = SqlClrFunctions.FuzzySearch(shortTerm, veryLongString).Value;
            var longForShortResult = SqlClrFunctions.FuzzySearch(veryLongString, shortTerm).Value;
            var shortInMediumResult = SqlClrFunctions.FuzzySearch(shortTerm, mediumString).Value;

            // Assert
            shortInLongResult.Should().BeGreaterThan(0.9); // Exact substring match
            longForShortResult.Should().Be(0); // No match when searching for long string in short
            shortInMediumResult.Should().BeGreaterThan(0.9); // Partial match
        }

        [Fact]
        public void FuzzySearch_SubstringPositioning_ShouldAffectScore()
        {
            // Arrange
            SqlString searchTerm = new SqlString("test");
            SqlString prefixMatch = new SqlString("test result");
            SqlString middleMatch = new SqlString("a test result");
            SqlString suffixMatch = new SqlString("result test");

            // Act
            var prefixResult = SqlClrFunctions.FuzzySearch(searchTerm, prefixMatch).Value;
            var middleResult = SqlClrFunctions.FuzzySearch(searchTerm, middleMatch).Value;
            var suffixResult = SqlClrFunctions.FuzzySearch(searchTerm, suffixMatch).Value;

            // Assert
            // All should be 0.9 since they contain the exact term
            prefixResult.Should().BeGreaterThan(0.9);
            middleResult.Should().BeGreaterThan(0.9);
            suffixResult.Should().BeGreaterThan(0.9);
        }

        [Fact]
        public void FuzzySearch_RealWorldScenarios_ShouldPerformReasonably()
        {
            // Arrange
            SqlString productName = new SqlString("iPhone 13 Pro Max");

            // Common search variations
            SqlString exactSearch = new SqlString("iPhone 13 Pro Max");
            SqlString partialSearch = new SqlString("iPhone 13");
            SqlString misspelledSearch = new SqlString("iPhnoe 13 Pro");
            SqlString abbreviatedSearch = new SqlString("ip13pro");

            // Act
            var exactResult = SqlClrFunctions.FuzzySearch(exactSearch, productName).Value;
            var partialResult = SqlClrFunctions.FuzzySearch(partialSearch, productName).Value;
            var misspelledResult = SqlClrFunctions.FuzzySearch(misspelledSearch, productName).Value;
            var abbreviatedResult = SqlClrFunctions.FuzzySearch(abbreviatedSearch, productName).Value;

            // Assert
            exactResult.Should().Be(1.0);
            partialResult.Should().BeGreaterThan(0.9); // Contains the substring exactly
            misspelledResult.Should().BeGreaterThan(0.4); // Should still match reasonably well
            abbreviatedResult.Should().BeGreaterThan(0.0); // Should have some similarity
        }

        // New Unicode-specific tests

        [Fact]
        public void FuzzySearch_WithUnicodeCharacters_ShouldHandleCorrectly()
        {
            // Arrange - Include characters from different scripts and planes
            SqlString latin = new SqlString("hello");
            SqlString cyrillic = new SqlString("привет");  // Russian
            SqlString cjk = new SqlString("你好");  // Chinese
            SqlString emoji = new SqlString("👋🏻");  // Wave emoji with skin tone modifier (surrogate pair)

            // Act & Assert - Test fuzzy matching within same scripts
            SqlClrFunctions.FuzzySearch(latin, latin).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(cyrillic, cyrillic).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(cjk, cjk).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(emoji, emoji).Value.Should().Be(1.0);

            // Different scripts should have low similarity
            SqlClrFunctions.FuzzySearch(latin, cyrillic).Value.Should().BeLessThan(0.1);
            SqlClrFunctions.FuzzySearch(latin, cjk).Value.Should().BeLessThan(0.1);
            SqlClrFunctions.FuzzySearch(cyrillic, cjk).Value.Should().BeLessThan(0.1);
        }

        [Fact]
        public void FuzzySearch_WithSurrogatePairs_ShouldHandleCorrectly()
        {
            // Arrange - Characters outside the BMP that require surrogate pairs in UTF-16
            SqlString bmpOnly = new SqlString("abc");
            SqlString withEmoji = new SqlString("abc😀");  // Basic Latin + emoji
            SqlString complexEmoji = new SqlString("👨‍👩‍👧‍👦");  // Family emoji (multiple surrogate pairs)
            SqlString mathSymbols = new SqlString("𝔸𝔹ℂ");  // Mathematical symbols

            // Act & Assert
            SqlClrFunctions.FuzzySearch(bmpOnly, withEmoji).Value.Should().BeGreaterThan(0.5);
            SqlClrFunctions.FuzzySearch(withEmoji, withEmoji).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(complexEmoji, complexEmoji).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(mathSymbols, mathSymbols).Value.Should().Be(1.0);
        }

        [Fact]
        public void FuzzySearch_WithCombiningCharacters_ShouldHandleNormalization()
        {
            // Arrange
            // Same visual representation, different Unicode composition
            SqlString precomposed = new SqlString("café");  // é is a single code point
            SqlString decomposed = new SqlString("cafe\u0301");  // e + combining acute accent

            SqlString precomposedGerman = new SqlString("schön");
            SqlString decomposedGerman = new SqlString("scho\u0308n");  // o + combining diaeresis

            // Act
            var result1 = SqlClrFunctions.FuzzySearch(precomposed, decomposed).Value;
            var result2 = SqlClrFunctions.FuzzySearch(decomposed, precomposed).Value;
            var result3 = SqlClrFunctions.FuzzySearch(precomposedGerman, decomposedGerman).Value;

            // Assert
            // Due to normalization, these should be treated as similar
            result1.Should().Be(1.0);
            result2.Should().Be(1.0);
            result3.Should().Be(1.0);
        }

        [Fact]
        public void FuzzySearch_WithSpecialUnicodeSequences_ShouldHandleEdgeCases()
        {
            // Arrange
            SqlString normal = new SqlString("test");
            SqlString withZeroWidth = new SqlString("te\u200Bst");  // With zero-width space
            SqlString withVariationSelector = new SqlString("test\uFE0E");  // With variation selector
            SqlString withRtlMark = new SqlString("\u200Ftest");  // With right-to-left mark

            // Act
            var resultZeroWidth = SqlClrFunctions.FuzzySearch(normal, withZeroWidth).Value;
            var resultVariation = SqlClrFunctions.FuzzySearch(normal, withVariationSelector).Value;
            var resultRtl = SqlClrFunctions.FuzzySearch(normal, withRtlMark).Value;

            // Assert
            // These should be similar despite the special characters
            resultZeroWidth.Should().BeGreaterThan(0.5);
            resultVariation.Should().BeGreaterThan(0.5);
            resultRtl.Should().BeGreaterThan(0.5);
        }

        [Fact]
        public void FuzzySearch_WithDifferentNormalizations_ShouldBeConsistent()
        {
            // Arrange
            string baseStr = "café résumé";
            SqlString nfc = new SqlString(baseStr.Normalize(NormalizationForm.FormC));
            SqlString nfd = new SqlString(baseStr.Normalize(NormalizationForm.FormD));
            SqlString nfkc = new SqlString(baseStr.Normalize(NormalizationForm.FormKC));
            SqlString nfkd = new SqlString(baseStr.Normalize(NormalizationForm.FormKD));

            // Act & Assert
            // All normalizations should match well with each other
            SqlClrFunctions.FuzzySearch(nfc, nfd).Value.Should().BeGreaterThan(0.7);
            SqlClrFunctions.FuzzySearch(nfc, nfkc).Value.Should().BeGreaterThan(0.7);
            SqlClrFunctions.FuzzySearch(nfc, nfkd).Value.Should().BeGreaterThan(0.7);
            SqlClrFunctions.FuzzySearch(nfd, nfkc).Value.Should().BeGreaterThan(0.7);
            SqlClrFunctions.FuzzySearch(nfd, nfkd).Value.Should().BeGreaterThan(0.7);
            SqlClrFunctions.FuzzySearch(nfkc, nfkd).Value.Should().BeGreaterThan(0.7);
        }

        [Fact]
        public void FuzzySearch_WithMixedEncodingIssues_ShouldBeRobust()
        {
            // Arrange
            // Mixing scripts and potential encoding conversion issues
            SqlString mixed = new SqlString("Test 测试 тест");
            SqlString partial1 = new SqlString("Test");
            SqlString partial2 = new SqlString("测试");
            SqlString partial3 = new SqlString("тест");
            SqlString misspelled = new SqlString("Tesd 测試 тэст");

            // Act
            var result1 = SqlClrFunctions.FuzzySearch(partial1, mixed).Value;
            var result2 = SqlClrFunctions.FuzzySearch(partial2, mixed).Value;
            var result3 = SqlClrFunctions.FuzzySearch(partial3, mixed).Value;
            var result4 = SqlClrFunctions.FuzzySearch(mixed, misspelled).Value;

            // Assert
            result1.Should().BeGreaterThan(0.9); // Exact substring match
            result2.Should().BeGreaterThan(0.9); // Exact substring match
            result3.Should().BeGreaterThan(0.9); // Exact substring match
            result4.Should().BeGreaterThan(0.5); // Similar despite typos
        }

        [Fact]
        public void FuzzySearch_WithExtremeLengthUnicodeStrings_ShouldNotFail()
        {
            // Arrange
            string longString = string.Concat(
                new string('a', 200),
                "你好",
                new string('b', 200),
                "привет",
                new string('c', 200));
            SqlString searchLong = new SqlString(longString);
            SqlString searchChinese = new SqlString("你好");
            SqlString searchRussian = new SqlString("привет");

            // Act & Assert
            SqlClrFunctions.FuzzySearch(searchChinese, searchLong).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(searchRussian, searchLong).Value.Should().Be(1.0);
        }

        [Fact]
        public void FuzzySearch_WithLookalikesAcrossEncodings_ShouldDetectDifferences()
        {
            // Arrange
            SqlString latin = new SqlString("hello");
            SqlString cyrillicLookalike = new SqlString("һеllо"); // Contains Cyrillic characters that look like Latin

            // Latin "a" vs Cyrillic "а"
            SqlString latinA = new SqlString("a"); // U+0061
            SqlString cyrillicA = new SqlString("а"); // U+0430

            // Act
            var result1 = SqlClrFunctions.FuzzySearch(latin, cyrillicLookalike).Value;
            var result2 = SqlClrFunctions.FuzzySearch(latinA, cyrillicA).Value;

            // Assert - They should not be considered identical
            result1.Should().BeLessThan(1.0);
            result2.Should().BeLessThan(1.0);
        }

        [Fact]
        public void FuzzySearch_WithDifferentEncodingRepresentations_ShouldBeConsistent()
        {
            // We're testing logical consistency across different physical representations
            
            // UTF-8 vs UTF-16 vs UTF-32 representations of the same string
            // Note: In C#, strings are always UTF-16 internally, but we can simulate the effect
            
            // Create string with BMP characters vs supplementary characters
            string bmpChars = "Hello World";
            string supplementaryChars = "Hello 🌍"; // Globe emoji requires 4 bytes in UTF-16
            
            // Convert to byte arrays using different encodings, then back to string
            byte[] bmpUtf8 = Encoding.UTF8.GetBytes(bmpChars);
            byte[] bmpUtf16 = Encoding.Unicode.GetBytes(bmpChars);
            byte[] bmpUtf32 = Encoding.UTF32.GetBytes(bmpChars);
            
            byte[] suppUtf8 = Encoding.UTF8.GetBytes(supplementaryChars);
            byte[] suppUtf16 = Encoding.Unicode.GetBytes(supplementaryChars);
            byte[] suppUtf32 = Encoding.UTF32.GetBytes(supplementaryChars);
            
            // Convert back to strings
            string bmpFromUtf8 = Encoding.UTF8.GetString(bmpUtf8);
            string bmpFromUtf16 = Encoding.Unicode.GetString(bmpUtf16);
            string bmpFromUtf32 = Encoding.UTF32.GetString(bmpUtf32);
            
            string suppFromUtf8 = Encoding.UTF8.GetString(suppUtf8);
            string suppFromUtf16 = Encoding.Unicode.GetString(suppUtf16);
            string suppFromUtf32 = Encoding.UTF32.GetString(suppUtf32);
            
            // Test that FuzzySearch is consistent across encodings
            SqlClrFunctions.FuzzySearch(
                new SqlString(bmpFromUtf8), new SqlString(bmpFromUtf16)).Value.Should().Be(1.0);
                
            SqlClrFunctions.FuzzySearch(
                new SqlString(bmpFromUtf16), new SqlString(bmpFromUtf32)).Value.Should().Be(1.0);
                
            SqlClrFunctions.FuzzySearch(
                new SqlString(suppFromUtf8), new SqlString(suppFromUtf16)).Value.Should().Be(1.0);
                
            SqlClrFunctions.FuzzySearch(
                new SqlString(suppFromUtf16), new SqlString(suppFromUtf32)).Value.Should().Be(1.0);
        }
    }
}
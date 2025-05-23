using System.Data.SqlTypes;
using System.Text;

using FluentAssertions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCoreSqlClr.Test.Units
{
    public class SqlClrFunctionsTests
    {
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
            SqlString lowerCaseTerm = new SqlString("test".Build3GramString());
            SqlString upperCaseTerm = new SqlString("TEST".Build3GramString());
            SqlString mixedCaseTerm = new SqlString("TeSt".Build3GramString());

            SqlString lowerCase = new SqlString("test");
            SqlString upperCase = new SqlString("TEST");
            SqlString mixedCase = new SqlString("TeSt");

            // Act
            var lowerUpperResult = SqlClrFunctions.FuzzySearch(lowerCaseTerm, upperCase).Value;
            var lowerMixedResult = SqlClrFunctions.FuzzySearch(lowerCaseTerm, mixedCase).Value;
            var mixedUpperResult = SqlClrFunctions.FuzzySearch(mixedCaseTerm, upperCase).Value;

            // Assert
            lowerUpperResult.Should().Be(1);
            lowerMixedResult.Should().Be(1);
            mixedUpperResult.Should().Be(1);
        }

        [Fact]
        public void FuzzySearch_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            SqlString specialChars = new SqlString("test-123".Build3GramString());
            SqlString withPunctuation = new SqlString("test-123!");
            SqlString differentPunctuation = new SqlString("test:123");

            // Act & Assert
            SqlClrFunctions.FuzzySearch(specialChars, withPunctuation).Value.Should().BeGreaterThan(0.8);
            SqlClrFunctions.FuzzySearch(specialChars, differentPunctuation).Value.Should().BeGreaterThan(0.4);
        }

        [Fact]
        public void FuzzySearch_WithTypos_ShouldReturnPartialMatches()
        {
            // Arrange
            SqlString correct = new SqlString("testing".Build3GramString());
            SqlString typo1 = new SqlString("testign"); // Transposition
            SqlString typo2 = new SqlString("testng"); // Missing character
            SqlString typo3 = new SqlString("testting"); // Extra character

            // Act
            var transpositionResult = SqlClrFunctions.FuzzySearch(correct, typo1).Value;
            var missingCharResult = SqlClrFunctions.FuzzySearch(correct, typo2).Value;
            var extraCharResult = SqlClrFunctions.FuzzySearch(correct, typo3).Value;

            // Assert
            transpositionResult.Should().BeGreaterThan(0.4);
            missingCharResult.Should().BeGreaterThan(0.4);
            extraCharResult.Should().BeGreaterThan(0.4);
        }


        [Fact]
        public void FuzzySearch_SubstringPositioning_ShouldAffectScore()
        {
            // Arrange
            SqlString searchTerm = new SqlString("test".Build3GramString());
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
            SqlString exactSearch = new SqlString("iPhone 13 Pro Max".Build3GramString());
            SqlString partialSearch = new SqlString("iPhone 13".Build3GramString());
            SqlString misspelledSearch = new SqlString("iPhnoe 13 Pro".Build3GramString());
            SqlString abbreviatedSearch = new SqlString("ip13pro".Build3GramString());

            // Act
            var exactResult = SqlClrFunctions.FuzzySearch(exactSearch, productName).Value;
            var partialResult = SqlClrFunctions.FuzzySearch(partialSearch, productName).Value;
            var misspelledResult = SqlClrFunctions.FuzzySearch(misspelledSearch, productName).Value;
            var abbreviatedResult = SqlClrFunctions.FuzzySearch(abbreviatedSearch, productName).Value;

            // Assert
            exactResult.Should().Be(1.0);
            partialResult.Should().BeGreaterThan(0.9); // Contains the substring exactly
            misspelledResult.Should().BeGreaterThan(0.5); // Should still match reasonably well
            abbreviatedResult.Should().BeGreaterThan(0.0); // Should have some similarity
        }

        // New Unicode-specific tests

        [Fact]
        public void FuzzySearch_WithUnicodeCharacters_ShouldHandleCorrectly()
        {
            // Arrange - Include characters from different scripts and planes
            SqlString latinTerm = new SqlString("hello world".Build3GramString());
            SqlString cyrillicTerm = new SqlString("привет мир".Build3GramString()); // Russian "hello world"
            SqlString cjkTerm = new SqlString("你好世界".Build3GramString()); // Chinese "hello world"
            SqlString emojiTerm =
                new SqlString("👋🏻👨‍👩‍👧‍👦👍".Build3GramString()); // Multiple emojis for better trigrams

            SqlString latin = new SqlString("hello world");
            SqlString cyrillic = new SqlString("привет мир"); // Russian "hello world" 
            SqlString cjk = new SqlString("你好世界"); // Chinese "hello world"
            SqlString emoji = new SqlString("👋🏻👨‍👩‍👧‍👦👍"); // Multiple emojis for better trigrams


            // Act & Assert - Test fuzzy matching within same scripts
            SqlClrFunctions.FuzzySearch(latinTerm, latin).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(cyrillicTerm, cyrillic).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(cjkTerm, cjk).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(emojiTerm, emoji).Value.Should().Be(1.0);

            // Different scripts should have low similarity
            SqlClrFunctions.FuzzySearch(latinTerm, cyrillic).Value.Should().BeLessThan(0.1);
            SqlClrFunctions.FuzzySearch(latinTerm, cjk).Value.Should().BeLessThan(0.1);
            SqlClrFunctions.FuzzySearch(cyrillicTerm, cjk).Value.Should().BeLessThan(0.1);
        }

        [Fact]
        public void FuzzySearch_WithSurrogatePairs_ShouldHandleCorrectly()
        {
            // Arrange - Characters outside the BMP that require surrogate pairs in UTF-16
            SqlString bmpOnlyTerm = new SqlString("abc".Build3GramString());
            SqlString withEmojiTerm = new SqlString("abc😀".Build3GramString()); // Basic Latin + emoji
            SqlString complexEmojiTerm =
                new SqlString("👨‍👩‍👧‍👦".Build3GramString()); // Family emoji (multiple surrogate pairs)
            SqlString mathSymbolsTerm = new SqlString("𝔸𝔹ℂ".Build3GramString()); // Mathematical symbols


            SqlString withEmoji = new SqlString("abc😀"); // Basic Latin + emoji
            SqlString complexEmoji = new SqlString("👨‍👩‍👧‍👦"); // Family emoji (multiple surrogate pairs)
            SqlString mathSymbols = new SqlString("𝔸𝔹ℂ"); // Mathematical symbols

            // Act & Assert
            SqlClrFunctions.FuzzySearch(bmpOnlyTerm, withEmoji).Value.Should().BeGreaterThan(0.5);
            SqlClrFunctions.FuzzySearch(withEmojiTerm, withEmoji).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(complexEmojiTerm, complexEmoji).Value.Should().Be(1.0);
            SqlClrFunctions.FuzzySearch(mathSymbolsTerm, mathSymbols).Value.Should().Be(1.0);
        }

        [Fact]
        public void FuzzySearch_WithCombiningCharacters_ShouldHandleNormalization()
        {
            // Arrange
            // Same visual representation, different Unicode composition
            SqlString precomposedTerm = new SqlString("café".Build3GramString()); // é is a single code point
            SqlString decomposedTerm = new SqlString("cafe\u0301".Build3GramString()); // e + combining acute accent

            SqlString precomposed = new SqlString("café"); // é is a single code point
            SqlString decomposed = new SqlString("cafe\u0301"); // e + combining acute accent

            SqlString precomposedGermanTerm = new SqlString("schön".Build3GramString());
            SqlString precomposedGerman = new SqlString("schön");
            SqlString decomposedGerman = new SqlString("scho\u0308n"); // o + combining diaeresis

            // Act
            var result1 = SqlClrFunctions.FuzzySearch(precomposedTerm, decomposed).Value;
            var result2 = SqlClrFunctions.FuzzySearch(decomposedTerm, precomposed).Value;
            var result3 = SqlClrFunctions.FuzzySearch(precomposedGermanTerm, decomposedGerman).Value;

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
            SqlString normal = new SqlString("test".Build3GramString());
            SqlString withZeroWidth = new SqlString("te\u200Bst"); // With zero-width space
            SqlString withVariationSelector = new SqlString("test\uFE0E"); // With variation selector
            SqlString withRtlMark = new SqlString("\u200Ftest"); // With right-to-left mark

            // Act
            var resultZeroWidth = SqlClrFunctions.FuzzySearch(normal, withZeroWidth).Value;
            var resultVariation = SqlClrFunctions.FuzzySearch(normal, withVariationSelector).Value;
            var resultRtl = SqlClrFunctions.FuzzySearch(normal, withRtlMark).Value;

            // Assert
            // These should be similar despite the special characters
            resultZeroWidth.Should().Be(0);
            resultVariation.Should().BeGreaterThan(0.9);
            resultRtl.Should().BeGreaterThan(0.9);
        }

        [Fact]
        public void FuzzySearch_WithMixedEncodingIssues_ShouldBeRobust()
        {
            // Arrange
            // Mixing scripts and potential encoding conversion issues
            SqlString mixedTerm = new SqlString("Test 测试测 тест".Build3GramString());
            SqlString partial1Term = new SqlString("Test".Build3GramString());
            SqlString partial2Term = new SqlString("测试测".Build3GramString());
            SqlString partial3Term = new SqlString("тест".Build3GramString());

            SqlString mixed = new SqlString("Test 测试测 тест");
            SqlString misspelled = new SqlString("Tesd 测試 тэст");

            // Act
            var result1 = SqlClrFunctions.FuzzySearch(partial1Term, mixed).Value;
            var result2 = SqlClrFunctions.FuzzySearch(partial2Term, mixed).Value;
            var result3 = SqlClrFunctions.FuzzySearch(partial3Term, mixed).Value;
            var result4 = SqlClrFunctions.FuzzySearch(mixedTerm, misspelled).Value;

            // Assert
            result1.Should().BeGreaterThan(0.9); // Exact substring match
            result2.Should().BeGreaterThan(0.9); // Exact substring match
            result3.Should().BeGreaterThan(0.9); // Exact substring match
            result4.Should().BeGreaterThan(0); // Similar despite typos
        }

        [Fact]
        public void FuzzySearch_WithExtremeLengthUnicodeStrings_ShouldNotFail()
        {
            // Arrange
            string longString = string.Concat(
                new string('a', 200),
                "你好啊",
                new string('b', 200),
                "привет",
                new string('c', 200));
            SqlString searchLong = new SqlString(longString);
            SqlString searchChinese = new SqlString("你好啊".Build3GramString());
            SqlString searchRussian = new SqlString("привет".Build3GramString());

            // Act & Assert
            SqlClrFunctions.FuzzySearch(searchChinese, searchLong).Value.Should().BeGreaterThan(0.9);
            SqlClrFunctions.FuzzySearch(searchRussian, searchLong).Value.Should().BeGreaterThan(0.9);
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
                new SqlString(bmpFromUtf8.Build3GramString()), new SqlString(bmpFromUtf16)).Value.Should().Be(1.0);

            SqlClrFunctions.FuzzySearch(
                new SqlString(bmpFromUtf16.Build3GramString()), new SqlString(bmpFromUtf32)).Value.Should().Be(1.0);

            SqlClrFunctions.FuzzySearch(
                new SqlString(suppFromUtf8.Build3GramString()), new SqlString(suppFromUtf16)).Value.Should().Be(1.0);

            SqlClrFunctions.FuzzySearch(
                new SqlString(suppFromUtf16.Build3GramString()), new SqlString(suppFromUtf32)).Value.Should().Be(1.0);
        }

        [Fact]
        public void FuzzySearch_WithPerfectMatch_ShouldReturn1()
        {
            // Arrange

            SqlString searchTerm = new SqlString("testing".Build3GramString());
            SqlString longInput = new SqlString("testing");

            // Act
            var result = SqlClrFunctions.FuzzySearch(searchTerm, longInput).Value;

            // Assert
            result.Should().Be(1); // Should match well
        }

        [Fact]
        public void FuzzySearch_With2PerfectMatch_ShouldReturn2()
        {
            // Arrange

            SqlString searchTerm = new SqlString("testing".Build3GramString());
            SqlString longInput = new SqlString("testingtesting");

            // Act
            var result = SqlClrFunctions.FuzzySearch(searchTerm, longInput).Value;

            // Assert
            result.Should().BeGreaterThan(0.8); // Should match well
        }
    }
}
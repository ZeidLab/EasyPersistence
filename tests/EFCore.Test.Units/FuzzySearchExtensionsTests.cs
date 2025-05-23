using System.Diagnostics.CodeAnalysis;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Helpers;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units;
[SuppressMessage("ConfigureAwait", "ConfigureAwaitEnforcer:ConfigureAwaitEnforcer")]
[SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
public sealed class FuzzySearchExtensionsTests
{
    [Fact]
    public async Task GetNGrams_whenThereIsValidInput_shouldReturnSetsOfGrams()
    {
        // Arrange
        var searchTerm = "123456789";
        // Act
        var result = searchTerm.Build3GramString();
        // Assert
        await Verify(result);
    }
}
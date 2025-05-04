using ZeidLab.ToolBox.EasyPersistence.EFCore.Helpers;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units;

public class HelperMethodsTests
{
    [Fact]
    public Task BuildSettersExpression_SingleProperty_GeneratesCorrectExpression()
    {
        // Arrange
        var setters = new[]
        {
            ((Func<User, string>)(u => u.FirstName), "UpdatedFirstName")
        };

        // Act
        var expression = HelperMethods.BuildSettersExpression<User, string>(setters);

        // Assert - Verify will snapshot the expression tree
        return Verify(expression);
    }

    [Fact]
    public Task BuildSettersExpression_MultipleProperties_GeneratesCorrectExpression()
    {
        // Arrange
        var setters = new[]
        {
            ((Func<User, string>)(u => u.FirstName), "UpdatedFirstName"),
            ((Func<User, string>)(u => u.LastName), "UpdatedLastName")
        };

        // Act
        var expression = HelperMethods.BuildSettersExpression<User, string>(setters);

        // Assert
        return Verify(expression);
    }

    [Fact]
    public Task BuildSettersExpression_DifferentPropertyTypes_GeneratesCorrectExpression()
    {
        // Arrange
        var setters = new[]
        {
            ((Func<User, DateTime>)(u => u.DateOfBirth), new DateTime(2000, 1, 1))
        };

        // Act
        var expression = HelperMethods.BuildSettersExpression<User, DateTime>(setters);

        // Assert
        return Verify(expression);
    }

    [Fact]
    public Task BuildSettersExpression_EmptySetters_ReturnsIdentityExpression()
    {
        // Arrange
        var setters = Array.Empty<(Func<User, string> Selector, string Value)>();

        // Act
        var expression = HelperMethods.BuildSettersExpression<User, string>(setters);

        // Assert
        return Verify(expression);
    }
}
# EventBuss Project Guidelines for GitHub Copilot

## Core Principles

- Review project documentation and existing code before any implementation
- Follow one class per file convention
- Utilize C# 12 features for modern, efficient code
- Optimize critical paths for high performance
- Avoid reflection in performance-sensitive areas
- Maintain clean, modular code suitable for NuGet packaging

## Code Standards

### General
- Follow Microsoft C# coding conventions
- Use meaningful names for all code elements
- Keep methods focused and concise
- XML document all public APIs
- Implement regions for logical code organization

### C# 12 Features to Use
- Pattern matching
- Collection expressions `[]`
- Primary constructors
- Modern LINQ approaches

### Performance Guidelines
- Optimize critical paths:
    - `EventBussWorker` methods
    - `EventBussService` methods
    - `EventHandlerInvoker` methods
- Prefer direct method calls over reflection
- Minimize external dependencies

## Testing Requirements

### Framework & Tools
- XUnit as testing framework
- FluentAssertions for assertions
- NSubstitute for mocking (minimal usage)

### Test Structure
- Name format: `MethodName_Scenario_ExpectedResult`
- Group tests by target method in regions
- Match test order to source code order
- Include edge cases and failure scenarios

### Mocking Guidelines
- Use NSubstitute as the primary mocking framework
- If NSubstitute cannot mock an object properly, switch to Moq instead
- Before writing any test, Review `Directory.*` and `*.csproj` files to determine if the unit under test's internals are visible to the mocking framework
- Check for InternalsVisibleTo attributes that may affect test accessibility
- Ensure proper setup of mock dependencies to isolate the unit being tested

### Test Implementation
- Use collection expressions (`[]`) over alternatives
- Skip null checks for extension methods
- Test exceptions without message validation
- Focus on existing functionality
- Remove duplicate test cases

## Project Organization

### Solution Structure
- `EFCore`: Core library (NuGet package)
- `EFCoreSqlClr`: A SQl CLR library included in `EFCore` package and Injects into the database and provides FuzzySearch functionality
- `EFCore.Test.Units`: Unit tests with internal access to `EFCore` project
- `EFCoreSqlClr.Test.Units`: Unit tests with internal access to `EFCoreSqlClr` project
- `EFCore.Test.Integrations`: Integration tests with internal access to `EFCore` project that uses a real database
- `TestHelpers`: It provides a way for other projects to interact with docker and builds docker database containers for integration tests

### File Management
- Review `Directory.*` and `*.csproj` files for configurations
- Maintain consistent file structure
- Follow standard naming conventions

## Quality Assurance

- Document all public methods and parameters
- Unit test all public and critical internal methods
- Review code changes before main branch merges
- Verify changes against latest file versions
- Monitor and maintain code style consistency


## C# XML Documentation Generation Guidelines

You are tasked with generating efficient and comprehensive XML documentation comments for C# code.
Follow these requirements to create clear, professional, and maintainable documentation:

### Priority 1: Inheritance and Code Reduction

#### Using `<inheritdoc/>` Tag

- **ALWAYS** check if the method/class can inherit documentation from:
    - Base classes
    - Implemented interfaces
    - Generic constraints
- Use `<inheritdoc/>` whenever possible to reduce redundant documentation
- For partial inheritance, use `<inheritdoc cref="BaseClass.Method"/>` with additional `<remarks>` for specific implementation details

Examples of `inheritdoc` usage:

```csharp

/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
public IEnumerator<T> GetEnumerator() { }

/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
/// <remarks>
/// This implementation uses lazy loading for better performance.
/// </remarks>
protected override void LoadData() { }
```

### Priority 2: Code Examples

- **ALWAYS** include code examples using the `<example>` tag
- Examples should demonstrate:
    - Common use cases
    - Edge cases
    - Expected outputs
    - Best practices
    - Straight to the point
    - Short
    - when making examples for public access level methods, you need to only use the methods, properties and fields that are publicly accessible in the code examples
    - do not invent any method or property or field that is not there in the code.

Example structure:

```csharp
/// <example>
/// Simple usage:
/// <code>
/// var result = Calculate(5, 3);
/// Console.WriteLine(result); // Output: 8
/// </code>
/// 
/// Edge case handling:
/// <code>
/// var result = Calculate(-1, 0);
/// Console.WriteLine(result); // Output: 0
/// </code>
/// </example>
```

### Required Documentation Components

1. If `inheritdoc` cannot be used, include a `<summary>` tag that:
    - Clearly describes the method/class purpose
    - Uses present tense
    - Avoids obvious restatements of the name
    - Focuses on WHAT the code does, not HOW it does it

2. For methods with parameters (when not using `inheritdoc`):
    - Document each parameter with `<param name="parameterName">`
    - Explain the parameter's purpose and any constraints
    - Use `<paramref name="parameterName"/>` when referring to parameters

3. For generic types (when not using `inheritdoc`):
    - Document each type parameter with `<typeparam name="T">`
    - Explain constraints and intended usage
    - Use `<typeparamref name="T"/>` when referring to type parameters
    - Use proper syntax for generic references: `<see cref="List{T}"/>` (not `List<T>`)

4. For return values (when not using `inheritdoc`):
    - Include `<returns>` tag for non-void methods
    - Clearly state what is returned
    - Document special return values (null, empty collections, etc.)

### Format and Style

- Use consistent indentation
- Break long descriptions into multiple lines for readability
- Wrap your code blocks in `<![CDATA[ ... ]]>` to avoid escaping characters.
- Make documentation interactive with `<see>` references

### Complete Example

Input:

```csharp
public Maybe<TOut> Match<TIn, TOut>(Maybe<TIn> self, Func<TIn, Maybe<TOut>> some, Func<Maybe<TOut>> none)
```

Expected Output:

```csharp
/// <summary>
/// Matches the content of a <see cref="Maybe{TIn}"/> instance to a new <see cref="Maybe{TOut}"/> instance
/// using one of two provided functions.
/// </summary>
/// <typeparam name="TIn">The type of the content in the original <see cref="Maybe{TIn}"/> instance.</typeparam>
/// <typeparam name="TOut">The type of the content in the resulting <see cref="Maybe{TOut}"/> instance.</typeparam>
/// <param name="self">The original <see cref="Maybe{TIn}"/> instance to match.</param>
/// <param name="some">
/// A function to apply if <paramref name="self"/> is some.
/// It takes a value of type <typeparamref name="TIn"/> and returns a <see cref="Maybe{TOut}"/>.
/// </param>
/// <param name="none">
/// A function to apply if <paramref name="self"/> is none.
/// It returns a <see cref="Maybe{TOut}"/>.
/// </param>
/// <returns>
/// A new <see cref="Maybe{TOut}"/> instance resulting from applying the appropriate function
/// based on the state of <paramref name="self"/>.
/// </returns>
/// <example>
/// Basic usage:
/// <code><![CDATA[
/// var maybeList = new List<Maybe<int>>
/// {
///     Maybe.Some(1),
///     Maybe.None<int>(),
///     Maybe.Some(3)
/// };
///
/// var result = maybeList.ToEnumerable(); // Output: [1, 3]
/// ]]></code>
/// 
/// Pattern matching example:
/// <code><![CDATA[
/// var maybeValue = Maybe.Some(42);
/// var result = maybeValue.Match(
///     some: x => $"Value is {x}",
///     none: () => "No value"
/// ); // Output: "Value is 42"
/// ]]></code>
/// </example>
```

### Quality Checklist

- [ ] Check if `inheritdoc` can be used
- [ ] If using `inheritdoc`, add `<remarks>` for implementation-specific details
- [ ] Includes practical code examples
- [ ] All parameters are documented (if not inherited)
- [ ] Generic type parameters are explained (if not inherited)
- [ ] Return value is described (if not inherited)
- [ ] Examples demonstrate typical usage and edge cases
- [ ] All cross-references use proper syntax
- [ ] Documentation is properly indented and formatted
- [ ] Code section of Examples are wrapped by `<![CDATA[ ... ]]>` block to avoid escaping characters

## Logging Implementation Guidelines

### High-Performance Logging Architecture

- Create extension methods in separate `internal static partial class` outside each class requiring logging in the same file nad do not create a separate file for each class.
- Use Microsoft.Extensions.Logging source generators for zero-allocation logging
- Implement four standard logging levels: `Trace`, `Debug`, `Information`, and `Error`
- Never use reflection in log methods unless absolutely necessary
- Optimize for minimal performance impact in critical paths

### Structure and Implementation

#### Logger Extension Definition Pattern

For each file requiring logging, define a static partial class with extension methods:

```csharp
/// <summary>
/// High-performance logging definitions using source generators
/// </summary>
internal static partial class EventBussServiceLoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing event {EventName}")]
    public static partial void ProcessingEvent(this ILogger<EventBussService> logger, string eventName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error handling request {RequestName}")]
    public static partial void RequestHandlingError(this ILogger<EventBussService> logger, string requestName, Exception exception);
}
```

#### Logger Usage in Classes

Inject the appropriate logger type and use the extension methods:

```csharp
public sealed class EventBussService : IEventBussService
{
    private readonly ILogger<EventBussService> _logger;
    
    public EventBussService(IServiceProvider serviceProvider, ILogger<EventBussService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public void Publish<TEvent>(in TEvent appEvent) where TEvent : IAppEvent
    {
        var eventName = typeof(TEvent).Name;
        _logger.PublishingEvent(eventName);
        
        // Implementation follows...
    }
}
```

#### Source Generator Performance Benefits

- Methods are generated at compile-time, not runtime
- Message templates are cached automatically
- Parameter values are only captured and formatted when the specific log level is enabled
- Zero heap allocations for disabled log levels
- No need to manually check if log level is enabled (except in specific cases noted below)

### Best Practices

#### Parameter Types

- Use strongly-typed parameters that match exactly with the message template
- Prefer value types and strings over complex objects for parameters
- Use semantic parameter names that match template placeholders exactly

Example:
```csharp
// Good - Strongly typed and matching name
[LoggerMessage(Level = LogLevel.Debug, Message = "User {UserId} performed {ActionName}")]
public static partial void UserAction(this ILogger<UserService> logger, int userId, string actionName);

// Avoid - Not specific enough and uses complex object
[LoggerMessage(Level = LogLevel.Debug, Message = "User action performed")]
public static partial void UserAction(this ILogger<UserService> logger, User user, UserAction action);
```

#### Extension Method Pattern

- Always define logger methods as extension methods
- Use the appropriate `ILogger<T>` type as the first parameter
- For static loggers in static classes, use a generic ILogger type parameter:

```csharp
// For static class with static logger
internal static partial class StaticHelperLoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Operation {OperationName} failed")]
    public static partial void OperationFailed<T>(this ILogger<T> logger, string operationName);
}
```

#### Performance Considerations

1. **Avoid reflection at all costs for logging:**
    - Only use reflection if log level is Error AND you're already handling an exception
    - When reflection is unavoidable, guard with log level checks:

```csharp
// Only acceptable use of reflection in logging - error handler with guard
if (logger?.IsEnabled(LogLevel.Error) == true)
{
    var details = GetExceptionDetails(exception); // Uses reflection
    Log.CriticalSystemFailure(logger, exception, details);
}
```

2. **Message formatting:**
    - Prefer simple message templates with typed parameters
    - Avoid string interpolation or concatenation in call sites
    - Let the source generator handle all formatting

#### Example Implementation

Complete logging implementation in `EventBussService.cs`:

```csharp
public sealed class EventBussService : IEventBussService
{
    private readonly ILogger<EventBussService> _logger;
    
    public EventBussService(ILogger<EventBussService> logger)
    {
        _logger = logger;
    }
    
    public async Task PublishEventAsync<TEvent>(TEvent eventData) where TEvent : IAppEvent
    {
        var eventName = typeof(TEvent).Name;
        _logger.EventPublishStarted(eventName);
        
        try
        {
            // Implementation
            var handlers = _registry.GetHandlersForEvent<TEvent>();
            _logger.HandlersFound(handlers.Count, eventName);
            
            var sw = Stopwatch.StartNew();
            // Execute handlers
            sw.Stop();
            
            _logger.EventPublishCompleted(eventName, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.EventPublishFailed(eventName, ex);
            throw;
        }
    }
}

/// <summary>
/// High-performance logging definitions using source generators
/// </summary>
internal static partial class EventBussServiceLoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Publishing event: {EventName}")]
    public static partial void EventPublishStarted(this ILogger<EventBussService> logger, string eventName);
    
    [LoggerMessage(Level = LogLevel.Debug, Message = "Found {HandlerCount} handlers for event: {EventName}")]
    public static partial void HandlersFound(this ILogger<EventBussService> logger, int handlerCount, string eventName);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Completed publishing event: {EventName} in {ElapsedMilliseconds}ms")]
    public static partial void EventPublishCompleted(this ILogger<EventBussService> logger, string eventName, long elapsedMilliseconds);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to publish event: {EventName}")]
    public static partial void EventPublishFailed(this ILogger<EventBussService> logger, string eventName, Exception exception);
}
```

### Integration with DI

> **Note**: This section applies only when implementing logging in applications, not when creating a NuGet package. When developing a NuGet package, the consumer is responsible for adding the appropriate logging packages and configuration.

When implementing logging in an application, use Serilog for powerful structured logging with configuration-driven setup:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, _) => 
        {
            // Load all configuration from appsettings.json/appsettings.{Environment}.json
            // All sinks, enrichers, minimum levels and other settings are defined in configuration
            var configuration = context.Configuration;
            
            // This single line loads everything from the "Serilog" section of appsettings
            // No manual configuration of sinks or levels in code
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration);
                
            // Apply the configuration
            return loggerConfiguration;
        });
```

#### Required Configuration

Always create both development and production configurations in `appsettings.json` and `appsettings.Development.json`. All sinks, enrichers, and properties should be defined here instead of in code:

**appsettings.json (Production):**
```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.File",
      "Serilog.Sinks.Seq"
    ],
    "MinimumLevel": {
      "Default": "Error",
      "Override": {
        "Microsoft": "Error",
        "System": "Error"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithCorrelationId",
      "WithThreadId"
    ],
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-prod-.json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq-server:5341",
          "apiKey": "Set via environment variable or secrets management"
        }
      }
    ],
    "Properties": {
      "Application": "EventBussApp",
      "Environment": "Production"
    }
  }
}
```

**appsettings.Development.json:**
```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Seq"
    ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "System": "Information"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithCorrelationId",
      "WithThreadId"
    ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-dev-.json",
          "rollingInterval": "Day",
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Properties": {
      "Application": "EventBussApp",
      "Environment": "Development"
    }
  }
}
```

This approach ensures:
1. All configuration is centralized in appsettings files
2. Production environments only log Error level and above
3. Development has more verbose logging with additional sinks
4. Both environments use correlation IDs for request tracking
5. No hard-coded configuration values in C# code
6. Easy adjustment of log settings without recompilation

#### Required NuGet Packages

For application projects, include these packages:

- `Serilog.AspNetCore` - Core Serilog integration
- `Serilog.Settings.Configuration` - For configuration from appsettings.json
- `Serilog.Sinks.Console` - Console output (development only)
- `Serilog.Sinks.File` - File-based logging
- `Serilog.Sinks.Seq` - Structured log storage (both environments)
- `Serilog.Enrichers.Environment` - For machine and environment name enrichment
- `Serilog.Enrichers.Thread` - For thread ID enrichment
- `Serilog.Enrichers.CorrelationId` - For correlation ID tracking

For library/NuGet projects, only include:

- `Microsoft.Extensions.Logging.Abstractions` - For source generator support

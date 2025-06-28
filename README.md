# ZeidLab.ToolBox.EventBuss

## 🤔 What is `EasyPersistence` Library?

**EasyPersistence** is a high-performance, modular extension for Entity Framework Core, providing advanced repository and unit of work patterns, fuzzy search with 3-gram algorithm, and SQL CLR integration. Designed for scalable, maintainable, and testable .NET data layers, it follows modern C# and Microsoft coding standards, with comprehensive XML documentation and NuGet-ready modularity.

### 🎁 Features

- **Repository & Unit of Work Patterns:** Clean abstractions for data access
- **Fuzzy Search:** Advanced search capabilities for EF Core with 3-gram algorithm
- **SQL CLR Integration:** Efficient search via SQL CLR functions
- **High Performance:** Optimized for critical paths, minimal overhead

[^ Back To Top](#-what-is-EasyPersistence-library)

## 📦 Installation

To use **EasyPersistence.EFCore** in your project, you can install it via NuGet:

```bash
dotnet add package ZeidLab.ToolBox.EasyPersistence.EFCore
```

For more information, please visit [EventBuss Package on NuGet](https://www.nuget.org/packages/ZeidLab.ToolBox.EasyPersistence.EFcore).

[^ Back To Top](#-what-is-EasyPersistence-library)

### Core Components

| Component                           | Description                                                    |
|-------------------------------------|----------------------------------------------------------------|
| `IAggregateRoot`                    | Marker for aggregate root entities in the domain model         |
| `IDomainEvent`                      | Marker for domain events in DDD                                |
| `IRepositoryBase`                   | Generic repository interface for entity data access            |
| `IUnitOfWork`                       | Unit of work contract for managing transactions and saving     |
| `EntityBase<TId>`                   | Base class for entities with typed ID and value-based equality |
| `PagedResult<T>`                    | Represents a paged result set with items and total count       |
| `PropertyScore`                     | Represents the score of a property in fuzzy search/comparison  |
| `ScoredRecord<TEntity>`             | Entity with fuzzy search score and per-property scores         |
| `UnitOfWorkBase<TContext>`          | Base implementation of IUnitOfWork for EF Core DbContext       |
| `RepositoryBase<TEntity,TEntityId>` | Base implementation of IRepositoryBase using EF Core           |

[^ Back To Top](#-what-is-EasyPersistence-library)

## 📝 ChangeLogs

With each release, we add new features and fix bugs. You can find the full changelog at [EasyPersistence Releases](https://github.com/ZeidLab/EasyPersistence/releases).

[^ Back To Top](#-what-is-EasyPersistence-library)

## 📖 Usage and Configuration

For more information and detailed usage instructions, please refer to the [EasyPersistence Documentation](https://github.com/ZeidLab/EasyPersistence/index.md).

[^ Back To Top](#-what-is-EasyPersistence-library)


## ⭐️ Star and Follow

Star this repository and follow me on GitHub to stay informed about new releases and updates. Your support fuels this
project's growth!

[^ Back To Top](#-what-is-EasyPersistence-library)

## 💡 Love My Work? Support the Journey!

If my content adds value to your projects, consider supporting me via crypto.

- **Bitcoin:** bc1qlfljm9mysdtu064z5cf4yq4ddxgdfztgvghw3w
- **USDT(TRC20):** TJFME9tAnwdnhmqGHDDG5yCs617kyQDV39

Thank you for being part of this community—let’s build smarter, together

[^ Back To Top](#-what-is-EasyPersistence-library)

## 🤝 Contributions

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

1. Fork the repository
2. Create a new branch for your feature or bugfix
3. Commit your changes following the project guidelines
4. Push your branch and submit a pull request

[^ Back To Top](#-what-is-EasyPersistence-library)

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE.txt) file for details.

[^ Back To Top](#-what-is-EasyPersistence-library)

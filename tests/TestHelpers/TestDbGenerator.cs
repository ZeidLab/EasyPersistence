using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace ZeidLab.ToolBox.EasyPersistence.TestHelpers;

public sealed class TestDbGenerator : IAsyncDisposable
{
    private const string WebAppImageName = "web-app-container:latest";
    private readonly IContainer? _mongoContainer;
    private readonly IContainer? _sqlServerContainer;
    private readonly IContainer? _redisContainer;
    private readonly IContainer? _appContainer;
    private readonly INetwork _appNetwork;

   // private readonly IFutureDockerImage _futureImage;


    private bool _isRunning = false;
    private bool _isStopping = false;
    private bool _isAboutToRun = false;

    public readonly bool HasMongodb;
    public readonly bool HasRedis;
    public readonly bool HasWebApp;
    public readonly bool HasSqlServer;

    public string? MongoConnectionString { get; private set; }
    public readonly string? MongoConnectionStringInternal;
    public string? RedisConnectionString { get; private set; }
    public readonly string? RedisConnectionStringInternal;
    public string? WebAppHttpUrl { get; private set; }
    public string? WebAppHttpsUrl { get; private set; }
    public string? SqlServerConnectionString { get; private set; }
    public readonly string? SqlServerConnectionStringInternal;

    private TestDbGenerator(bool mongoDb = false, bool redis = false, bool webApp = false, bool sqlServer = false)
    {
        HasMongodb = mongoDb;
        HasRedis = redis;
        HasWebApp = webApp;
        HasSqlServer = sqlServer;
        var mongoNetworkAliases = Guid.NewGuid().ToString("D");
        var redisNetworkAliases = Guid.NewGuid().ToString("D");
        var sqlServerNetworkAliases = Guid.NewGuid().ToString("D");
        _appNetwork = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();
        // var solutionDir = CommonDirectoryPath.GetSolutionDirectory();
        // _futureImage = new ImageFromDockerfileBuilder()
        //     //.WithDeleteIfExists(false)
        //     .WithCleanUp(true)
        //     .WithName("web-app-container:latest")
        //     .WithDockerfileDirectory(solutionDir, Path.Combine("src", "WebApp"))
        //     .WithDockerfile("Dockerfile")
        //     .Build();
        if (mongoDb)
        {
            _mongoContainer = new ContainerBuilder()
                .WithName(Guid.NewGuid().ToString("D"))
                .WithImage("mongo")
                .WithPortBinding(27017, true)
                .WithNetwork(_appNetwork)
                .WithNetworkAliases(mongoNetworkAliases)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017)).Build();

            MongoConnectionStringInternal = $"mongodb://{mongoNetworkAliases}:27017/testDb";
        }

        if (redis)
        {
            _redisContainer = new ContainerBuilder()
                .WithImage("redis")
                .WithPortBinding(6379, true)
                .WithNetwork(_appNetwork)
                .WithNetworkAliases(redisNetworkAliases)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379)).Build();
            RedisConnectionStringInternal = $"{redisNetworkAliases}:6379";
        }

        if (sqlServer)
        {
            _sqlServerContainer = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPortBinding(1433, true)
                .WithNetwork(_appNetwork)
                .WithNetworkAliases(sqlServerNetworkAliases)
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", "Test@Pass12345")
                .WithEnvironment("MSSQL_PID", "Developer")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();

            SqlServerConnectionStringInternal =
                $"Server={sqlServerNetworkAliases};Database=WebAppDb;User Id=sa;Password=Test@Pass12345;TrustServerCertificate=true;Encrypt=false;";
        }

        if (webApp)
        {
            _appContainer = new ContainerBuilder()
                .WithImage(WebAppImageName)
                .WithPortBinding(8080, true)
                .WithPortBinding(8081, true)
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("CONNECTIONSTRINGS__SQLSERVER", SqlServerConnectionStringInternal)
                .WithNetwork(_appNetwork)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
                .Build();
        }
    }

    private static string ToDockerPath(string windowsAbsolutePath)
    {
        var parts = windowsAbsolutePath.Split(':');
        if (string.IsNullOrEmpty(windowsAbsolutePath) || parts.Length != 2)
            throw new InvalidOperationException(
                $"value provided is not a windows absolute path : {windowsAbsolutePath}");
        var lastValue = $"//{parts[0].ToLowerInvariant()}/{parts[1].Replace('\\', '/').Trim('/')}";
        return lastValue;
    }

    public static TestDbGenerator GenerateMongoDbOnly()
    {
        return new TestDbGenerator(mongoDb:true);
    }

    public static TestDbGenerator GenerateRedisOnly()
    {
        return new TestDbGenerator(redis:true);
    }

    public static TestDbGenerator GenerateMongoAndRedis()
    {
        return new TestDbGenerator(mongoDb:true, redis:true);
    }

    public static TestDbGenerator GenerateWebAppWithRedis()
    {
        return new TestDbGenerator(sqlServer:true,redis: true, webApp:true);
    }

    public static TestDbGenerator GenerateWebAppWithoutRedis()
    {
        return new TestDbGenerator(sqlServer:true, webApp:true);
    }

    public static TestDbGenerator GenerateSqlServerOnly()
    {
        return new TestDbGenerator(sqlServer: true);
    }

    public async ValueTask MakeSureIsRunningAsync()
    {
        if (_isRunning || _isAboutToRun)
            return;
        if (!_isRunning)
            await StartAsync();
    }

    public async ValueTask MakeSureIsStoppedAsync()
    {
        if (!_isRunning || _isStopping)
            return;
        if (_isRunning)
            await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isStopping)
            return;
        try
        {
            _isStopping = true;
            if (HasMongodb && _mongoContainer is not null)
                await _mongoContainer.DisposeAsync();
            if (HasRedis && _redisContainer is not null)
                await _redisContainer.DisposeAsync();
            if (HasSqlServer && _sqlServerContainer is not null)
                await _sqlServerContainer.DisposeAsync();
            if (HasWebApp && _appContainer is not null)
                await _appContainer.DisposeAsync();
            await _appNetwork.DeleteAsync();
        }
        finally
        {
            _isStopping = false;
            _isRunning = false;
        }
    }

    private async ValueTask StartAsync()
    {
        if (_isAboutToRun)
            return;
        try
        {
            _isAboutToRun = true;
            await _appNetwork.CreateAsync();
            if (HasMongodb && _mongoContainer is not null)
            {
                await _mongoContainer.StartAsync();
                MongoConnectionString = $"mongodb://localhost:{_mongoContainer.GetMappedPublicPort(27017)}/testDb";
            }

            if (HasRedis && _redisContainer is not null)
            {
                await _redisContainer.StartAsync();
                RedisConnectionString = $"localhost:{_redisContainer.GetMappedPublicPort(6379)}";
            }

            if (HasSqlServer && _sqlServerContainer is not null)
            {
                await _sqlServerContainer.StartAsync();
                SqlServerConnectionString =
                    $"Server=localhost,{_sqlServerContainer.GetMappedPublicPort(1433)};Database=WebAppDb;User Id=sa;Password=Test@Pass12345;TrustServerCertificate=true;Encrypt=false;";
            }

            if (HasWebApp && _appContainer is not null)
            {
                //await _futureImage.CreateAsync();
                await _appContainer.StartAsync();
                WebAppHttpUrl = $"http://localhost:{_appContainer.GetMappedPublicPort(8080)}";
                WebAppHttpsUrl = $"http://localhost:{_appContainer.GetMappedPublicPort(8081)}";
            }

            _isRunning = true;
        }
        catch (Exception)
        {
            _isRunning = false;
            throw;
        }
        finally
        {
            _isAboutToRun = false;
        }
    }
}
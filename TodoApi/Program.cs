using Microsoft.EntityFrameworkCore;
using TodoApi.Services;
using TodoApi.Data;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetSection("Redis")["Connection"];

if (string.IsNullOrEmpty(redisConnection))
{
    throw new InvalidOperationException("Строка подключения Redis не настроена.");
}

// Настройка Kestrel для прослушивания порта 80
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP REST API на 80 порту
    options.ListenAnyIP(80);

    // gRPC на 5001 порту с HTTP/2
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Регистрация Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация DbContext
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Настройка конвертера для enum
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new
        System.Text.Json.Serialization.JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy =
    System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Регистрация Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});

builder.Services.AddSingleton<RedisCacheService>();

// Регистрация IConnectionMultiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection));

// Регистрация RabbitMqService
builder.Services.AddSingleton<RabbitMqService>();

// Регистрация gRPC сервиса
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// Добавление контроллеров
builder.Services.AddControllers();

// Добавление сервиса для миграции
builder.Services.AddScoped<MigrationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1"));
    app.MapGrpcReflectionService();
}

// Автоматическое применение миграций с повторными попытками подключения к БД
using var scope = app.Services.CreateScope();
var migrationService = scope.ServiceProvider.GetRequiredService<MigrationService>();
await migrationService.ApplyMigrationsWithRetryAsync();

app.MapGrpcService<TodoAnalyticsService>();

app.UseAuthorization();

app.MapControllers();

app.Run();

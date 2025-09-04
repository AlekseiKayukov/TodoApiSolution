using Microsoft.EntityFrameworkCore;
using TodoApi.Services;
using TodoApi.Data;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetSection("Redis")["Connection"];

if (string.IsNullOrEmpty(redisConnection))
{
    throw new InvalidOperationException("������ ����������� Redis �� ���������.");
}

// ��������� Kestrel ��� ������������� ����� 80
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP REST API �� 80 �����
    options.ListenAnyIP(80);

    // gRPC �� 5001 ����� � HTTP/2
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// ����������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ����������� DbContext
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// ��������� ���������� ��� enum
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new
        System.Text.Json.Serialization.JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy =
    System.Text.Json.JsonNamingPolicy.CamelCase;
});

// ����������� Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});

builder.Services.AddSingleton<RedisCacheService>();

// ����������� IConnectionMultiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection));

// ����������� RabbitMqService
builder.Services.AddSingleton<RabbitMqService>();

// ����������� gRPC �������
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// ���������� ������������
builder.Services.AddControllers();

// ���������� ������� ��� ��������
builder.Services.AddScoped<MigrationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1"));
    app.MapGrpcReflectionService();
}

// �������������� ���������� �������� � ���������� ��������� ����������� � ��
using var scope = app.Services.CreateScope();
var migrationService = scope.ServiceProvider.GetRequiredService<MigrationService>();
await migrationService.ApplyMigrationsWithRetryAsync();

app.MapGrpcService<TodoAnalyticsService>();

app.UseAuthorization();

app.MapControllers();

app.Run();

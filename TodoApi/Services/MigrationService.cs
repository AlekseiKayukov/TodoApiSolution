using Microsoft.EntityFrameworkCore;
using Npgsql;
using TodoApi.Data;

namespace TodoApi.Services
{
    /// <summary>
    /// Сервис для применения миграций базы данных с повторными попытками подключения.
    /// Обеспечивает готовность БД перед обработкой запросов приложения.
    /// </summary>
    public class MigrationService
    {
        private readonly TodoDbContext _dbContext;
        private readonly ILogger<MigrationService> _logger;
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="MigrationService"/>.
        /// Получает необходимые сервисы через внедрение зависимостей.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных <see cref="TodoDbContext"/>.</param>
        /// <param name="logger">Логгер для записи информационных сообщений и предупреждений.</param>
        /// <param name="configuration">Конфигурация приложения для получения строки подключения.</param>
        public MigrationService(TodoDbContext dbContext, ILogger<MigrationService> logger,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("Postgres");
        }

        /// <summary>
        /// Асинхронно применяет миграции базы данных, выполняя повторные попытки подключения
        /// в случае временной недоступности БД.
        /// </summary>
        /// <param name="maxRetries">Максимальное количество повторных попыток (по умолчанию 30).</param>
        /// <param name="delaySeconds">Задержка между попытками в секундах (по умолчанию 2).</param>
        /// <returns>Задача, представляющая асинхронную операцию применения миграций.</returns>
        public async Task ApplyMigrationsWithRetryAsync(int maxRetries = 30, 
            int delaySeconds = 2)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync();

                    await _dbContext.Database.MigrateAsync();

                    _logger.LogInformation("Миграции базы данных успешно применены.");
                    break; // Завершение цикла при успехе
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"База данных недоступна (попытка " +
                        $"{i + 1}/{maxRetries}): {ex.Message}");

                    if (i == maxRetries - 1) 
                        throw; // Выброс исключения, если попытка закончились

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }
    }
}

using TodoApi.Data;
using Grpc.Core;
using TodoApi.Models;
using TodoApi.Grpc;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Services
{
    /// <summary>
    /// Реализация gRPC сервиса для предоставления статистики по задачам.
    /// Класс наследует сгенерированный базовый класс TodoAnalyticsBase из proto файла.
    /// </summary>
    public class TodoAnalyticsService : TodoAnalytics.TodoAnalyticsBase
    {
        private readonly TodoDbContext _dbContext;

        /// <summary>
        /// Конструктор, получает контекст базы данных через внедрение зависимостей.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных <see cref="TodoDbContext"/>.</param>
        public TodoAnalyticsService(TodoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Обработка вызова gRPC метода GetStats.
        /// Асинхронно подсчитывает количество задач по статусам и возвращает
        /// результат в объекте <see cref="StatsResponse"/>.
        /// </summary>
        /// <param name="request">Запрос, пока пустой <see cref="StatsRequest"/>.</param>
        /// <param name="context">Контекст вызова gRPC.</param>
        /// <returns>Ответ с количеством активных и завершенных задач.</returns>
        public override async Task<StatsResponse> GetStats(StatsRequest request,
            ServerCallContext context)
        {
            int activeTaskCount = await _dbContext.Tasks.CountAsync(t =>
                t.Status == TodoTaskStatus.Active);

            int completedTasksCount = await _dbContext.Tasks.CountAsync(t =>
                t.Status == TodoTaskStatus.Completed);

            return new StatsResponse
            {
                ActiveTasks = activeTaskCount,
                CompletedTasks = completedTasksCount
            };
        }
    }
}

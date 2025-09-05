using TodoApi.Data;
using Grpc.Core;
using TodoApi.Enum;
using TodoApi.Grpc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Interface;

namespace TodoApi.Services
{
    /// <summary>
    /// Реализация gRPC‑сервиса аналитики задач.
    /// Считает агрегаты по статусам через <see cref="ITodoTaskRepository"/>.
    /// Наследуется от сгенерированного базового класса <see cref="TodoAnalytics.TodoAnalyticsBase"/>.
    /// </summary>
    public class TodoAnalyticsService : TodoAnalytics.TodoAnalyticsBase
    {
        private readonly ITodoTaskRepository _repository;

        /// <summary>
        /// Создаёт экземпляр сервиса аналитики.
        /// Получает зависимости через внедрение зависимостей.
        /// </summary>
        /// <param name="repository">Репозиторий задач для выполнения агрегирующих запросов.</param>
        public TodoAnalyticsService(ITodoTaskRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Обрабатывает gRPC‑вызов получения агрегированной статистики.
        /// Асинхронно подсчитывает количество задач по статусам.
        /// </summary>
        /// <param name="request">Запрос (без параметров).</param>
        /// <param name="context">Контекст вызова gRPC.</param>
        /// <returns><see cref="StatsResponse"/> с количеством активных и завершённых задач.</returns>
        public override async Task<StatsResponse> GetStats(StatsRequest request,
            ServerCallContext context)
        {
            int activeTaskCount = await _repository.CountByStatusAsync(TodoTaskStatus.Active);

            int completedTasksCount = await _repository.CountByStatusAsync(TodoTaskStatus.Completed);

            return new StatsResponse
            {
                ActiveTasks = activeTaskCount,
                CompletedTasks = completedTasksCount
            };
        }
    }
}

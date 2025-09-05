using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TodoApi.DTO;
using TodoApi.Enum;
using TodoApi.Interface;
using TodoApi.Models;
using TodoApi.Models.Mappings;

namespace TodoApi.Services
{
    /// <summary>
    /// Доменный сервис задач.
    /// Инкапсулирует бизнес-логику, работу с репозиторием, кэширование чтений
    /// и публикацию событий при изменении статуса.
    /// </summary>
    public class TodoTaskService : ITodoTaskService
    {
        private readonly ITodoTaskRepository _repository;
        private readonly RedisCacheService _redisCacheService;
        private readonly RabbitMqService _rabbitMqService;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Создаёт экземпляр сервиса задач.
        /// Получает зависимости через внедрение зависимостей.
        /// </summary>
        /// <param name="repository">Репозиторий задач (абстракция доступа к данным).</param>
        /// <param name="redisCacheService">Сервис инвалидации кэша по паттерну ключей.</param>
        /// <param name="rabbitMqService">Сервис публикации событий в RabbitMQ.</param>
        /// <param name="cache">Распределённый кэш для кеширования результатов чтения.</param>
        public TodoTaskService(
            ITodoTaskRepository repository,
            RedisCacheService redisCacheService,
            RabbitMqService rabbitMqService,
            IDistributedCache cache)
        {
            _repository = repository;
            _redisCacheService = redisCacheService;
            _rabbitMqService = rabbitMqService;
            _cache = cache;
        }

        /// <summary>
        /// Создаёт новую задачу и инвалидирует связанные ключи кэша.
        /// </summary>
        /// <param name="newTask">Доменная сущность задачи для создания.</param>
        /// <returns>Созданная сущность <see cref="TodoTask"/>.</returns>
        public async Task<TodoTask> CreateTaskAsync(TodoTask newTask)
        {
            newTask.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(newTask);

            await InvalidateCachesAsync();
            return newTask;
        }

        /// <summary>
        /// Обновляет существующую задачу.
        /// При изменении статуса публикует событие и инвалидирует кэш.
        /// </summary>
        /// <param name="id">Идентификатор обновляемой задачи.</param>
        /// <param name="updateTask">Новые значения полей задачи.</param>
        /// <returns>true — если задача обновлена; false — если не найдена или id не совпал.</returns>
        public async Task<bool> UpdateTaskAsync(int id, TodoTask updateTask)
        {
            if (id != updateTask.Id)
                return false;

            var existingTask = await _repository.GetByIdAsync(id);
            if (existingTask == null)
                return false;

            if (existingTask.Status != updateTask.Status)
            {
                _rabbitMqService.PublishTaskStatusChanged(updateTask.Id,
                    updateTask.Status.ToString().ToLower());
            }

            existingTask.Title = updateTask.Title;
            existingTask.Description = updateTask.Description;
            existingTask.Status = updateTask.Status;
            await _repository.UpdateAsync(existingTask);

            await InvalidateCachesAsync();
            return true;
        }

        /// <summary>
        /// Удаляет задачу и инвалидирует кэш.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>true — если задача удалена; false — если задача не найдена.</returns>
        public async Task<bool> DeleteTaskAsync(int id)
        {
            var existingTask = await _repository.GetByIdAsync(id);
            if (existingTask == null)
                return false;

            await _repository.RemoveAsync(existingTask);

            await InvalidateCachesAsync();
            return true;
        }

        /// <summary>
        /// Инвалидирует ключи кэша, связанные со списками и элементами задач.
        /// Вызывается после операций мутации.
        /// </summary>
        private async Task InvalidateCachesAsync()
        {
            await _redisCacheService.RemoveKeysByPatternAsync("task-*");
            await _redisCacheService.RemoveKeysByPatternAsync("tasks-*");
        }

        /// <summary>
        /// Возвращает пагинированный список задач (DTO) с фильтрацией и поиском.
        /// Результат кэшируется в <see cref="IDistributedCache"/> по составному ключу.
        /// </summary>
        /// <param name="search">Поиск по названию (регистронезависимый). Если null — без фильтра.</param>
        /// <param name="status">Фильтр по статусу. Если null — без фильтра.</param>
        /// <param name="page">Номер страницы (начиная с 1).</param>
        /// <param name="pageSize">Размер страницы (количество элементов).</param>
        /// <returns><see cref="PagedResultDto{TodoTaskDto}"/> со списком и метаданными пагинации.</returns>
        public async Task<PagedResultDto<TodoTaskDto>> GetTasksAsync(string? search, TodoTaskStatus? status, int page, int pageSize)
        {
            string cacheKey = $"tasks-{search}-{status}-{page}-{pageSize}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<PagedResultDto<TodoTaskDto>>(cachedData);
                if (cachedResult != null)
                    return cachedResult;
            }

            var (totalItems, entities) = await _repository.GetPagedAsync(search, status, page, pageSize);
            var items = entities.Select(t => t.ToDto()).ToList();

            var result = new PagedResultDto<TodoTaskDto>
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = items
            };

            var serializedResult = JsonSerializer.Serialize(result);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, serializedResult, cacheOptions);

            return result;
        }

        /// <summary>
        /// Возвращает задачу (DTO) по идентификатору.
        /// Результат кэшируется; при наличии в кэше возвращается без запроса к БД.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns><see cref="TodoTaskDto"/> или null, если задача не найдена.</returns>
        public async Task<TodoTaskDto?> GetTaskByIdAsync(int id)
        {
            string cacheKey = $"task-{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<TodoTaskDto>(cachedData);
                if (cachedResult != null)
                    return cachedResult;
            }

            var entity = await _repository.GetByIdAsync(id, asNoTracking: true);
            var task = entity == null ? null : entity.ToDto();

            if (task == null)
                return null;

            var serializedTask = JsonSerializer.Serialize(task);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, serializedTask, cacheOptions);

            return task;
        }
    }
}



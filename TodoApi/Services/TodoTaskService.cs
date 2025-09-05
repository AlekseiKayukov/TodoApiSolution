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
    /// �������� ������ �����.
    /// ������������� ������-������, ������ � ������������, ����������� ������
    /// � ���������� ������� ��� ��������� �������.
    /// </summary>
    public class TodoTaskService : ITodoTaskService
    {
        private readonly ITodoTaskRepository _repository;
        private readonly RedisCacheService _redisCacheService;
        private readonly RabbitMqService _rabbitMqService;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// ������ ��������� ������� �����.
        /// �������� ����������� ����� ��������� ������������.
        /// </summary>
        /// <param name="repository">����������� ����� (���������� ������� � ������).</param>
        /// <param name="redisCacheService">������ ����������� ���� �� �������� ������.</param>
        /// <param name="rabbitMqService">������ ���������� ������� � RabbitMQ.</param>
        /// <param name="cache">������������� ��� ��� ����������� ����������� ������.</param>
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
        /// ������ ����� ������ � ������������ ��������� ����� ����.
        /// </summary>
        /// <param name="newTask">�������� �������� ������ ��� ��������.</param>
        /// <returns>��������� �������� <see cref="TodoTask"/>.</returns>
        public async Task<TodoTask> CreateTaskAsync(TodoTask newTask)
        {
            newTask.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(newTask);

            await InvalidateCachesAsync();
            return newTask;
        }

        /// <summary>
        /// ��������� ������������ ������.
        /// ��� ��������� ������� ��������� ������� � ������������ ���.
        /// </summary>
        /// <param name="id">������������� ����������� ������.</param>
        /// <param name="updateTask">����� �������� ����� ������.</param>
        /// <returns>true � ���� ������ ���������; false � ���� �� ������� ��� id �� ������.</returns>
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
        /// ������� ������ � ������������ ���.
        /// </summary>
        /// <param name="id">������������� ������.</param>
        /// <returns>true � ���� ������ �������; false � ���� ������ �� �������.</returns>
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
        /// ������������ ����� ����, ��������� �� �������� � ���������� �����.
        /// ���������� ����� �������� �������.
        /// </summary>
        private async Task InvalidateCachesAsync()
        {
            await _redisCacheService.RemoveKeysByPatternAsync("task-*");
            await _redisCacheService.RemoveKeysByPatternAsync("tasks-*");
        }

        /// <summary>
        /// ���������� �������������� ������ ����� (DTO) � ����������� � �������.
        /// ��������� ���������� � <see cref="IDistributedCache"/> �� ���������� �����.
        /// </summary>
        /// <param name="search">����� �� �������� (�������������������). ���� null � ��� �������.</param>
        /// <param name="status">������ �� �������. ���� null � ��� �������.</param>
        /// <param name="page">����� �������� (������� � 1).</param>
        /// <param name="pageSize">������ �������� (���������� ���������).</param>
        /// <returns><see cref="PagedResultDto{TodoTaskDto}"/> �� ������� � ����������� ���������.</returns>
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
        /// ���������� ������ (DTO) �� ��������������.
        /// ��������� ����������; ��� ������� � ���� ������������ ��� ������� � ��.
        /// </summary>
        /// <param name="id">������������� ������.</param>
        /// <returns><see cref="TodoTaskDto"/> ��� null, ���� ������ �� �������.</returns>
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



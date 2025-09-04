using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    /// <summary>
    /// API контроллер для управления задачами.
    /// Предоставляет CRUD операции, поиск, фильтрацию и пагинацию.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TodoDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly RedisCacheService _redisCacheService;
        private readonly RabbitMqService _rabbitMqService;

        /// <summary>
        /// Конструктор контроллера с внедрением DbContext и кеша.
        /// </summary>
        /// <param name="context">Контекст БД TodoDbContext.</param>
        /// <param name="cache">Redis-кеш</param>
        /// <param name="redisCacheService">Очистка кеша по паттерну</param>
        public TasksController(TodoDbContext context, IDistributedCache cache,
            RedisCacheService redisCacheService, RabbitMqService rabbitMqService)
        {
            _context = context;
            _cache = cache;
            _redisCacheService = redisCacheService;
            _rabbitMqService = rabbitMqService;
        }

        /// <summary>
        /// Получение списка задач с опциональным поиском по названию,
        /// фильтрацией по статусу и пагинацией.
        /// </summary>
        /// <param name="search">Текст для поиска в название задачи.</param>
        /// <param name="status">Фильтр по статусу задачи (active или completed).</param>
        /// <param name="page">Номер страницы для пагинации (по умолчанию 1).</param>
        /// <param name="pageSize">Размер содержимого страницы (по умолчанию 10).</param>
        /// <returns>Пагинированный список задач с информацией о количестве.</returns>
        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] string? search = null,
            [FromQuery] TodoTaskStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            string cacheKey = $"tasks-{search}-{status}-{page}-{pageSize}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<object>(cachedData);
                return Ok(cachedResult);
            }

            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status.Equals(status));
            }

            var totalItems = await query.CountAsync();

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = tasks
            };

            var serializedResult = JsonSerializer.Serialize(result);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, serializedResult, cacheOptions);

            return Ok(result);
        }

        /// <summary>
        /// Получение задачи по ее идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>Задача с соответствующим идентификатором, 
        /// иначе статус 404, если не найдена.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            string cacheKey = $"task-{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<object>(cachedData);
                return Ok(cachedResult);
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            var serializedResult = JsonSerializer.Serialize(task);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey,serializedResult, cacheOptions);

            return Ok(task);
        }

        /// <summary>
        /// Создание новой задачи.
        /// </summary>
        /// <param name="newTask">Объект задачи для создания.</param>
        /// <returns>Созданная задача с присвоенным Id.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TodoTask newTask)
        {
            newTask.CreatedAt = DateTime.UtcNow;
            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            await _redisCacheService.RemoveKeysByPatternAsync("task-*");
            await _redisCacheService.RemoveKeysByPatternAsync("tasks-*");

            return CreatedAtAction(nameof(GetTask), new { id = newTask.Id }, newTask);
        }

        /// <summary>
        /// Обновление существующей задачи по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи для обновления.</param>
        /// <param name="updateTask">Обновленный объект задачи.</param>
        /// <returns>Статус 204 No Content при успешном обновление, 
        /// 400 - если идентификатор не совпадает,
        /// 404 - если задача не найдена.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, 
            [FromBody] TodoTask updateTask)
        {
            if (id != updateTask.Id)
                return BadRequest();

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
                return NotFound();

            if (existingTask.Status != updateTask.Status)
            {
                _rabbitMqService.PublishTaskStatusChanged(updateTask.Id, 
                    updateTask.Status.ToString().ToLower());
            }

            existingTask.Title = updateTask.Title;
            existingTask.Description = updateTask.Description;
            existingTask.Status = updateTask.Status;
            await _context.SaveChangesAsync();

            await _redisCacheService.RemoveKeysByPatternAsync("task-*");
            await _redisCacheService.RemoveKeysByPatternAsync("tasks-*");

            return NoContent();
        }

        /// <summary>
        /// Удаление задачи по ее идентификатору.
        /// </summary>
        /// <param name="id">Id задачи для удаления.</param>
        /// <returns>Статус 204 No Content при успешном удаление,
        /// 404 - если задача не найдена.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
                return NotFound();

            _context.Tasks.Remove(existingTask);
            await _context.SaveChangesAsync();

            await _redisCacheService.RemoveKeysByPatternAsync("task-*");
            await _redisCacheService.RemoveKeysByPatternAsync("tasks-*");

            return NoContent();
        }
    }
}

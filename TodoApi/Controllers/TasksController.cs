using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using TodoApi.Data;
using TodoApi.DTO;
using TodoApi.Enum;
using TodoApi.Interface;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    /// <summary>
    /// API контроллер для управления задачами.
    /// Выполняет CRUD, поиск, фильтрацию и пагинацию, возвращая DTO.
    /// Вся бизнес-логика и работа с инфраструктурой делегируются в <see cref="ITodoTaskService"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITodoTaskService _taskService;

        /// <summary>
        /// Конструктор контроллера. Получает зависимости из DI.
        /// </summary>
        /// <param name="taskService">Доменный сервис задач.</param>
        public TasksController(ITodoTaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Возвращает пагинированный список задач (DTO) с поиском и фильтрацией по статусу.
        /// </summary>
        /// <param name="search">Поиск по названию (регистронезависимый).</param>
        /// <param name="status">Фильтр по статусу (Active или Completed).</param>
        /// <param name="page">Номер страницы (>= 1, по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (>= 1, по умолчанию 10).</param>
        /// <returns><see cref="PagedResultDto{TodoTaskDto}"/> с метаданными пагинации и списком элементов.</returns>
        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] string? search = null,
            [FromQuery] TodoTaskStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _taskService.GetTasksAsync(search, status, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Возвращает задачу (DTO) по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>200 с <see cref="TodoTaskDto"/>, либо 404 если не найдена.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        /// <summary>
        /// Создает новую задачу.
        /// </summary>
        /// <param name="newTask">Доменная модель задачи.</param>
        /// <returns>201 Созданная задача с присвоенными данными.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TodoTask newTask)
        {
            var created = await _taskService.CreateTaskAsync(newTask);

            var dto = new TodoTaskDto
            {
                Id = created.Id,
                Title = created.Title,
                Description = created.Description,
                Status = created.Status,
                CreatedAt = created.CreatedAt
            };

            return CreatedAtAction(nameof(GetTask), new { id = created.Id }, dto);
        }

        /// <summary>
        /// Обновляет существующую задачу по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <param name="updateTask">Новые значения полей задачи.</param>
        /// <returns>204 No Content при успехе, 404 если задача не найдена.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, 
            [FromBody] TodoTask updateTask)
        {
            var updated = await _taskService.UpdateTaskAsync(id, updateTask);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Удаляет задачу по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>204 No Content при успехе, 404 если задача не найдена.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}

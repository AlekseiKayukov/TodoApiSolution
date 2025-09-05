using TodoApi.DTO;
using TodoApi.Enum;
using TodoApi.Models;

namespace TodoApi.Interface
{
    /// <summary>
    /// Доменный сервис задач. Инкапсулирует бизнес-логику,
    /// работу с репозиторием, кэшированием и инфраструктурными событиями.
    /// </summary>
    public interface ITodoTaskService
    {
        /// <summary>
        /// Создаёт новую задачу.
        /// Устанавливает системные поля (например, дату создания) и инициирует инвалидацию кэша.
        /// </summary>
        /// <param name="newTask">Доменная сущность задачи для создания.</param>
        /// <returns>Созданная сущность <see cref="TodoTask"/>.</returns>
        Task<TodoTask> CreateTaskAsync(TodoTask newTask);

        /// <summary>
        /// Обновляет существующую задачу.
        /// При изменении статуса публикует событие и инвалидирует кэш.
        /// </summary>
        /// <param name="id">Идентификатор обновляемой задачи.</param>
        /// <param name="updateTask">Новые значения полей задачи.</param>
        /// <returns>true — если обновление выполнено; false — если задача не найдена или id не совпал.</returns>
        Task<bool> UpdateTaskAsync(int id, TodoTask updateTask);

        /// <summary>
        /// Удаляет задачу и инвалидирует кэш.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>true — если удаление выполнено; false — если задача не найдена.</returns>
        Task<bool> DeleteTaskAsync(int id);

        /// <summary>
        /// Возвращает пагинированный список задач с фильтрацией по статусу и поиском по названию.
        /// Результат кэшируется для оптимизации повторных запросов.
        /// </summary>
        /// <param name="search">Поиск по названию (регистронезависимый). Если null — без фильтра.</param>
        /// <param name="status">Фильтр по статусу. Если null — без фильтра.</param>
        /// <param name="page">Номер страницы (начиная с 1).</param>
        /// <param name="pageSize">Размер страницы (количество элементов).</param>
        /// <returns>Пагинированный результат с элементами типа <see cref="TodoTaskDto"/>.</returns>
        Task<PagedResultDto<TodoTaskDto>> GetTasksAsync(string? search, TodoTaskStatus? status, int page, int pageSize);

        /// <summary>
        /// Возвращает задачу по идентификатору.
        /// Результат может браться из кэша.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns><see cref="TodoTaskDto"/> или null, если не найдена.</returns>
        Task<TodoTaskDto?> GetTaskByIdAsync(int id);
    }
}
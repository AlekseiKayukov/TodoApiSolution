using TodoApi.Enum;
using TodoApi.Models;

namespace TodoApi.Interface
{
    /// <summary>
    /// Репозиторий задач. Инкапсулирует доступ к хранилищу данных для сущности <see cref="TodoTask"/>.
    /// Предоставляет операции чтения с фильтрацией/пагинацией и базовые CRUD-методы.
    /// </summary>
    public interface ITodoTaskRepository
    {
        /// <summary>
        /// Возвращает страницу задач с общим количеством элементов, с учётом поиска и фильтра по статусу.
        /// </summary>
        /// <param name="search">Поиск по названию (регистронезависимый). Если null — без фильтра.</param>
        /// <param name="status">Фильтр по статусу. Если null — без фильтра.</param>
        /// <param name="page">Номер страницы (начиная с 1).</param>
        /// <param name="pageSize">Размер страницы (кол-во элементов).</param>
        /// <returns>
        /// Кортеж: <c>totalItems</c> — общее кол-во элементов без учёта пагинации,
        /// <c>items</c> — элементы текущей страницы.
        /// </returns>
        Task<(int totalItems, List<TodoTask> items)> GetPagedAsync(string? search, TodoTaskStatus? status, int page, int pageSize);

        /// <summary>
        /// Возвращает задачу по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <param name="asNoTracking">Если true — чтение без отслеживания (read-only сценарии).</param>
        /// <returns>Найденная <see cref="TodoTask"/> или null, если не существует.</returns>
        Task<TodoTask?> GetByIdAsync(int id, bool asNoTracking = false);

        /// <summary>
        /// Добавляет новую задачу в хранилище и сохраняет изменения.
        /// </summary>
        /// <param name="task">Сущность задачи для добавления.</param>
        Task AddAsync(TodoTask task);

        /// <summary>
        /// Обновляет существующую задачу и сохраняет изменения.
        /// </summary>
        /// <param name="task">Сущность задачи с изменёнными полями.</param>
        Task UpdateAsync(TodoTask task);

        /// <summary>
        /// Удаляет задачу из хранилища и сохраняет изменения.
        /// </summary>
        /// <param name="task">Сущность задачи для удаления.</param>
        Task RemoveAsync(TodoTask task);

        /// <summary>
        /// Возвращает количество задач по заданному статусу.
        /// </summary>
        /// <param name="status">Статус задачи.</param>
        /// <returns>Число задач с указанным статусом.</returns>
        Task<int> CountByStatusAsync(TodoTaskStatus status);

        /// <summary>
        /// Асинхронно сохраняет все изменения, внесённые в текущий контекст базы данных.
        /// </summary>
        /// <returns>
        /// Возвращает количество записей, затронутых в базе данных.
        /// </returns>
        /// <remarks>
        /// Этот метод вызывает метод SaveChangesAsync контекста данных EF Core.
        /// Используется для применения внесённых изменений (добавления, обновления, удаления) к базе данных.
        /// </remarks>
        Task<int> SaveChangesAsync();
    }
}
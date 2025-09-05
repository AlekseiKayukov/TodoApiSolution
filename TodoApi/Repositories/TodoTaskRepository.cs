using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Enum;
using TodoApi.Interface;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    /// <summary>
    /// Репозиторий для работы с сущностью задач <see cref="TodoTask"/>.
    /// Инкапсулирует доступ к базе данных и операции чтения/записи.
    /// </summary>
    public class TodoTaskRepository : ITodoTaskRepository
    {
        private readonly TodoDbContext _context;

        /// <summary>
        /// Создаёт экземпляр репозитория задач.
        /// </summary>
        /// <param name="context">Экземпляр контекста базы данных.</param>
        public TodoTaskRepository(TodoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает страницу задач с учётом поиска и фильтрации по статусу.
        /// </summary>
        /// <param name="search">Поиск по названию (регистронезависимый). Если null — без фильтра.</param>
        /// <param name="status">Фильтр по статусу. Если null — без фильтра.</param>
        /// <param name="page">Номер страницы (начиная с 1).</param>
        /// <param name="pageSize">Размер страницы (количество элементов).</param>
        /// <returns>
        /// Кортеж: <c>totalItems</c> — общее количество элементов,
        /// <c>items</c> — элементы текущей страницы.
        /// </returns>
        public async Task<(int totalItems, List<TodoTask> items)> GetPagedAsync(string? search, TodoTaskStatus? status, int page, int pageSize)
        {
            var query = _context.Tasks.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status.Equals(status));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (totalItems, items);
        }

        /// <summary>
        /// Возвращает задачу по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <param name="asNoTracking">Если true — чтение без отслеживания изменений.</param>
        /// <returns>Найденная задача или null, если не существует.</returns>
        public async Task<TodoTask?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var set = asNoTracking ? _context.Tasks.AsNoTracking() : _context.Tasks;
            return await set.FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Добавляет новую задачу и сохраняет изменения.
        /// </summary>
        /// <param name="task">Сущность задачи для добавления.</param>
        public async Task AddAsync(TodoTask task)
        {
            _context.Tasks.Add(task);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Обновляет существующую задачу и сохраняет изменения.
        /// </summary>
        /// <param name="task">Сущность задачи с изменёнными полями.</param>
        public async Task UpdateAsync(TodoTask task)
        {
            _context.Tasks.Update(task);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Удаляет задачу и сохраняет изменения.
        /// </summary>
        /// <param name="task">Сущность задачи для удаления.</param>
        public async Task RemoveAsync(TodoTask task)
        {
            _context.Tasks.Remove(task);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Возвращает количество задач с указанным статусом.
        /// </summary>
        /// <param name="status">Статус задач для подсчёта.</param>
        /// <returns>Число задач в данном статусе.</returns>
        public Task<int> CountByStatusAsync(TodoTaskStatus status)
        {
            return _context.Tasks.AsNoTracking().CountAsync(t => t.Status == status);
        }

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
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Enum;
using TodoApi.Interface;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    /// <summary>
    /// ����������� ��� ������ � ��������� ����� <see cref="TodoTask"/>.
    /// ������������� ������ � ���� ������ � �������� ������/������.
    /// </summary>
    public class TodoTaskRepository : ITodoTaskRepository
    {
        private readonly TodoDbContext _context;

        /// <summary>
        /// ������ ��������� ����������� �����.
        /// </summary>
        /// <param name="context">��������� ��������� ���� ������.</param>
        public TodoTaskRepository(TodoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ���������� �������� ����� � ������ ������ � ���������� �� �������.
        /// </summary>
        /// <param name="search">����� �� �������� (�������������������). ���� null � ��� �������.</param>
        /// <param name="status">������ �� �������. ���� null � ��� �������.</param>
        /// <param name="page">����� �������� (������� � 1).</param>
        /// <param name="pageSize">������ �������� (���������� ���������).</param>
        /// <returns>
        /// ������: <c>totalItems</c> � ����� ���������� ���������,
        /// <c>items</c> � �������� ������� ��������.
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
        /// ���������� ������ �� ��������������.
        /// </summary>
        /// <param name="id">������������� ������.</param>
        /// <param name="asNoTracking">���� true � ������ ��� ������������ ���������.</param>
        /// <returns>��������� ������ ��� null, ���� �� ����������.</returns>
        public async Task<TodoTask?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var set = asNoTracking ? _context.Tasks.AsNoTracking() : _context.Tasks;
            return await set.FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// ��������� ����� ������ � ��������� ���������.
        /// </summary>
        /// <param name="task">�������� ������ ��� ����������.</param>
        public async Task AddAsync(TodoTask task)
        {
            _context.Tasks.Add(task);
            await SaveChangesAsync();
        }

        /// <summary>
        /// ��������� ������������ ������ � ��������� ���������.
        /// </summary>
        /// <param name="task">�������� ������ � ���������� ������.</param>
        public async Task UpdateAsync(TodoTask task)
        {
            _context.Tasks.Update(task);
            await SaveChangesAsync();
        }

        /// <summary>
        /// ������� ������ � ��������� ���������.
        /// </summary>
        /// <param name="task">�������� ������ ��� ��������.</param>
        public async Task RemoveAsync(TodoTask task)
        {
            _context.Tasks.Remove(task);
            await SaveChangesAsync();
        }

        /// <summary>
        /// ���������� ���������� ����� � ��������� ��������.
        /// </summary>
        /// <param name="status">������ ����� ��� ��������.</param>
        /// <returns>����� ����� � ������ �������.</returns>
        public Task<int> CountByStatusAsync(TodoTaskStatus status)
        {
            return _context.Tasks.AsNoTracking().CountAsync(t => t.Status == status);
        }

        /// <summary>
        /// ���������� ��������� ��� ���������, �������� � ������� �������� ���� ������.
        /// </summary>
        /// <returns>
        /// ���������� ���������� �������, ���������� � ���� ������.
        /// </returns>
        /// <remarks>
        /// ���� ����� �������� ����� SaveChangesAsync ��������� ������ EF Core.
        /// ������������ ��� ���������� �������� ��������� (����������, ����������, ��������) � ���� ������.
        /// </remarks>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
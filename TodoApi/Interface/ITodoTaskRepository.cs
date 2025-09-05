using TodoApi.Enum;
using TodoApi.Models;

namespace TodoApi.Interface
{
    /// <summary>
    /// ����������� �����. ������������� ������ � ��������� ������ ��� �������� <see cref="TodoTask"/>.
    /// ������������� �������� ������ � �����������/���������� � ������� CRUD-������.
    /// </summary>
    public interface ITodoTaskRepository
    {
        /// <summary>
        /// ���������� �������� ����� � ����� ����������� ���������, � ������ ������ � ������� �� �������.
        /// </summary>
        /// <param name="search">����� �� �������� (�������������������). ���� null � ��� �������.</param>
        /// <param name="status">������ �� �������. ���� null � ��� �������.</param>
        /// <param name="page">����� �������� (������� � 1).</param>
        /// <param name="pageSize">������ �������� (���-�� ���������).</param>
        /// <returns>
        /// ������: <c>totalItems</c> � ����� ���-�� ��������� ��� ����� ���������,
        /// <c>items</c> � �������� ������� ��������.
        /// </returns>
        Task<(int totalItems, List<TodoTask> items)> GetPagedAsync(string? search, TodoTaskStatus? status, int page, int pageSize);

        /// <summary>
        /// ���������� ������ �� ��������������.
        /// </summary>
        /// <param name="id">������������� ������.</param>
        /// <param name="asNoTracking">���� true � ������ ��� ������������ (read-only ��������).</param>
        /// <returns>��������� <see cref="TodoTask"/> ��� null, ���� �� ����������.</returns>
        Task<TodoTask?> GetByIdAsync(int id, bool asNoTracking = false);

        /// <summary>
        /// ��������� ����� ������ � ��������� � ��������� ���������.
        /// </summary>
        /// <param name="task">�������� ������ ��� ����������.</param>
        Task AddAsync(TodoTask task);

        /// <summary>
        /// ��������� ������������ ������ � ��������� ���������.
        /// </summary>
        /// <param name="task">�������� ������ � ���������� ������.</param>
        Task UpdateAsync(TodoTask task);

        /// <summary>
        /// ������� ������ �� ��������� � ��������� ���������.
        /// </summary>
        /// <param name="task">�������� ������ ��� ��������.</param>
        Task RemoveAsync(TodoTask task);

        /// <summary>
        /// ���������� ���������� ����� �� ��������� �������.
        /// </summary>
        /// <param name="status">������ ������.</param>
        /// <returns>����� ����� � ��������� ��������.</returns>
        Task<int> CountByStatusAsync(TodoTaskStatus status);

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
        Task<int> SaveChangesAsync();
    }
}
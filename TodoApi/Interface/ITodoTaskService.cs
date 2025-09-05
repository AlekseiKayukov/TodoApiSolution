using TodoApi.DTO;
using TodoApi.Enum;
using TodoApi.Models;

namespace TodoApi.Interface
{
    /// <summary>
    /// �������� ������ �����. ������������� ������-������,
    /// ������ � ������������, ������������ � ����������������� ���������.
    /// </summary>
    public interface ITodoTaskService
    {
        /// <summary>
        /// ������ ����� ������.
        /// ������������� ��������� ���� (��������, ���� ��������) � ���������� ����������� ����.
        /// </summary>
        /// <param name="newTask">�������� �������� ������ ��� ��������.</param>
        /// <returns>��������� �������� <see cref="TodoTask"/>.</returns>
        Task<TodoTask> CreateTaskAsync(TodoTask newTask);

        /// <summary>
        /// ��������� ������������ ������.
        /// ��� ��������� ������� ��������� ������� � ������������ ���.
        /// </summary>
        /// <param name="id">������������� ����������� ������.</param>
        /// <param name="updateTask">����� �������� ����� ������.</param>
        /// <returns>true � ���� ���������� ���������; false � ���� ������ �� ������� ��� id �� ������.</returns>
        Task<bool> UpdateTaskAsync(int id, TodoTask updateTask);

        /// <summary>
        /// ������� ������ � ������������ ���.
        /// </summary>
        /// <param name="id">������������� ������.</param>
        /// <returns>true � ���� �������� ���������; false � ���� ������ �� �������.</returns>
        Task<bool> DeleteTaskAsync(int id);

        /// <summary>
        /// ���������� �������������� ������ ����� � ����������� �� ������� � ������� �� ��������.
        /// ��������� ���������� ��� ����������� ��������� ��������.
        /// </summary>
        /// <param name="search">����� �� �������� (�������������������). ���� null � ��� �������.</param>
        /// <param name="status">������ �� �������. ���� null � ��� �������.</param>
        /// <param name="page">����� �������� (������� � 1).</param>
        /// <param name="pageSize">������ �������� (���������� ���������).</param>
        /// <returns>�������������� ��������� � ���������� ���� <see cref="TodoTaskDto"/>.</returns>
        Task<PagedResultDto<TodoTaskDto>> GetTasksAsync(string? search, TodoTaskStatus? status, int page, int pageSize);

        /// <summary>
        /// ���������� ������ �� ��������������.
        /// ��������� ����� ������� �� ����.
        /// </summary>
        /// <param name="id">������������� ������.</param>
        /// <returns><see cref="TodoTaskDto"/> ��� null, ���� �� �������.</returns>
        Task<TodoTaskDto?> GetTaskByIdAsync(int id);
    }
}
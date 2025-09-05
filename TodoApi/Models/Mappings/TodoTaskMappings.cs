using TodoApi.DTO;

namespace TodoApi.Models.Mappings
{
    /// <summary>
    /// ���������� ��� �������� �������� ������ ������ � DTO.
    /// �������� ������ ��� ������������� �������� ��������� � ������� �������� ������.
    /// </summary>
    public static class TodoTaskMappings
    {
        /// <summary>
        /// ����������� �������� ������ � DTO ��� ������� API.
        /// </summary>
        /// <param name="entity">�������� �������� ������.</param>
        /// <returns>��������� <see cref="TodoTaskDto"/>.</returns>
        public static TodoTaskDto ToDto(this TodoTask entity)
        {
            return new TodoTaskDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt
            };
        }

        /// <summary>
        /// ��������� �������� ������� ����� �� DTO �� ������ ���� ������.
        /// �������� ��� EF Core � �������� ����� ��������� �������� (��� �������� ������ �����).
        /// </summary>
        /// <param name="query">������ � �������.</param>
        /// <returns><see cref="IQueryable{T}"/> � ���������� ���� <see cref="TodoTaskDto"/>.</returns>
        public static IQueryable<TodoTaskDto> SelectDto(this IQueryable<TodoTask> query)
        {
            return query.Select(t => new TodoTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            });
        }
    }
}
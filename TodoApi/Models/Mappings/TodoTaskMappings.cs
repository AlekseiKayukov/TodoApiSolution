using TodoApi.DTO;

namespace TodoApi.Models.Mappings
{
    /// <summary>
    /// Расширения для маппинга доменной модели задачи в DTO.
    /// Содержит методы для единообразной проекции сущностей в объекты передачи данных.
    /// </summary>
    public static class TodoTaskMappings
    {
        /// <summary>
        /// Преобразует сущность задачи в DTO для ответов API.
        /// </summary>
        /// <param name="entity">Доменная сущность задачи.</param>
        /// <returns>Экземпляр <see cref="TodoTaskDto"/>.</returns>
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
        /// Выполняет проекцию запроса задач на DTO на уровне базы данных.
        /// Подходит для EF Core — проекция будет выполнена сервером (без загрузки лишних полей).
        /// </summary>
        /// <param name="query">Запрос к задачам.</param>
        /// <returns><see cref="IQueryable{T}"/> с элементами типа <see cref="TodoTaskDto"/>.</returns>
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
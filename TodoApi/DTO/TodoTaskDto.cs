using TodoApi.Enum;

namespace TodoApi.DTO
{
    /// <summary>
    /// DTO задачи для ответов API.
    /// Содержит основные данные о задаче без навигационных свойств и EF‑специфики.
    /// </summary>
    public class TodoTaskDto
    {
        /// <summary>
        /// Уникальный идентификатор задачи.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Заголовок (краткое название) задачи.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Подробное описание задачи (необязательно).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Текущий статус задачи.
        /// </summary>
        public TodoTaskStatus Status { get; set; }

        /// <summary>
        /// Время создания задачи в UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TodoApi.Enum;

namespace TodoApi.Models
{
    /// <summary>
    /// Модель задачи для управления задачами Todo.
    /// </summary>
    public class TodoTask
    {
        /// <summary>
        /// Уникальный идентификатор задачи.
        /// </summary>
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Название задачи.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Описание задачи.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Статус задачи (активна или выполнена).
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public TodoTaskStatus Status { get; set; } = TodoTaskStatus.Active;

        /// <summary>
        /// Дата и время создания задачи (в формате UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

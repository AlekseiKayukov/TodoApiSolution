namespace TodoApi.DTO
{
    /// <summary>
    /// Пагинированный результат для ответов API.
    /// Содержит метаданные пагинации и элементы текущей страницы.
    /// </summary>
    /// <typeparam name="T">Тип элементов в коллекции.</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Общее количество элементов без учёта пагинации.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Номер текущей страницы (начиная с 1).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Размер страницы (количество элементов на странице).
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Коллекция элементов текущей страницы.
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
    }
}
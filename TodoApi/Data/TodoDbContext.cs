using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data
{        
    /// <summary>
    /// Контекст базы данных для приложения TodoApi.
    /// Обеспечивает доступ к сущности <see cref="TodoTask"/>
    /// через коллекцию <see cref="Tasks"/>.
    /// </summary>
    public class TodoDbContext : DbContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TodoDbContext"/>
        /// с заданными параметрами конфигурации.
        /// </summary>
        /// <param name="options">Опции конфигурации контекста базы данных.</param>
        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options)
        { }

        /// <summary>
        /// Коллекция задач Todo для доступа и управления в базе данных.
        /// </summary>
        public DbSet<TodoTask> Tasks { get; set; } = null!;
    }
}
